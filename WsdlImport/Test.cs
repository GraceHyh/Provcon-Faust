//
// Test.cs
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
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

using WS = System.Web.Services.Description;

namespace WsdlImport {

	[TestFixture]
	public class Test {

		const string WspNamespace = "http://schemas.xmlsoap.org/ws/2004/09/policy";

		public IMetadataProvider MetadataProvider {
			get;
			set;
		}

		public Test ()
			: this (Utils.EmbeddedResourceProvider)
		{
		}

		public Test (IMetadataProvider metadata)
		{
			MetadataProvider = metadata;
		}

		void CheckImportErrors (WsdlImporter importer, TestLabel label)
		{
			label.EnterScope ("import-errors");

			bool foundErrors = false;
			var myLabel = label.Get ();
			foreach (var error in importer.Errors) {
				if (error.IsWarning)
					Console.WriteLine ("WARNING ({0}): {1}", myLabel, error.Message);
				else {
					Console.WriteLine ("ERROR ({0}): {1}", myLabel, error.Message);
					foundErrors = true;
				}
			}

			if (foundErrors)
				Assert.Fail ("Found import errors", myLabel);
			label.LeaveScope ();
		}

		void CheckSoapBinding (object extension, string transport, TestLabel label)
		{
			label.EnterScope ("soap");
			Assert.That (extension, Is.InstanceOfType (typeof (WS.SoapBinding)), label.Get ());
			var soap = (WS.SoapBinding)extension;
			Assert.That (soap.Style, Is.EqualTo (WS.SoapBindingStyle.Document), label.Get ());
			Assert.That (soap.Transport, Is.EqualTo (transport), label.Get ());
			Assert.That (soap.Required, Is.False, label.Get ());
			label.LeaveScope ();
		}

		void CheckBasicHttpBinding (Binding binding, string scheme, BasicHttpSecurityMode security,
		                            WSMessageEncoding encoding, HttpClientCredentialType clientCred,
		                            AuthenticationSchemes authScheme, TestLabel label)
		{
			label.EnterScope ("http");

			if (security == BasicHttpSecurityMode.Message) {
				Assert.That (binding, Is.InstanceOfType (typeof(CustomBinding)), label.Get ());
			} else {
				Assert.That (binding, Is.InstanceOfType (typeof(BasicHttpBinding)), label.Get ());
				var basicHttp = (BasicHttpBinding)binding;
				Assert.That (basicHttp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap11), label.Get ());
				Assert.That (basicHttp.MessageVersion, Is.EqualTo (MessageVersion.Soap11), label.Get ());
				Assert.That (basicHttp.Scheme, Is.EqualTo (scheme), label.Get ());
				Assert.That (basicHttp.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());
				Assert.That (basicHttp.MessageEncoding, Is.EqualTo (encoding), label.Get ());
				Assert.That (basicHttp.Security, Is.Not.Null, label.Get ());
				Assert.That (basicHttp.Security.Mode, Is.EqualTo (security), label.Get ());
				Assert.That (basicHttp.Security.Transport.ClientCredentialType, Is.EqualTo (clientCred), label.Get ());
			}

			label.EnterScope ("elements");

			var elements = binding.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label.Get ());
			if ((security == BasicHttpSecurityMode.Message) ||
				(security == BasicHttpSecurityMode.TransportWithMessageCredential))
				Assert.That (elements.Count, Is.EqualTo (3), label.Get ());
			else
				Assert.That (elements.Count, Is.EqualTo (2), label.Get ());
			
			TextMessageEncodingBindingElement textElement = null;
			TransportSecurityBindingElement securityElement = null;
			HttpTransportBindingElement transportElement = null;
			AsymmetricSecurityBindingElement asymmSecurityElement = null;
			MtomMessageEncodingBindingElement mtomElement = null;
			
