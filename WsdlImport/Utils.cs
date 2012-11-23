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

		public static MetadataSet LoadFromResource (string name)
		{
			var asm = Assembly.GetExecutingAssembly ();
			var resname = "WsdlImport.Resources." + name;
			using (var stream = asm.GetManifestResourceStream (resname)) {
				var reader = new XmlTextReader (stream);
				return MetadataSet.ReadFrom (reader);
			}
		}

		public static IMetadataProvider EmbeddedResourceProvider = new _EmbeddedResourceProvider ();
		public static IMetadataProvider DefaultMetadataProvider = new _DefaultMetadataProvider ();

		class _EmbeddedResourceProvider : IMetadataProvider {
			public MetadataSet Get (string name)
			{
				return Utils.LoadFromResource (name);
			}
		}

		class _DefaultMetadataProvider : IMetadataProvider {
			public MetadataSet Get (string name)
			{
				return MetadataSamples.GetMetadataByName (name);
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

		static void CreateConfig_NET (Binding binding, string filename)
		{
			if (File.Exists (filename))
				File.Delete (filename);
			
			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);

			var generator = new ServiceContractGenerator (config);

			string sectionName, configName;
			generator.GenerateBinding (binding, out sectionName, out configName);

			Console.WriteLine ("CONFIG: {0} {1} {2}", binding, sectionName, configName);

			config.Save ();
		}

		public static void CreateConfig (Binding binding, string filename)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				// Use the real API on Windows.
				CreateConfig_NET (binding, filename);
				return;
			}

			// The reflection stuff below uses private Mono APIs.

			var configName = GetConfigElementName (binding);
			if (configName == null)
				throw new InvalidOperationException ();
			
			var element = CreateConfigElement (binding, configName);
			if (element == null)
				return;

			var init = element.GetType ().GetMethod (
				"InitializeFrom", BindingFlags.Instance | BindingFlags.NonPublic);
			init.Invoke (element, new object[] { binding });

			var collectionElement = CreateCollectionElement (configName, element);

			// FIXME: Mono bug
			filename = Path.GetFullPath (filename);

			if (File.Exists (filename))
				File.Delete (filename);

			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);

			Console.WriteLine ("CREATE CONFIG: {0} {1}", config, binding);

			var section = (BindingsSection)config.GetSection ("system.serviceModel/bindings");

			var method = section.GetType ().GetMethod (
				"set_Item", BindingFlags.Instance | BindingFlags.NonPublic, null,
				new Type [] { typeof (string), typeof (object) }, null);
			method.Invoke (section, new object[] { configName, collectionElement });

			config.Save ();
		}
	}
}
