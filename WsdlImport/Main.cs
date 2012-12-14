//
// Main.cs
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Discovery;
using Microsoft.CSharp;

using WS = System.Web.Services.Description;

using NUnit.Framework;
using Mono.Options;

using MonoTests.System.ServiceModel.MetadataTests;

namespace WsdlImport {

	class Program {
		enum Mode {
			Default,
			Export,
			Server,
			Client
		}

		static void Main (string[] args)
		{
			string cache = null;
			Mode mode = Mode.Default;
			var options = new OptionSet ();
			options.Add ("mode=", m => mode = (Mode)Enum.Parse (typeof (Mode), m, true));
			options.Add ("cache=", c => cache = c);
			options.Parse (args);

			switch (mode) {
			case Mode.Export:
				MetadataSamples.Export ("metadata");
				return;

			case Mode.Server:
				Server.Run ();
				return;

			case Mode.Client:
				Client.Run (new Uri ("http://provcon-faust:9999/?singleWsdl"), cache);
				return;

			default:
				ConfigTest.Run ();
				// ConfigTest.Run ("my.config", "my2.config");
				// TestConfig ();
				return;
			}
		}

		static void TestConfig ()
		{
			var test = new ImportTests_CreateMetadata ();
			test.BasicHttp_Config ();
			test.BasicHttp_Config2 ();
		}

		static void TestSvcUtil (string filename)
		{
			var sd = WS.ServiceDescription.Read (filename);
			MetadataSet metadata = new MetadataSet ();
			metadata.MetadataSections.Add (new MetadataSection ("http://schemas.xmlsoap.org/wsdl/", "Test", sd));

			WsdlImporter importer = new WsdlImporter (metadata);

			var endpoints = importer.ImportAllEndpoints ();
			var contracts = importer.ImportAllContracts ();

			var code_provider = new Microsoft.CSharp.CSharpCodeProvider ();

			var ccu = new CodeCompileUnit ();
			var cns = new CodeNamespace ("TestNamespace");
			ccu.Namespaces.Add (cns);

			if (File.Exists ("output.cs"))
				File.Delete ("output.cs");
			if (File.Exists ("test.config"))
				File.Delete ("test.config");

			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = "test.config";
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);
			
			var generator = new ServiceContractGenerator (ccu, config);
			generator.Options = ServiceContractGenerationOptions.None;

			foreach (ContractDescription cd in contracts) {
				// generator.GenerateServiceContractType (cd);
			}

			using (TextWriter w = File.CreateText ("output.cs")) {
				code_provider.GenerateCodeFromCompileUnit (ccu, w, null);
			}

			foreach (var endpoint in endpoints) {
				ChannelEndpointElement channelElement;
				generator.GenerateServiceEndpoint (endpoint, out channelElement);

				// string sectionName, configName;
				//generator.GenerateBinding (endpoint.Binding, out sectionName, out configName);
			}

			config.Save (ConfigurationSaveMode.Minimal);
		}

		static void CheckConfig ()
		{
			var doc = new XPathDocument ("test2.config");
			var nav = doc.CreateNavigator ();

			var iter = nav.Select ("/configuration/system.serviceModel/bindings/*");
			Console.WriteLine ("SELECT: {0} - {1} {2}", iter, iter.Count, iter.Current);
			foreach (XPathNavigator binding in iter) {
				Console.WriteLine ("TEST: {0} - {1} {2} {3} {4}", binding.GetType (),
				                   binding.HasChildren, binding.OuterXml, binding.Value,
				                   binding.HasAttributes);
			}
		}

		static void TestConfigSection ()
		{
			var filename = "test.config";
			if (File.Exists (filename))
				File.Delete (filename);
			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);
			
			var section = (BindingsSection)config.GetSection ("system.serviceModel/bindings");
			var element = new BasicHttpBindingElement ("Test");
			section.BasicHttpBinding.Bindings.Add (element);
			
			config.Save (ConfigurationSaveMode.Minimal);

			Console.WriteLine ("TEST: {0}", config.FilePath);

			Utils.Dump (filename);
		}
	}
}
