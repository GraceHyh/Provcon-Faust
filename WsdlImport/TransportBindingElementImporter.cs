//
// TransportBindingElementImporter.cs
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

	public class TransportBindingElementImporter : IWsdlImportExtension, IPolicyImportExtension {
		#region IWsdlImportExtension implementation

		public void BeforeImport (WS.ServiceDescriptionCollection wsdlDocuments, XmlSchemaSet xmlSchemas,
		                          ICollection<XmlElement> policy)
		{
			Console.WriteLine ("TRANSPORT BEFORE IMPORT");
		}

		public void ImportContract (WsdlImporter importer, WsdlContractConversionContext contractContext)
		{
			Console.WriteLine ("TRANSPORT IMPORT CONTRACT");
		}

		public void ImportEndpoint (WsdlImporter importer, WsdlEndpointConversionContext context)
		{
			var endpoint = context.Endpoint;
			var binding = context.Endpoint.Binding;
			
			var qname = new QName (binding.Name, binding.Namespace);
			
			Console.WriteLine ("TRANSPORT IMPORT ENDPOINT: {0} {1} {2} {3}",
			                   binding, binding.Scheme, qname, context.WsdlPort != null);

			// Only import the binding, not the endpoint.
			if (context.WsdlPort == null)
				return;
			
			if (DoImportEndpoint (context)) {
				Console.WriteLine ("Successfully imported endpoint.");
				return;
			}
		}

		bool DoImportEndpoint (WsdlEndpointConversionContext context)
		{
			if (ImportBasicHttpEndpoint (context))
				return true;
			if (ImportNetTcpEndpoint (context))
				return true;
			return false;
		}

		bool ImportBasicHttpEndpoint (WsdlEndpointConversionContext context)
		{
			var http = context.Endpoint.Binding as BasicHttpBinding;
			if (http == null)
				return false;
			
			WS.SoapAddressBinding address = null;
			foreach (var extension in context.WsdlPort.Extensions) {
				var check = extension as WS.SoapAddressBinding;
				if (check != null) {
					address = check;
					break;
				}
			}
			
			if (address == null)
				return false;
			
			context.Endpoint.Address = new EndpointAddress (address.Location);
			context.Endpoint.ListenUri = new Uri (address.Location);
			context.Endpoint.ListenUriMode = ListenUriMode.Explicit;
			return true;
		}

		bool ImportNetTcpEndpoint (WsdlEndpointConversionContext context)
		{
			var tcp = context.Endpoint.Binding as NetTcpBinding;
			if (tcp == null)
				return false;

			WS.Soap12AddressBinding address = null;
			foreach (var extension in context.WsdlPort.Extensions) {
				var check = extension as WS.Soap12AddressBinding;
				if (check != null) {
					address = check;
					break;
				}
			}
			
			if (address == null)
				return false;
			
			context.Endpoint.Address = new EndpointAddress (address.Location);
			context.Endpoint.ListenUri = new Uri (address.Location);
			context.Endpoint.ListenUriMode = ListenUriMode.Explicit;
			return true;
		}

		#endregion

		#region IPolicyImportExtension implementation

		const string SecurityPolicyNS = "http://schemas.xmlsoap.org/ws/2005/07/securitypolicy";
		const string PolicyNS = "http://schemas.xmlsoap.org/ws/2004/09/policy";
		const string MimeSerializationNS = "http://schemas.xmlsoap.org/ws/2004/09/policy/optimizedmimeserialization";
		const string HttpAuthNS = "http://schemas.microsoft.com/ws/06/2004/policy/http";

		public void ImportPolicy (MetadataImporter importer, PolicyConversionContext context)
		{
			Console.WriteLine ("TRANSPORT IMPORT POLICY");

			try {
				HandleTransportBinding (context);
			} catch (Exception ex) {
				Console.WriteLine ("POLICY IMPORT ERROR: {0}", ex);
			}
		}

		static List<XmlElement> FindAssertionByNS (PolicyAssertionCollection collection, string ns)
		{
			var list = new List<XmlElement> ();
			foreach (var assertion in collection) {
				if (assertion.NamespaceURI.Equals (ns))
					list.Add (assertion);
			}
			return list;
		}

		bool HandleTransportBinding (PolicyConversionContext context)
		{
			var assertions = context.GetBindingAssertions ();
			Console.WriteLine (assertions.Count);
			
			var transport = assertions.Find ("TransportBinding", SecurityPolicyNS);
			if (transport == null)
				return false;
			
			var tokenList = transport.GetElementsByTagName ("TransportToken", SecurityPolicyNS);
			if (tokenList.Count != 1)
				return false;

			var token = (XmlElement)tokenList [0];
			Console.WriteLine ("TOKEN: {0}", token.InnerXml);

			var httpsList = token.GetElementsByTagName ("HttpsToken", SecurityPolicyNS);
			if (httpsList.Count != 1)
				return false;

			var https = (XmlElement)httpsList [0];
			Console.WriteLine ("HTTPS: {0}", https.OuterXml);

			var bindingElement = new HttpsTransportBindingElement ();
			context.BindingElements.Add (bindingElement);

			// MessageEncoding:
			var wsoma = transport.GetElementsByTagName ("OptimizedMimeSerialization", MimeSerializationNS);
			if (wsoma.Count > 0) {
				Console.WriteLine ("TEST");
			}

			// Auth:
			// binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
			// <http:NtlmAuthentication xmlns:http="http://schemas.microsoft.com/ws/06/2004/policy/http" />

			// Certificate:
			// <sp:HttpsToken RequireClientCertificate="true" />

			//  BasicHttpsSecurityMode.TransportWithMessageCredential adds:
			//      <sp:SignedSupportingTokens xmlns:sp="http://schemas.xmlsoap.org/ws/2005/07/securitypolicy">

			var authSchemes = AuthenticationSchemes.None;

			foreach (var assertion in assertions) {
				if (!assertion.NamespaceURI.Equals (HttpAuthNS))
					continue;
			}

			var authElements = FindAssertionByNS (assertions, HttpAuthNS);
			foreach (XmlElement authElement in authElements) {
				Console.WriteLine ("AUTH ELEMENT: {0} {1}", authElement.Name, authElement.OuterXml);
				assertions.Remove (authElement);
				switch (authElement.LocalName) {
				case "BasicAuthentication":
					authSchemes |= AuthenticationSchemes.Basic;
					break;
				case "NtlmAuthentication":
					authSchemes |= AuthenticationSchemes.Ntlm;
					break;
				case "DigestAuthentication":
					authSchemes |= AuthenticationSchemes.Digest;
					break;
				case "NegotiateAuthentication":
					authSchemes |= AuthenticationSchemes.Negotiate;
					break;
				default:
					Console.WriteLine ("UNKNOWN AUTH ELEMENT: {0}", authElement.OuterXml);
					break;
				}
			}

			bindingElement.AuthenticationScheme = authSchemes;

			Console.WriteLine ("Got HttpsTransportBindingElement!");
			return true;
		}

		#endregion
	}
}

