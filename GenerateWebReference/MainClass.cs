//
// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Schema;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Web.Services;
using System.Web.Services.Discovery;
using System.Runtime.Remoting.MetadataServices;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace TestWSDL2
{
	public class MainClass
	{
		const string DefaultURL = "http://provcon-faust/TestWCF/MyService.svc?singleWsdl";

		static int Main(string[] args)
		{
			var url = args.Length > 0 ? args[0] : DefaultURL;

			if (args.Length > 1)
			{
				using (TextWriter writer = File.CreateText(args[1]))
					Generate(url, writer);
			}
			else
			{
				Generate(url, Console.Out);
			}

			return 0;
		}

		static void Generate(string url, TextWriter writer)
		{
			var cr = new ContractReference();
			cr.Url = url;

			var protocol = new DiscoveryClientProtocol();

			var wc = new WebClient();
			using (var stream = wc.OpenRead(cr.Url))
				protocol.Documents.Add(cr.Url, cr.ReadDocument(stream));

			var mset = ToMetadataSet(protocol);

			var importer = new WsdlImporter(mset);
			var xsdImporter = new XsdDataContractImporter();
			var options = new ImportOptions();
			options.ReferencedCollectionTypes.Add(typeof(LinkedList<>));
			xsdImporter.Options = options;
			importer.State.Add(typeof(XsdDataContractImporter), xsdImporter);
	
			Collection<ContractDescription> contracts = importer.ImportAllContracts();

			CodeCompileUnit ccu = new CodeCompileUnit();
			CodeNamespace cns = new CodeNamespace("TestNamespace");
			ccu.Namespaces.Add(cns);

			var generator = new ServiceContractGenerator(ccu);

			foreach (var cd in contracts)
				generator.GenerateServiceContractType(cd);

			var provider = new CSharpCodeProvider();
			provider.GenerateCodeFromCompileUnit(ccu, writer, null);
		}

		static MetadataSet ToMetadataSet(DiscoveryClientProtocol prot)
		{
			MetadataSet metadata = new MetadataSet();
			foreach (object o in prot.Documents.Values)
			{
				if (o is System.Web.Services.Description.ServiceDescription)
				{
					metadata.MetadataSections.Add(
						new MetadataSection(MetadataSection.ServiceDescriptionDialect, "", (System.Web.Services.Description.ServiceDescription)o));
				}
				if (o is XmlSchema)
				{
					metadata.MetadataSections.Add(
						new MetadataSection(MetadataSection.XmlSchemaDialect, "", (XmlSchema)o));
				}
			}

			return metadata;
		}

	}
}

