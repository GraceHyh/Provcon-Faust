//
// MetadataProvider.cs
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
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Configuration;
using WS = System.Web.Services.Description;

namespace WsdlImport {

	public static class MetadataSamples  {

		internal const string HttpUri = "http://tempuri.org/TestHttp/";
		internal const string HttpsUri = "https://tempuri.org/TestHttps/";
		internal const string NetTcpUri = "net-tcp://tempuri.org:8000/TestNetTcp/";

		[MetadataSample ("http")]
		public static MetadataSet GetBasicHttpMetadata ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}

		[MetadataSample ("http2")]
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

		[MetadataSample ("http3")]
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
		
		[MetadataSample ("http4")]
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
		
		[MetadataSample ("http5")]
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
		[MetadataSample ("https")]
		public static MetadataSet GetBasicHttpsMetadata ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new BasicHttpsBinding (), new EndpointAddress (HttpsUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}
		
		[MetadataSample ("https2")]
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
		
		[MetadataSample ("https3")]
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
		
		[MetadataSample ("https4")]
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
		
		[MetadataSample]
		public static MetadataSet NetTcp ()
		{
			var exporter = new WsdlExporter ();
			
			var cd = new ContractDescription ("MyContract");
			
			exporter.ExportEndpoint (new ServiceEndpoint (
				cd, new NetTcpBinding (SecurityMode.None, false),
				new EndpointAddress (NetTcpUri)));
			
			var doc = exporter.GetGeneratedMetadata ();
			return doc;
		}
		
		[MetadataSample ("net-tcp2")]
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
		
		[MetadataSample ("net-tcp3")]
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
		
		[MetadataSample]
		public static MetadataSet NetTcp_TransferMode ()
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

		public static void Export ()
		{
			var bf = BindingFlags.Public | BindingFlags.Static;
			foreach (var method in typeof (MetadataSamples).GetMethods (bf)) {
				var cattr = method.GetCustomAttribute<MetadataSampleAttribute> ();
				if (cattr == null)
					continue;

				var name = cattr.Name ?? method.Name;
				var doc = (MetadataSet)method.Invoke (null, null);

				var filename = Path.Combine ("Resources", name + ".xml");
				Utils.Save (filename, doc);
			}
		}

		public static MetadataSet GetMetadataByName (string name)
		{
			if (name.EndsWith (".xml"))
				name = name.Substring (name.Length - 4);

			var bf = BindingFlags.Public | BindingFlags.Static;
			foreach (var method in typeof (MetadataSamples).GetMethods (bf)) {
				var cattr = method.GetCustomAttribute<MetadataSampleAttribute> ();
				if (cattr == null)
					continue;
				
				if (!name.Equals (cattr.Name ?? method.Name))
					continue;

				return (MetadataSet)method.Invoke (null, null);
			}

			throw new InvalidOperationException ();
		}

		public class MetadataSampleAttribute : Attribute {
			
			public MetadataSampleAttribute ()
			{
			}
			
			public MetadataSampleAttribute (string name)
			{
				Name = name;
			}
			
			public string Name {
				get; set;
			}
			
		}
	}
}