			foreach (var element in elements) {
				if (element is TextMessageEncodingBindingElement)
					textElement = (TextMessageEncodingBindingElement)element;
				else if (element is HttpTransportBindingElement)
					transportElement = (HttpTransportBindingElement)element;
				else if (element is TransportSecurityBindingElement)
					securityElement = (TransportSecurityBindingElement)element;
				else if (element is AsymmetricSecurityBindingElement)
					asymmSecurityElement = (AsymmetricSecurityBindingElement)element;
				else if (element is MtomMessageEncodingBindingElement)
					mtomElement = (MtomMessageEncodingBindingElement)element;
				else
					Assert.Fail (string.Format (
						"Unknown element: {0}", element.GetType ()), label.Get ());
			}

			label.EnterScope ("text");
			if (encoding == WSMessageEncoding.Text) {
				Assert.That (textElement, Is.Not.Null, label.Get ());
				Assert.That (textElement.WriteEncoding, Is.InstanceOfType (typeof(UTF8Encoding)), label.Get ());
			} else {
				Assert.That (textElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("mtom");
			if (encoding == WSMessageEncoding.Mtom) {
				Assert.That (mtomElement, Is.Not.Null, label.Get ());
			} else {
				Assert.That (mtomElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("security");
			if (security == BasicHttpSecurityMode.TransportWithMessageCredential) {
				Assert.That (securityElement, Is.Not.Null, label.Get ());
				Assert.That (securityElement.SecurityHeaderLayout,
				             Is.EqualTo (SecurityHeaderLayout.Lax), label.Get ());
			} else {
				Assert.That (securityElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("asymmetric");
			if (security == BasicHttpSecurityMode.Message) {
				Assert.That (asymmSecurityElement, Is.Not.Null, label.Get ());
			} else {
				Assert.That (asymmSecurityElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("transport");
			Assert.That (transportElement, Is.Not.Null, label.Get ());
			
			Assert.That (transportElement.Realm, Is.Empty, label.Get ());
			Assert.That (transportElement.Scheme, Is.EqualTo (scheme), label.Get ());
			Assert.That (transportElement.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());

			label.EnterScope ("auth");
			Assert.That (transportElement.AuthenticationScheme, Is.EqualTo (authScheme), label.Get ());
			label.LeaveScope (); // auth
			label.LeaveScope (); // transport
			label.LeaveScope (); // elements
			label.LeaveScope (); // http
		}

		void CheckEndpoint (ServiceEndpoint endpoint, string uri, TestLabel label)
		{
			label.EnterScope ("endpoint");
			Assert.That (endpoint.ListenUri, Is.EqualTo (new Uri (uri)), label.Get ());
			Assert.That (endpoint.ListenUriMode, Is.EqualTo (ListenUriMode.Explicit), label.Get ());
			Assert.That (endpoint.Contract, Is.Not.Null, label.Get ());
			Assert.That (endpoint.Contract.Name, Is.EqualTo ("MyContract"), label.Get ());
			Assert.That (endpoint.Address, Is.Not.Null, label.Get ());
			Assert.That (endpoint.Address.Uri, Is.EqualTo (new Uri (uri)), label.Get ());
			Assert.That (endpoint.Address.Identity, Is.Null, label.Get ());
			Assert.That (endpoint.Address.Headers, Is.Not.Null, label.Get ());
			Assert.That (endpoint.Address.Headers.Count, Is.EqualTo (0), label.Get ());
			label.LeaveScope ();
		}

		[Test]
		public void BasicHttpBinding ()
		{
			var doc = MetadataProvider.Get ("http.xml");

			var label = new TestLabel ("BasicHttpBinding");
			BasicHttpBinding (doc, BasicHttpSecurityMode.None, label);
		}

		[Test]
		public void BasicHttpBinding2 ()
		{
			var doc = MetadataProvider.Get ("http2.xml");
			
			var label = new TestLabel ("BasicHttpBinding2");
			BasicHttpBinding (doc, BasicHttpSecurityMode.Transport, label);
		}

		[Test]
		public void BasicHttpBinding3 ()
		{
			var doc = MetadataProvider.Get ("http3.xml");
			
			var label = new TestLabel ("BasicHttpBinding3");
			BasicHttpBinding (doc, BasicHttpSecurityMode.Message, label);
		}

		[Test]
		public void BasicHttpBinding4 ()
		{
			var doc = MetadataProvider.Get ("http4.xml");
			
			var label = new TestLabel ("BasicHttpBinding4");
			BasicHttpBinding (doc, BasicHttpSecurityMode.TransportWithMessageCredential, label);
		}

		[Test]
		public void BasicHttpBinding5 ()
		{
			var doc = MetadataProvider.Get ("http5.xml");
			
			var label = new TestLabel ("BasicHttpBinding5");
			BasicHttpBinding (doc, WSMessageEncoding.Mtom, label);
		}

		public void BasicHttpBinding (MetadataSet doc, WSMessageEncoding encoding,
		                              TestLabel label)
		{
			BasicHttpBinding (doc, BasicHttpSecurityMode.None, encoding, label);
		}

		public void BasicHttpBinding (MetadataSet doc, BasicHttpSecurityMode security,
		                              TestLabel label)
		{
			BasicHttpBinding (doc, security, WSMessageEncoding.Text, label);
		}
		
		void BasicHttpBinding (MetadataSet doc, BasicHttpSecurityMode security,
		                       WSMessageEncoding encoding, TestLabel label)
		{
			label.EnterScope ("basicHttpBinding");

			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");
			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());

			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());

			switch (security) {
			case BasicHttpSecurityMode.None:
				if (encoding == WSMessageEncoding.Mtom)
					Assert.That (binding.Extensions.Count, Is.EqualTo (2), label.Get ());
				else
					Assert.That (binding.Extensions.Count, Is.EqualTo (1), label.Get ());
				break;
			case BasicHttpSecurityMode.Message:
			case BasicHttpSecurityMode.Transport:
			case BasicHttpSecurityMode.TransportWithMessageCredential:
				if (encoding == WSMessageEncoding.Mtom)
					throw new InvalidOperationException ();
				Assert.That (binding.Extensions.Count, Is.EqualTo (2), label.Get ());
				break;
			default:
				throw new InvalidOperationException ();
			}
			label.LeaveScope ();

			WS.SoapBinding soap = null;
			XmlElement xml = null;

			foreach (var ext in binding.Extensions) {
				if (ext is WS.SoapBinding)
					soap = (WS.SoapBinding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}

			CheckSoapBinding (soap, WS.SoapBinding.HttpTransport, label);
			label.LeaveScope ();

			if (security != BasicHttpSecurityMode.None) {
				label.EnterScope ("policy-xml");
				
				Assert.That (xml, Is.Not.Null, label.Get ());
				
				Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
				Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference"), label.Get ());
				
				label.LeaveScope ();
			}

			var importer = new WsdlImporter (doc);

			label.EnterScope ("bindings");
			var bindings = importer.ImportAllBindings ();
			CheckImportErrors (importer, label);

			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());

			string scheme;
			if ((security == BasicHttpSecurityMode.Transport) ||
			    (security == BasicHttpSecurityMode.TransportWithMessageCredential))
				scheme = "https";
			else
				scheme = "http";

			CheckBasicHttpBinding (
				bindings [0], scheme, security, encoding,
				HttpClientCredentialType.None, AuthenticationSchemes.Anonymous,
				label);
			label.LeaveScope ();

			label.EnterScope ("endpoints");
			var endpoints = importer.ImportAllEndpoints ();
			CheckImportErrors (importer, label);

			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());

			CheckEndpoint (endpoints [0], MetadataSamples.HttpUri, label);
			label.LeaveScope ();

			label.LeaveScope ();
		}

		[Test]
		public void BasicHttpsBinding ()
		{
			var doc = MetadataProvider.Get ("https.xml");
			var label = new TestLabel ("BasicHttpsBinding");
			var binding = BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.None, AuthenticationSchemes.Anonymous,
				label);
		}

		[Test]
		public void BasicHttpsBinding2 ()
		{
			var doc = MetadataProvider.Get ("https2.xml");
			var label = new TestLabel ("BasicHttpsBinding2");
			var binding = BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Ntlm, AuthenticationSchemes.Ntlm,
				label);
		}

		[Test]
		public void BasicHttpsBinding3 ()
		{
			var doc = MetadataProvider.Get ("https3.xml");
			var label = new TestLabel ("BasicHttpsBinding3");
			BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Certificate, AuthenticationSchemes.Anonymous,
				label);
		}

		[Test]
		public void BasicHttpsBinding4 ()
		{
			var doc = MetadataProvider.Get ("https4.xml");
			var label = new TestLabel ("BasicHttpsBinding4");
			BasicHttpsBinding (
				doc, BasicHttpSecurityMode.TransportWithMessageCredential,
				WSMessageEncoding.Text, HttpClientCredentialType.None,
				AuthenticationSchemes.Anonymous, label);
		}
		
		Binding BasicHttpsBinding (MetadataSet doc, BasicHttpSecurityMode security,
		                           WSMessageEncoding encoding, HttpClientCredentialType clientCred,
		                           AuthenticationSchemes authScheme, TestLabel label)
		{
			label.EnterScope ("basicHttpsBinding");

			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");

			Assert.That (sd.Extensions, Is.Not.Null, label.Get ());
			Assert.That (sd.Extensions.Count, Is.EqualTo (1), label.Get ());
			Assert.That (sd.Extensions [0], Is.InstanceOfType (typeof (XmlElement)), label.Get ());

			label.EnterScope ("extensions");
			var extension = (XmlElement)sd.Extensions [0];
			Assert.That (extension.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (extension.LocalName, Is.EqualTo ("Policy"), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());
			Assert.That (binding.Extensions.Count, Is.EqualTo (2), label.Get ());
			label.LeaveScope ();

			WS.SoapBinding soap = null;
			XmlElement xml = null;

			foreach (var ext in binding.Extensions) {
				if (ext is WS.SoapBinding)
					soap = (WS.SoapBinding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}

			CheckSoapBinding (soap, WS.SoapBinding.HttpTransport, label);

			label.EnterScope ("policy-xml");

			Assert.That (xml, Is.Not.Null, label.Get ());
			
			Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference"), label.Get ());

			label.LeaveScope ();
			label.LeaveScope (); // wsdl

			var importer = new WsdlImporter (doc);

			label.EnterScope ("bindings");
			var bindings = importer.ImportAllBindings ();
			CheckImportErrors (importer, label);
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());

			CheckBasicHttpBinding (
				bindings [0], "https", security, encoding,
				clientCred, authScheme, label);
			label.LeaveScope ();

			label.EnterScope ("endpoints");
			var endpoints = importer.ImportAllEndpoints ();
			CheckImportErrors (importer, label);
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());
			
			CheckEndpoint (endpoints [0], MetadataSamples.HttpsUri, label);
			label.LeaveScope ();

			label.LeaveScope ();
			return bindings [0];
		}

		void CheckNetTcpBinding (Binding binding, SecurityMode security,
		                         bool reliableSession, TransferMode transferMode,
		                         TestLabel label)
		{
			label.EnterScope ("net-tcp");
			Assert.That (binding, Is.InstanceOfType (typeof(NetTcpBinding)), label.Get ());
			var netTcp = (NetTcpBinding)binding;
			Assert.That (netTcp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap12), label.Get ());
			Assert.That (netTcp.MessageVersion, Is.EqualTo (MessageVersion.Soap12WSAddressing10), label.Get ());
			Assert.That (netTcp.Scheme, Is.EqualTo ("net.tcp"), label.Get ());
			Assert.That (netTcp.TransferMode, Is.EqualTo (transferMode), label.Get ());
			Assert.That (netTcp.Security, Is.Not.Null, label.Get ());
			Assert.That (netTcp.Security.Mode, Is.EqualTo (security), label.Get ());

			label.EnterScope ("elements");
			
			var elements = netTcp.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label.Get ());

