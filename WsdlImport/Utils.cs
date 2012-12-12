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
using System.Xml.XPath;
using System.Reflection;
using System.Configuration;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using WS = System.Web.Services.Description;

namespace WsdlImport {

	public static class Utils {

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

		internal static string GetConfigElementName (Binding binding)
		{
			if (binding is BasicHttpBinding)
				return "basicHttpBinding";
#if NET_4_5
			else if (binding is BasicHttpsBinding)
				return "basicHttpsBinding";
#endif
			else if (binding is NetTcpBinding)
				return "netTcpBinding";
			else if (binding is CustomBinding)
				return "customBinding";
			else
				return null;
		}

		static IBindingConfigurationElement CreateConfigElement (Binding binding, string bindingName)
		{
			var name = binding.Name;

			switch (bindingName) {
			case "basicHttpBinding":
				return new BasicHttpBindingElement (name);
#if NET_4_5
			case "basicHttpsBinding":
				return new BasicHttpsBindingElement (name);
#endif
			case "netTcpBinding":
				return new NetTcpBindingElement (name);
			case "customBinding":
				return new CustomBindingElement (name);
			default:
				throw new InvalidOperationException ();
			}
		}

		static BindingCollectionElement CreateCollectionElement (
			string bindingName, IBindingConfigurationElement element)
		{
			switch (bindingName) {
			case "basicHttpBinding": {
				var http = new BasicHttpBindingCollectionElement ();
				http.Bindings.Add ((BasicHttpBindingElement)element);
				return http;
			}
#if NET_4_5
			case "basicHttpsBinding": {
				var https = new BasicHttpsBindingCollectionElement ();
				https.Bindings.Add ((BasicHttpsBindingElement)element);
				return https;
			}
#endif
			case "netTcpBinding": {
				var netTcp = new NetTcpBindingCollectionElement ();
				netTcp.Bindings.Add ((NetTcpBindingElement)element);
				return netTcp;
			}
			case "customBinding": {
				var custom = new CustomBindingCollectionElement ();
				custom.Bindings.Add ((CustomBindingElement)element);
				return custom;
			}
			default:
				throw new InvalidOperationException ();
			}
		}

		internal static void CreateConfig (Binding binding, string filename)
		{
			if (false && File.Exists (filename))
				File.Delete (filename);

			var element = new BasicHttpBindingElement ("Test");
			foreach (PropertyInformation prop in element.ElementInformation.Properties) {
				Console.WriteLine ("PROP: {0} {1}", prop.Name, prop.IsRequired);
			}
			
			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);

			Console.WriteLine ("CREATE CONFIG: {0}", binding.Name);

			var generator = new ServiceContractGenerator (config);

			string sectionName, configName;
			generator.GenerateBinding (binding, out sectionName, out configName);

			Console.WriteLine ("CONFIG: {0} {1} {2}", binding, sectionName, configName);

			config.Save (ConfigurationSaveMode.Minimal);
			Console.WriteLine ("CONFIG: {0}", config.FilePath);
			Dump (config.FilePath);
		}

		public static void NormalizeConfig (string filename)
		{
			var doc = new XmlDocument ();
			doc.Load (filename);
			var nav = doc.CreateNavigator ();
			
			var empty = new List<XPathNavigator> ();
			var iter = nav.Select ("/configuration/system.serviceModel/bindings/*");
			foreach (XPathNavigator node in iter) {
				if (!node.HasChildren && !node.HasAttributes && string.IsNullOrEmpty (node.Value))
					empty.Add (node);
			}
			foreach (var node in empty)
				node.DeleteSelf ();
			
			var settings = new XmlWriterSettings ();
			settings.Indent = true;
			settings.NewLineHandling = NewLineHandling.Replace;
			
			using (var writer = XmlWriter.Create (filename, settings)) {
				doc.WriteTo (writer);
			}
			Console.WriteLine ();
		}

		public static void Dump (string filename)
		{
			if (!File.Exists (filename)) {
				Console.WriteLine ("ERROR: File does not exist!");
				return;
			}
			using (var reader = new StreamReader (filename)) {
				Console.WriteLine (reader.ReadToEnd ());
				Console.WriteLine ();
			}
		}
	}
}
