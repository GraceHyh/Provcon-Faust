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

		public static MetadataSet GetBasicHttpMetadata2 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");

			var binding = new BasicHttpBinding ();
			binding.Security.Mode = BasicHttpSecurityMode.Transport;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetBasicHttpMetadata3 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding ();
			binding.Security.Mode = BasicHttpSecurityMode.Message;
			binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetBasicHttpMetadata4 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding ();
			binding.Security.Mode = BasicHttpSecurityMode.TransportWithMessageCredential;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetBasicHttpMetadata5 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpBinding ();
			binding.MessageEncoding = WSMessageEncoding.Mtom;

			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}
		
#if NET_4_5
		public static MetadataSet GetBasicHttpsMetadata ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpsBinding (), new EndpointAddress (HttpsUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetBasicHttpsMetadata2 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");

			var binding = new BasicHttpsBinding ();

			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;

			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpsUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetBasicHttpsMetadata3 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpsBinding ();
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpsUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetBasicHttpsMetadata4 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new BasicHttpsBinding (BasicHttpsSecurityMode.TransportWithMessageCredential);

			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (HttpsUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}
#endif

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

		public static MetadataSet GetNetTcpMetadata2 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.Transport, false),
				new EndpointAddress (NetTcpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		public static MetadataSet GetNetTcpMetadata3 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");

			var binding = new NetTcpBinding (SecurityMode.None, true);
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (NetTcpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}
		
		public static MetadataSet GetNetTcpMetadata4 ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			var binding = new NetTcpBinding (SecurityMode.None, false);
			binding.TransferMode = TransferMode.Streamed;
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, binding, new EndpointAddress (NetTcpUri)));
			
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

		static readonly string[] MetadataFiles = {
			"http.xml", "http2.xml", "http3.xml", "http4.xml", "http5.xml",
#if NET_4_5
			"https.xml", "https2.xml", "https3.xml", "https4.xml",
#endif
			"net-tcp.xml", "net-tcp2.xml", "net-tcp3.xml", "net-tcp4.xml"
		};

		internal static void SaveMetadata ()
		{
			foreach (var name in MetadataFiles)
				Save (name);
			Console.WriteLine ("Metadata saved.");
		}

		static void Save (string name)
		{
			Save (name, DefaultMetadataProvider.Get (name));
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
				switch (name) {
				case "http.xml":
					return Utils.GetBasicHttpMetadata ();
				case "http2.xml":
					return Utils.GetBasicHttpMetadata2 ();
				case "http3.xml":
					return Utils.GetBasicHttpMetadata3 ();
				case "http4.xml":
					return Utils.GetBasicHttpMetadata4 ();
				case "http5.xml":
					return Utils.GetBasicHttpMetadata5 ();
#if NET_4_5
				case "https.xml":
					return Utils.GetBasicHttpsMetadata ();
				case "https2.xml":
					return Utils.GetBasicHttpsMetadata2 ();
				case "https3.xml":
					return Utils.GetBasicHttpsMetadata3 ();
				case "https4.xml":
					return Utils.GetBasicHttpsMetadata4 ();
#endif
				case "net-tcp.xml":
					return Utils.GetNetTcpMetadata ();
				case "net-tcp2.xml":
					return Utils.GetNetTcpMetadata2 ();
				case "net-tcp3.xml":
					return Utils.GetNetTcpMetadata3 ();
				case "net-tcp4.xml":
					return Utils.GetNetTcpMetadata4 ();
				default:
					throw new ArgumentException ("No such metadata.");
				}
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