			TcpTransportBindingElement transportElement = null;
			TransactionFlowBindingElement transactionFlowElement = null;
			BinaryMessageEncodingBindingElement encodingElement = null;
			WindowsStreamSecurityBindingElement windowsStreamElement = null;
			ReliableSessionBindingElement reliableSessionElement = null;
			
			foreach (var element in elements) {
				if (element is TcpTransportBindingElement)
					transportElement = (TcpTransportBindingElement)element;
				else if (element is TransactionFlowBindingElement)
					transactionFlowElement = (TransactionFlowBindingElement)element;
				else if (element is BinaryMessageEncodingBindingElement)
					encodingElement = (BinaryMessageEncodingBindingElement)element;
				else if (element is WindowsStreamSecurityBindingElement)
					windowsStreamElement = (WindowsStreamSecurityBindingElement)element;
				else if (element is ReliableSessionBindingElement)
					reliableSessionElement = (ReliableSessionBindingElement)element;
				else
					Assert.Fail (string.Format (
						"Unknown element `{0}'.", element.GetType ()), label.Get ());
			}

			label.EnterScope ("windows-stream");
			if (security == SecurityMode.Transport) {
				Assert.That (windowsStreamElement, Is.Not.Null, label.Get ());
			} else {
				Assert.That (windowsStreamElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("reliable-session");
			if (reliableSession) {
				Assert.That (reliableSessionElement, Is.Not.Null, label.Get ());
			} else {
				Assert.That (reliableSessionElement, Is.Null, label.Get ());
			}
			label.LeaveScope ();

			label.EnterScope ("encoding");
			Assert.That (encodingElement, Is.Not.Null, label.Get ());
			label.LeaveScope ();

			label.EnterScope ("transaction");
			Assert.That (transactionFlowElement, Is.Not.Null, label + "m");
			label.LeaveScope ();

			label.EnterScope ("transport");
			Assert.That (transportElement, Is.Not.Null, label.Get ());

			Assert.That (transportElement.Scheme, Is.EqualTo ("net.tcp"), label.Get ());
			Assert.That (transportElement.TransferMode, Is.EqualTo (transferMode), label.Get ());
			label.LeaveScope (); // transport
			label.LeaveScope (); // elements
			label.LeaveScope (); // net-tcp
		}

		[Test]
		public void NetTcpBinding ()
		{
			var doc = MetadataProvider.Get ("net-tcp.xml");
			var label = new TestLabel ("NetTcpBinding");
			NetTcpBinding (
				doc, SecurityMode.None, false, TransferMode.Buffered, label);
		}

		[Test]
		public void NetTcpBinding2 ()
		{
			var doc = MetadataProvider.Get ("net-tcp2.xml");
			var label = new TestLabel ("NetTcpBinding2");
			NetTcpBinding (
				doc, SecurityMode.Transport, false,
				TransferMode.Buffered, label);
		}

		[Test]
		public void NetTcpBinding3 ()
		{
			var doc = MetadataProvider.Get ("net-tcp3.xml");
			var label = new TestLabel ("NetTcpBinding3");
			NetTcpBinding (
				doc, SecurityMode.None, true,
				TransferMode.Buffered, label);
		}
		
		[Test]
		public void NetTcpBinding4 ()
		{
			var doc = MetadataProvider.Get ("net-tcp4.xml");
			var label = new TestLabel ("NetTcpBinding4");
			NetTcpBinding (
				doc, SecurityMode.None, false,
				TransferMode.Streamed, label);
		}
		
		public void NetTcpBinding (MetadataSet doc, SecurityMode security,
		                           bool reliableSession, TransferMode transferMode,
		                           TestLabel label)
		{
			label.EnterScope ("netTcpBinding");

			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");

			label.EnterScope ("extensions");
			Assert.That (sd.Extensions, Is.Not.Null, label.Get ());
			Assert.That (sd.Extensions.Count, Is.EqualTo (1), label.Get ());
			Assert.That (sd.Extensions [0], Is.InstanceOfType (typeof(XmlElement)), label.Get ());
			
			var extension = (XmlElement)sd.Extensions [0];
			Assert.That (extension.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (extension.LocalName, Is.EqualTo ("Policy"), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());
			Assert.That (binding.Extensions.Count, Is.EqualTo (2), label.Get ());

			WS.SoapBinding soap = null;
			XmlElement xml = null;
			
			foreach (var ext in binding.Extensions) {
				if (ext is WS.SoapBinding)
					soap = (WS.SoapBinding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}
			
			CheckSoapBinding (soap, "http://schemas.microsoft.com/soap/tcp", label);

			label.EnterScope ("policy-xml");
			Assert.That (xml, Is.Not.Null, label.Get ());
			
			Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), label.Get ());
			Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference"), label.Get ());
			label.LeaveScope ();

			label.LeaveScope (); // wsdl

			var importer = new WsdlImporter (doc);

			label.EnterScope ("bindings");
			var bindings = importer.ImportAllBindings ();
			CheckImportErrors (importer, label);
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());
			
			CheckNetTcpBinding (
				bindings [0], security, reliableSession,
				transferMode, label);
			label.LeaveScope ();

			label.EnterScope ("endpoints");
			var endpoints = importer.ImportAllEndpoints ();
			CheckImportErrors (importer, label);
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());
			
