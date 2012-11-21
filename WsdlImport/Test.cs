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
		                            bool useAuth, TestLabel label)
		{
			label.EnterScope ("http");
			Assert.That (binding, Is.InstanceOfType (typeof(BasicHttpBinding)), label.Get ());
			var basicHttp = (BasicHttpBinding)binding;
			Assert.That (basicHttp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap11), label.Get ());
			Assert.That (basicHttp.MessageVersion, Is.EqualTo (MessageVersion.Soap11), label.Get ());
			Assert.That (basicHttp.Scheme, Is.EqualTo (scheme), label.Get ());
			Assert.That (basicHttp.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());
			Assert.That (basicHttp.MessageEncoding, Is.EqualTo (WSMessageEncoding.Text), label.Get ());
			Assert.That (basicHttp.Security, Is.Not.Null, label.Get ());
			Assert.That (basicHttp.Security.Mode, Is.EqualTo (security), label.Get ());

			label.EnterScope ("elements");

			var elements = basicHttp.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label.Get ());
			Assert.That (elements.Count, Is.EqualTo (2), label.Get ());
			
			TextMessageEncodingBindingElement textElement = null;
			HttpTransportBindingElement transportElement = null;
			
			foreach (var element in elements) {
				if (element is TextMessageEncodingBindingElement)
					textElement = (TextMessageEncodingBindingElement)element;
				else if (element is HttpTransportBindingElement)
					transportElement = (HttpTransportBindingElement)element;
				else
					Assert.Fail (label.Get ());
			}

			label.EnterScope ("text");
			Assert.That (textElement, Is.Not.Null, label.Get ());
			Assert.That (textElement.WriteEncoding, Is.InstanceOfType (typeof(UTF8Encoding)), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("transport");
			Assert.That (transportElement, Is.Not.Null, label.Get ());
			
			Assert.That (transportElement.Realm, Is.Empty, label.Get ());
			Assert.That (transportElement.Scheme, Is.EqualTo (scheme), label.Get ());
			Assert.That (transportElement.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());

			label.EnterScope ("auth");
			var authScheme = useAuth ? AuthenticationSchemes.Ntlm : AuthenticationSchemes.Anonymous;
			Assert.That (transportElement.AuthenticationScheme, Is.EqualTo (authScheme), label.Get ());

			var clientCred = useAuth ? HttpClientCredentialType.Ntlm : HttpClientCredentialType.None;
			Assert.That (basicHttp.Security.Transport.ClientCredentialType, Is.EqualTo (clientCred), label.Get ());
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
			BasicHttpBinding (doc, label);
		}

		public void BasicHttpBinding (MetadataSet doc, TestLabel label)
		{
			label.EnterScope ("basicHttpBinding");

			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");
			label.EnterScope ("bindings");
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), label.Get ());

			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, label.Get ());
			Assert.That (binding.Extensions, Is.Not.Null, label.Get ());
			Assert.That (binding.Extensions.Count, Is.EqualTo (1), label.Get ());
			label.LeaveScope ();

			CheckSoapBinding (binding.Extensions [0], WS.SoapBinding.HttpTransport, label);
			label.LeaveScope ();

			var importer = new WsdlImporter (doc);

			var bindings = importer.ImportAllBindings ();

			label.EnterScope ("bindings");
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());

			CheckBasicHttpBinding (bindings [0], "http", BasicHttpSecurityMode.None, false, label);
			label.LeaveScope ();

			var endpoints = importer.ImportAllEndpoints ();

			label.EnterScope ("endpoints");
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());

			CheckEndpoint (endpoints [0], Utils.HttpUri, label);
			label.LeaveScope ();

			Utils.CreateConfig (bindings [0], "http.config");

			label.LeaveScope ();
		}

		[Test]
		public void BasicHttpsBinding ()
		{
			var doc = MetadataProvider.Get ("https.xml");
			var label = new TestLabel ("BasicHttpsBinding");
			var binding = BasicHttpsBinding (doc, BasicHttpSecurityMode.Transport, false, label);
			Utils.CreateConfig (binding, "https.config");
		}

		[Test]
		public void BasicHttpsBinding2 ()
		{
			var doc = MetadataProvider.Get ("https2.xml");
			var label = new TestLabel ("BasicHttpsBinding2");
			var binding = BasicHttpsBinding (doc, BasicHttpSecurityMode.Transport, true, label);
			Utils.CreateConfig (binding, "https2.config");
		}

		Binding BasicHttpsBinding (MetadataSet doc, BasicHttpSecurityMode security,
		                           bool useAuth, TestLabel label)
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
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());

			CheckBasicHttpBinding (bindings [0], "https", security, useAuth, label);
			label.LeaveScope ();

			label.EnterScope ("endpoints");
			var endpoints = importer.ImportAllEndpoints ();
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());
			
			CheckEndpoint (endpoints [0], Utils.HttpsUri, label);
			label.LeaveScope ();

			label.LeaveScope ();
			return bindings [0];
		}

		void CheckNetTcpBinding (Binding binding, SecurityMode security, TestLabel label)
		{
			label.EnterScope ("net-tcp");
			Assert.That (binding, Is.InstanceOfType (typeof (NetTcpBinding)), label.Get ());
			var netTcp = (NetTcpBinding)binding;
			Assert.That (netTcp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap12), label.Get ());
			Assert.That (netTcp.MessageVersion, Is.EqualTo (MessageVersion.Soap12WSAddressing10), label.Get ());
			Assert.That (netTcp.Scheme, Is.EqualTo ("net.tcp"), label.Get ());
			Assert.That (netTcp.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());
			Assert.That (netTcp.Security, Is.Not.Null, label.Get ());
			Assert.That (netTcp.Security.Mode, Is.EqualTo (security), label.Get ());

			label.EnterScope ("elements");
			
			var elements = netTcp.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label.Get ());
			Assert.That (elements.Count, Is.EqualTo (3), label.Get ());
			
			TcpTransportBindingElement transportElement = null;
			TransactionFlowBindingElement transactionFlowElement = null;
			BinaryMessageEncodingBindingElement encodingElement = null;
			
			foreach (var element in elements) {
				if (element is TcpTransportBindingElement)
					transportElement = (TcpTransportBindingElement)element;
				else if (element is TransactionFlowBindingElement)
					transactionFlowElement = (TransactionFlowBindingElement)element;
				else if (element is BinaryMessageEncodingBindingElement)
					encodingElement = (BinaryMessageEncodingBindingElement)element;
				else
					Assert.Fail (label.Get ());
			}

			label.EnterScope ("encoding");
			Assert.That (encodingElement, Is.Not.Null, label.Get ());
			label.LeaveScope ();

			label.EnterScope ("transaction");
			Assert.That (transactionFlowElement, Is.Not.Null, label + "m");
			label.LeaveScope ();

			label.EnterScope ("transport");
			Assert.That (transportElement, Is.Not.Null, label.Get ());

			Assert.That (transportElement.Scheme, Is.EqualTo ("net.tcp"), label.Get ());
			Assert.That (transportElement.TransferMode, Is.EqualTo (TransferMode.Buffered), label.Get ());
			label.LeaveScope (); // transport
			label.LeaveScope (); // elements
			label.LeaveScope (); // net-tcp
		}

		[Test]
		public void NetTcpBinding ()
		{
			var doc = MetadataProvider.Get ("net-tcp.xml");
			var label = new TestLabel ("NetTcpBinding");
			NetTcpBinding (doc, label);
		}

		public void NetTcpBinding (MetadataSet doc, TestLabel label)
		{
			label.EnterScope ("netTcpBinding");

			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			label.EnterScope ("wsdl");

			label.EnterScope ("extensions");
			Assert.That (sd.Extensions, Is.Not.Null, label.Get ());
			Assert.That (sd.Extensions.Count, Is.EqualTo (1), label.Get ());
			Assert.That (sd.Extensions [0], Is.InstanceOfType (typeof (XmlElement)), label.Get ());
			
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
			Assert.That (bindings, Is.Not.Null, label.Get ());
			Assert.That (bindings.Count, Is.EqualTo (1), label.Get ());
			
			CheckNetTcpBinding (bindings [0], SecurityMode.None, label);
			label.LeaveScope ();
			
			var endpoints = importer.ImportAllEndpoints ();
			Assert.That (endpoints, Is.Not.Null, label.Get ());
			Assert.That (endpoints.Count, Is.EqualTo (1), label.Get ());
			
			CheckEndpoint (endpoints [0], Utils.NetTcpUri, label);
			label.LeaveScope ();

			Utils.CreateConfig (bindings [0], "net-tcp.config");

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
	}
}
