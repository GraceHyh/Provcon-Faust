//
// Utils.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using WS = System.Web.Services.Description;

namespace WsdlImport {

	public static class Utils {

		internal const string HttpUri = "http://tempuri.org/TestHttp/";
		internal const string HttpsUri = "https://tempuri.org/TestHttps/";
		internal const string NetTcpUri = "net-tcp://tempuri.org:8000/TestNetTcp/";

		public static MetadataSet GetBasicHttpMetadata ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}
		
		public static MetadataSet GetBasicHttpsMetadata ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpsBinding (), new EndpointAddress (HttpsUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetNetTcpMetadata ()
		{
			var exporter = new WsdlExporter ();

			var cd = new ContractDescription ("MyContract");

			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.None, false),
				new EndpointAddress (NetTcpUri)));

			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}
		
		public static void Save (string filename, WS.ServiceDescription service)
		{
			using (var file = new StreamWriter (filename, false)) {
				var writer = new XmlTextWriter (file);
				writer.Formatting = Formatting.Indented;
				service.Write (writer);
			}
		}

		public static void Save (string filename, MetadataSet metadata)
		{
			using (var file = new StreamWriter (filename, false)) {
				var writer = new XmlTextWriter (file);
				writer.Formatting = Formatting.Indented;
				metadata.WriteTo (writer);
			}
		}

		public static MetadataSet Load (string filename)
		{
			using (var file = new StreamReader (filename)) {
				var reader = new XmlTextReader (file);
				return MetadataSet.ReadFrom (reader);
			}
		}

		public static MetadataSet LoadFromResource (string name)
		{
			var asm = Assembly.GetExecutingAssembly ();
			var resname = "WsdlImport.Resources." + name;
			using (var stream = asm.GetManifestResourceStream (resname)) {
				var reader = new XmlTextReader (stream);
				return MetadataSet.ReadFrom (reader);
			}
		}

		public static MetadataSet LoadBasicHttpMetadata ()
		{
			return LoadFromResource ("http.xml");
		}

		public static MetadataSet LoadBasicHttpsMetadata ()
		{
			return LoadFromResource ("https.xml");
		}

		public static MetadataSet LoadNetTcpMetadata ()
		{
			return LoadFromResource ("net-tcp.xml");
		}

		internal static void SaveMetadata ()
		{
			var basicHttp = GetBasicHttpMetadata ();
			Utils.Save ("http.xml", basicHttp);
			
			var basicHttps = GetBasicHttpsMetadata ();
			Utils.Save ("https.xml", basicHttps);

			var netTcp = GetNetTcpMetadata ();
			Utils.Save ("net-tcp.xml", netTcp);
			
			Console.WriteLine ("Metadata saved.");
		}

		internal static WsdlImporter GetCustomImporter (MetadataSet doc)
		{
			// Don't use any of .NET's default importers, only our own.
			var wsdlExtensions = new List<IWsdlImportExtension> ();
			wsdlExtensions.Add (new StandardBindingImporter ());

			// Don't use any of .NET's default policy importers.
			var policyExtensions = new List<IPolicyImportExtension> ();
			
			return new WsdlImporter (doc, policyExtensions, wsdlExtensions);
		}
	}
}
