//
// StandardBindingImporter.cs
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
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using WS = System.Web.Services.Description;
using QName = System.Xml.XmlQualifiedName;

namespace WsdlImport {

	public class StandardBindingImporter : IWsdlImportExtension {
		#region IWsdlImportExtension implementation

		public void BeforeImport (WS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas,
		                          ICollection<XmlElement> policy)
		{
			Console.WriteLine ("STANDARD BEFORE IMPORT");
		}

		public void ImportContract (WsdlImporter importer, WsdlContractConversionContext contractContext)
		{
			Console.WriteLine ("STANDARD IMPORT CONTRACT");
		}

		WS.Port LookupPort (WsdlImporter importer, QName name)
		{
			foreach (WS.ServiceDescription doc in importer.WsdlDocuments) {
				foreach (WS.Service service in doc.Services) {
					foreach (WS.Port port in service.Ports) {
						if (!name.Namespace.Equals (port.Binding.Namespace))
							continue;
						if (!name.Name.Equals (port.Binding.Name))
							continue;
						return port;
					}
				}
			}

			return null;
		}

		public void ImportEndpoint (WsdlImporter importer, WsdlEndpointConversionContext context)
		{
			var endpoint = context.Endpoint;
			var binding = context.Endpoint.Binding;

			var qname = new QName (binding.Name, binding.Namespace);

			Console.WriteLine ("STANDARD IMPORT ENDPOINT: {0} {1} {2} {3}",
			                   binding, binding.Scheme, qname, context.WsdlPort != null);

			if (!(binding is CustomBinding))
				return;

			if (DoImportBinding (context)) {
				Console.WriteLine ("Successfully imported binding.");
			}
		}

		bool DoImportBinding (WsdlEndpointConversionContext context)
		{
			if (ImportBasicHttpBinding (context))
				return true;
			if (ImportNetTcpBinding (context))
				return true;
			return false;
		}

		bool ImportBasicHttpBinding (WsdlEndpointConversionContext context)
		{
			var custom = context.Endpoint.Binding as CustomBinding;
			if (custom == null)
				return false;
			
			WS.SoapBinding soap = null;

			foreach (var extension in context.WsdlBinding.Extensions) {
				var check = extension as WS.SoapBinding;
				if (check != null) {
					soap = check;
					break;
				}
			}

			if (soap == null)
				return false;
			if (soap.Transport != WS.SoapBinding.HttpTransport)
				return false;
			if (soap.Style != WS.SoapBindingStyle.Document)
				return false;

			// Ok, we have a match.
			Console.WriteLine ("Found http binding!");

			TransportBindingElement transportElement = null;

			foreach (var element in custom.Elements) {
				var check = element as TransportBindingElement;
				if (check != null) {
					transportElement = check;
					break;
				}
			}

			BasicHttpBinding httpBinding;
			AuthenticationSchemes authScheme;

			/*
			 * FIXME: Maybe make the BasicHttpBinding use the transport element
			 * that we created with the TransportBindingElementImporter ?
			 * 
			 * There seems to be no public API to do that, so maybe add a private .ctor ?
			 * 
			 */

			var httpsTransport = transportElement as HttpsTransportBindingElement;
			var httpTransport = transportElement as HttpTransportBindingElement;

			if (httpsTransport != null) {
				httpBinding = new BasicHttpBinding (BasicHttpSecurityMode.Transport);
				authScheme = httpsTransport.AuthenticationScheme;
			} else if (httpTransport != null) {
				httpBinding = new BasicHttpBinding ();
				authScheme = httpTransport.AuthenticationScheme;
			} else {
				httpBinding = new BasicHttpBinding ();
				authScheme = AuthenticationSchemes.Anonymous;
			}

			httpBinding.Name = context.Endpoint.Binding.Name;
			httpBinding.Namespace = context.Endpoint.Binding.Namespace;

			switch (authScheme) {
			case AuthenticationSchemes.None:
			case AuthenticationSchemes.Anonymous:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
				break;
			case AuthenticationSchemes.Basic:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
				break;
			case AuthenticationSchemes.Digest:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Digest;
				break;
			case AuthenticationSchemes.Ntlm:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
				break;
			case AuthenticationSchemes.Negotiate:
				httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
				break;
			default:
				Console.WriteLine ("Invalid auth scheme: {0}", authScheme);
				return false;
			}

			context.Endpoint.Binding = httpBinding;
			return true;
		}

		const string TcpTransport = "http://schemas.microsoft.com/soap/tcp";

		bool ImportNetTcpBinding (WsdlEndpointConversionContext context)
		{
			WS.Soap12Binding soap = null;
			foreach (var extension in context.WsdlBinding.Extensions) {
				Console.WriteLine (extension);
				if (extension is WS.Soap12Binding) {
					soap = (WS.Soap12Binding)extension;
					break;
				}
			}

			if (soap == null)
				return false;
			if (soap.Transport != TcpTransport)
				return false;
			if (soap.Style != WS.SoapBindingStyle.Document)
				return false;

			// Ok, we have a match.
			Console.WriteLine ("Found net.tcp binding!");

			var netTcp = new NetTcpBinding (SecurityMode.None);

			netTcp.Name = context.Endpoint.Binding.Name;
			netTcp.Namespace = context.Endpoint.Binding.Namespace;

			context.Endpoint.Binding = netTcp;
			return true;
		}

		#endregion
	}
}

