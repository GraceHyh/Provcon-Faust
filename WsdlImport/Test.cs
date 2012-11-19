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

		[Test]
		public void BasicHttpBinding ()
		{
			var doc = Utils.LoadBasicHttpMetadata ();
			var importer = new WsdlImporter (doc);
			BasicHttpBinding (doc, importer);
		}

		[Test]
		public void BasicHttpBinding_CustomImporter ()
		{
			var doc = Utils.LoadBasicHttpMetadata ();
			var importer = Utils.GetCustomImporter (doc);
			BasicHttpBinding (doc, importer);
		}

		void CheckSoapBinding (object extension, string transport, string label)
		{
			Assert.That (extension, Is.InstanceOfType (typeof (WS.SoapBinding)), label);
			var soap = (WS.SoapBinding)extension;
			Assert.That (soap.Style, Is.EqualTo (WS.SoapBindingStyle.Document), label + "a");
			Assert.That (soap.Transport, Is.EqualTo (transport), label + "b");
			Assert.That (soap.Required, Is.False, label + "c");
		}

		void CheckBasicHttpBinding (Binding binding, string scheme, BasicHttpSecurityMode security, string label)
		{
			Assert.That (binding, Is.InstanceOfType (typeof (BasicHttpBinding)), label);
			var basicHttp = (BasicHttpBinding)binding;
			Assert.That (basicHttp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap11), label + "a");
			Assert.That (basicHttp.MessageVersion, Is.EqualTo (MessageVersion.Soap11), label + "b");
			Assert.That (basicHttp.Scheme, Is.EqualTo (scheme), label + "c");
			Assert.That (basicHttp.TransferMode, Is.EqualTo (TransferMode.Buffered), label + "d");
			Assert.That (basicHttp.MessageEncoding, Is.EqualTo (WSMessageEncoding.Text), label + "e");
			Assert.That (basicHttp.Security, Is.Not.Null, label + "f");
			Assert.That (basicHttp.Security.Mode, Is.EqualTo (security), label + "g");

			var elements = basicHttp.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label + "h");
			Assert.That (elements.Count, Is.EqualTo (2), label + "i");
			
			TextMessageEncodingBindingElement textElement = null;
			HttpTransportBindingElement transportElement = null;
			
			foreach (var element in elements) {
				if (element is TextMessageEncodingBindingElement)
					textElement = (TextMessageEncodingBindingElement)element;
				else if (element is HttpTransportBindingElement)
					transportElement = (HttpTransportBindingElement)element;
				else
					Assert.Fail (label + "j");
			}
			
			Assert.That (textElement, Is.Not.Null, label + "k");
			Assert.That (transportElement, Is.Not.Null, label + "l");
			
			Assert.That (textElement.WriteEncoding, Is.InstanceOfType (typeof(UTF8Encoding)), label + "m");
			Assert.That (transportElement.Realm, Is.Empty, label + "n");
			Assert.That (transportElement.Scheme, Is.EqualTo (scheme), label + "o");
			Assert.That (transportElement.TransferMode, Is.EqualTo (TransferMode.Buffered), label + "p");
		}

		void CheckEndpoint (ServiceEndpoint endpoint, string uri, string label)
		{
			Assert.That (endpoint.ListenUri, Is.EqualTo (new Uri (uri)), label + "a");
			Assert.That (endpoint.ListenUriMode, Is.EqualTo (ListenUriMode.Explicit), label + "b");
			Assert.That (endpoint.Contract, Is.Not.Null, "c");
			Assert.That (endpoint.Contract.Name, Is.EqualTo ("MyContract"), "d");
			Assert.That (endpoint.Address, Is.Not.Null, "e");
			Assert.That (endpoint.Address.Uri, Is.EqualTo (new Uri (uri)), label + "f");
			Assert.That (endpoint.Address.Identity, Is.Null, label + "g");
			Assert.That (endpoint.Address.Headers, Is.Not.Null, label + "h");
			Assert.That (endpoint.Address.Headers.Count, Is.EqualTo (0), label + "i");
		}

		public void BasicHttpBinding (MetadataSet doc, WsdlImporter importer)
		{
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), "#1");

			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, "#2a");
			Assert.That (binding.Extensions, Is.Not.Null, "#2b");
			Assert.That (binding.Extensions.Count, Is.EqualTo (1), "#2c");

			CheckSoapBinding (binding.Extensions [0], WS.SoapBinding.HttpTransport, "#3");

			var bindings = importer.ImportAllBindings ();
			Assert.That (bindings, Is.Not.Null, "#4a");
			Assert.That (bindings.Count, Is.EqualTo (1), "#4b");

			CheckBasicHttpBinding (bindings [0], "http", BasicHttpSecurityMode.None, "#5");

			var endpoints = importer.ImportAllEndpoints ();
			Assert.That (endpoints, Is.Not.Null, "#6");
			Assert.That (endpoints.Count, Is.EqualTo (1), "#6a");

			CheckEndpoint (endpoints [0], Utils.HttpUri, "#7");
		}

		[Test]
		public void BasicHttpsBinding ()
		{
			var doc = Utils.LoadBasicHttpsMetadata ();
			var importer = new WsdlImporter (doc);
			BasicHttpsBinding (doc, importer);
		}

		[Test]
		public void BasicHttpsBinding_CustomImporter ()
		{
			var doc = Utils.LoadBasicHttpsMetadata ();
			var importer = Utils.GetCustomImporter (doc);
			BasicHttpsBinding (doc, importer);
		}

		public void BasicHttpsBinding (MetadataSet doc, WsdlImporter importer)
		{
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;

			Assert.That (sd.Extensions, Is.Not.Null, "#1");
			Assert.That (sd.Extensions.Count, Is.EqualTo (1), "#1a");
			Assert.That (sd.Extensions [0], Is.InstanceOfType (typeof (XmlElement)), "#1b");

			var extension = (XmlElement)sd.Extensions [0];
			Assert.That (extension.NamespaceURI, Is.EqualTo (WspNamespace), "#1c");
			Assert.That (extension.LocalName, Is.EqualTo ("Policy"), "#1d");

			Assert.That (sd.Bindings.Count, Is.EqualTo (1), "#2");
			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, "#2a");
			Assert.That (binding.Extensions, Is.Not.Null, "#2b");
			Assert.That (binding.Extensions.Count, Is.EqualTo (2), "#2c");

			WS.SoapBinding soap = null;
			XmlElement xml = null;

			foreach (var ext in binding.Extensions) {
				if (ext is WS.SoapBinding)
					soap = (WS.SoapBinding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}

			CheckSoapBinding (soap, WS.SoapBinding.HttpTransport, "#3");

			var bindings = importer.ImportAllBindings ();
			Assert.That (bindings, Is.Not.Null, "#4a");
			Assert.That (bindings.Count, Is.EqualTo (1), "#4b");

			CheckBasicHttpBinding (bindings [0], "https", BasicHttpSecurityMode.Transport, "#5");

			Assert.That (xml, Is.Not.Null, "#6");

			Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), "#6a");
			Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference"), "#6b");

			var endpoints = importer.ImportAllEndpoints ();
			Assert.That (endpoints, Is.Not.Null, "#7");
			Assert.That (endpoints.Count, Is.EqualTo (1), "#7a");
			
			CheckEndpoint (endpoints [0], Utils.HttpsUri, "#8");
		}

		void CheckNetTcpBinding (Binding binding, SecurityMode security, string label)
		{
			Assert.That (binding, Is.InstanceOfType (typeof (NetTcpBinding)), label);
			var netTcp = (NetTcpBinding)binding;
			Assert.That (netTcp.EnvelopeVersion, Is.EqualTo (EnvelopeVersion.Soap12), label + "a");
			Assert.That (netTcp.MessageVersion, Is.EqualTo (MessageVersion.Soap12WSAddressing10), label + "b");
			Assert.That (netTcp.Scheme, Is.EqualTo ("net.tcp"), label + "c");
			Assert.That (netTcp.TransferMode, Is.EqualTo (TransferMode.Buffered), label + "d");
			Assert.That (netTcp.Security, Is.Not.Null, label + "e");
			Assert.That (netTcp.Security.Mode, Is.EqualTo (security), label + "f");
			
			var elements = netTcp.CreateBindingElements ();
			Assert.That (elements, Is.Not.Null, label + "h");
			Assert.That (elements.Count, Is.EqualTo (3), label + "i");
			
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
					Assert.Fail (label + "j");
			}
			
			Assert.That (encodingElement, Is.Not.Null, label + "k");
			Assert.That (transportElement, Is.Not.Null, label + "l");
			Assert.That (transactionFlowElement, Is.Not.Null, label + "m");
			
			Assert.That (transportElement.Scheme, Is.EqualTo ("net.tcp"), label + "o");
			Assert.That (transportElement.TransferMode, Is.EqualTo (TransferMode.Buffered), label + "p");
		}

		[Test]
		public void NetTcpBinding ()
		{
			var doc = Utils.LoadNetTcpMetadata ();
			var importer = new WsdlImporter (doc);
			NetTcpBinding (doc, importer);
		}

		[Test]
		public void NetTcpBinding_CustomImporter ()
		{
			var doc = Utils.LoadNetTcpMetadata ();
			var importer = Utils.GetCustomImporter (doc);
			NetTcpBinding (doc, importer);
		}

		public void NetTcpBinding (MetadataSet doc, WsdlImporter importer)
		{
			var sd = (WS.ServiceDescription)doc.MetadataSections [0].Metadata;
			
			Assert.That (sd.Extensions, Is.Not.Null, "#1");
			Assert.That (sd.Extensions.Count, Is.EqualTo (1), "#1a");
			Assert.That (sd.Extensions [0], Is.InstanceOfType (typeof (XmlElement)), "#1b");
			
			var extension = (XmlElement)sd.Extensions [0];
			Assert.That (extension.NamespaceURI, Is.EqualTo (WspNamespace), "#1c");
			Assert.That (extension.LocalName, Is.EqualTo ("Policy"), "#1d");
			
			Assert.That (sd.Bindings.Count, Is.EqualTo (1), "#2");
			var binding = sd.Bindings [0];
			Assert.That (binding.ExtensibleAttributes, Is.Null, "#2a");
			Assert.That (binding.Extensions, Is.Not.Null, "#2b");
			Assert.That (binding.Extensions.Count, Is.EqualTo (2), "#2c");
			
			WS.SoapBinding soap = null;
			XmlElement xml = null;
			
			foreach (var ext in binding.Extensions) {
				if (ext is WS.SoapBinding)
					soap = (WS.SoapBinding)ext;
				else if (ext is XmlElement)
					xml = (XmlElement)ext;
			}
			
			CheckSoapBinding (soap, "http://schemas.microsoft.com/soap/tcp", "#3");
			
			var bindings = importer.ImportAllBindings ();
			Assert.That (bindings, Is.Not.Null, "#4a");
			Assert.That (bindings.Count, Is.EqualTo (1), "#4b");
			
			CheckNetTcpBinding (bindings [0], SecurityMode.None, "#5");
			
			Assert.That (xml, Is.Not.Null, "#6");
			
			Assert.That (xml.NamespaceURI, Is.EqualTo (WspNamespace), "#6a");
			Assert.That (xml.LocalName, Is.EqualTo ("PolicyReference"), "#6b");

			var endpoints = importer.ImportAllEndpoints ();
			Assert.That (endpoints, Is.Not.Null, "#7");
			Assert.That (endpoints.Count, Is.EqualTo (1), "#7a");
			
			CheckEndpoint (endpoints [0], Utils.NetTcpUri, "#8");
		}
	}
}