			CheckEndpoint (endpoints [0], MetadataSamples.NetTcpUri, label);
			label.LeaveScope ();

			label.LeaveScope ();
		}

		[Test]
		public void BasicHttpBinding_ImportBinding ()
		{
			var label = new TestLabel ("BasicHttpBinding_ImportBinding");
			
			var doc = MetadataProvider.Get ("http.xml");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			var wsdlBinding = sd.Bindings [0];
			
			var importer = new WsdlImporter (doc);
			
			Assert.That (sd.Bindings, Is.Not.Null, label.Get ());
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			
			var binding = importer.ImportBinding (wsdlBinding);
			CheckImportErrors (importer, label);
			Assert.That (binding, Is.Not.Null, label.Get ());
		}

		[Test]
		public void BasicHttpBinding_ImportEndpoint ()
		{
			var label = new TestLabel ("BasicHttpBinding_ImportEndpoint");
			
			var doc = MetadataProvider.Get ("http.xml");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");
			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());

			var service = sd.Services [0];
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();

			var importer = new WsdlImporter (doc);

			var port = importer.ImportEndpoint (service.Ports [0]);
			CheckImportErrors (importer, label);
			Assert.That (port, Is.Not.Null, label.Get ());
		}

		[Test]
		public void BasicHttpBinding_Error ()
		{
			var label = new TestLabel ("BasicHttpBinding_Error");
			
			var doc = MetadataProvider.Get ("http-error.xml");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			var wsdlBinding = sd.Bindings [0];

			var importer = new WsdlImporter (doc);

			label.EnterScope ("all");

			var bindings = importer.ImportAllBindings ();
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (0), label.Get ());

			label.EnterScope ("errors");
			Assert.That (importer.Errors, Is.Not.Null, label.Get ());
			Assert.That (importer.Errors.Count, Is.EqualTo (1), label.Get ());

			var error = importer.Errors [0];
			Assert.That (error.IsWarning, Is.False, label.Get ());
			label.LeaveScope ();
			label.LeaveScope ();

			label.EnterScope ("single");

			try {
				importer.ImportBinding (wsdlBinding);
				Assert.Fail (label.Get ());
			} catch {
				;
			}

			Assert.That (importer.Errors.Count, Is.EqualTo (1), label.Get ());

			label.LeaveScope ();

			label.EnterScope ("single-first");

			var importer2 = new WsdlImporter (doc);

			try {
				importer2.ImportBinding (wsdlBinding);
				Assert.Fail (label.Get ());
			} catch {
				;
			}

			Assert.That (importer2.Errors.Count, Is.EqualTo (1), label.Get ());

			try {
				importer2.ImportBinding (wsdlBinding);
				Assert.Fail (label.Get ());
			} catch {
				;
			}

			var bindings2 = importer.ImportAllBindings ();
			Assert.That (bindings2, Is.Not.Null, label.Get ());
			Assert.That (bindings2.Count, Is.EqualTo (0), label.Get ());

			label.LeaveScope ();
		}

		[Test]
		public void BasicHttpBinding_Error2 ()
		{
			var label = new TestLabel ("BasicHttpBinding_Error2");
			
			var doc = MetadataProvider.Get ("http-error.xml");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");
			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());
			
			var service = sd.Services [0];
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();
			
			var importer = new WsdlImporter (doc);
			
			label.EnterScope ("all");
			
			var endpoints = importer.ImportAllEndpoints ();
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (0), label.Get ());
			
			label.EnterScope ("errors");
			Assert.That (importer.Errors, Is.Not.Null, label.Get ());
			Assert.That (importer.Errors.Count, Is.EqualTo (2), label.Get ());

			Assert.That (importer.Errors [0].IsWarning, Is.False, label.Get ());
			Assert.That (importer.Errors [1].IsWarning, Is.False, label.Get ());
			label.LeaveScope ();
			label.LeaveScope ();
			
			label.EnterScope ("single");

			try {
				importer.ImportEndpoint (service.Ports [0]);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			Assert.That (importer.Errors.Count, Is.EqualTo (2), label.Get ());
			
			label.LeaveScope ();
			
			label.EnterScope ("single-first");
			
			var importer2 = new WsdlImporter (doc);
			
			try {
				importer2.ImportEndpoint (service.Ports [0]);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			Assert.That (importer2.Errors.Count, Is.EqualTo (2), label.Get ());
			
			try {
				importer2.ImportEndpoint (service.Ports [0]);
				Assert.Fail (label.Get ());
			} catch {
				;
			}
			
			var endpoints2 = importer.ImportAllEndpoints ();
			Assert.That (endpoints2, Is.Not.Null, label.Get ());
			Assert.That (endpoints2.Count, Is.EqualTo (0), label.Get ());

			label.LeaveScope ();
		}

		[Test]
		public void BasicHttpBinding_ImportEndpoints ()
		{
			var label = new TestLabel ("BasicHttpBinding_ImportEndpoints");
			
			var doc = MetadataProvider.Get ("http.xml");
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			
			label.EnterScope ("wsdl");
			Assert.That (sd.Bindings, Is.Not.Null, label.Get ());
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());
			var binding = sd.Bindings [0];

			Assert.That (sd.Services, Is.Not.Null, label.Get ());
			Assert.That (sd.Services.Count, Is.EqualTo (1), label.Get ());
			var service = sd.Services [0];

			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (1), label.Get ());
			var port = service.Ports [0];

			Assert.That (sd.PortTypes, Is.Not.Null, label.Get ());
			Assert.That (sd.PortTypes.Count, Is.EqualTo (1), label.Get ());
			var portType = sd.PortTypes [0];

			label.LeaveScope ();
			
			var importer = new WsdlImporter (doc);

			label.EnterScope ("by-service");
			var byService = importer.ImportEndpoints (service);
			CheckImportErrors (importer, label);
			Assert.That (byService, Is.Not.Null, label.Get ());
			Assert.That (byService.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("by-binding");
			var byBinding = importer.ImportEndpoints (binding);
			CheckImportErrors (importer, label);
			Assert.That (byBinding, Is.Not.Null, label.Get ());
			Assert.That (byBinding.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("by-port-type");
			var byPortType = importer.ImportEndpoints (portType);
			CheckImportErrors (importer, label);
			Assert.That (byPortType, Is.Not.Null, label.Get ());
			Assert.That (byPortType.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();
		}
	}
}
