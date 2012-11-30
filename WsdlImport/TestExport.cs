//
// TestExport.cs
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
using System.ServiceModel.Description;
using MonoTests.System.ServiceModel.MetadataTests;

using QName = System.Xml.XmlQualifiedName;
using WS = System.Web.Services.Description;

using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace WsdlImport {

	[TestFixture]
	public class TestExport {
		internal const string HttpUri = "http://tempuri.org/TestHttp/";
		internal const string HttpsUri = "https://tempuri.org/TestHttps/";
		internal const string NetTcpUri = "net-tcp://tempuri.org:8000/TestNetTcp/";
		internal const string CustomUri = "custom://tempuri.org:8000/Test/";

		[Test]
		public void SimpleExport ()
		{
			var label = new TestLabel ("DuplicateContract");
			
			var cd = new ContractDescription ("MyContract");
			var endpoint = new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri));
			
			var exporter = new WsdlExporter ();
			exporter.ExportContract (cd);
			exporter.ExportEndpoint (endpoint);
			
			CheckExport (
				exporter, new QName ("MyContract", "http://tempuri.org/"),
				"BasicHttpBinding", 1, label);
		}

		[Test]
		public void DuplicateContract ()
		{
			var label = new TestLabel ("DuplicateContract");

			var cd = new ContractDescription ("MyContract");
			var endpoint = new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri));

			var exporter = new WsdlExporter ();
			exporter.ExportContract (cd);
			exporter.ExportContract (cd);
			exporter.ExportEndpoint (endpoint);

			CheckExport (
				exporter, new QName ("MyContract", "http://tempuri.org/"),
				"BasicHttpBinding", 1, label);
		}

		[Test]
		public void DuplicateEndpoint ()
		{
			var label = new TestLabel ("DuplicateEndpoint");

			var cd = new ContractDescription ("MyContract");
			var endpoint = new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri));

			var exporter = new WsdlExporter ();
			exporter.ExportEndpoint (endpoint);
			exporter.ExportEndpoint (endpoint);

			CheckExport (
				exporter, new QName ("MyContract", "http://tempuri.org/"),
				"BasicHttpBinding", 1, label);
		}

		[Test]
		public void DuplicateEndpoint2 ()
		{
			var label = new TestLabel ("DuplicateEndpoint2");
			
			var cd = new ContractDescription ("MyContract");
			var endpoint = new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri));
			var endpoint2 = new ServiceEndpoint (
				cd, new BasicHttpBinding (), new EndpointAddress (HttpUri));
			
			var exporter = new WsdlExporter ();
			exporter.ExportEndpoint (endpoint);
			exporter.ExportEndpoint (endpoint);
			exporter.ExportEndpoint (endpoint2);
			
			CheckExport (
				exporter, new QName ("MyContract", "http://tempuri.org/"),
				"BasicHttpBinding", 2, label);
		}

		public static void CheckExport (
			WsdlExporter exporter, QName contractName, string bindingName,
			int countEndpoints, TestLabel label)
		{
			Assert.That (exporter.GeneratedWsdlDocuments, Is.Not.Null, label.Get ());
			Assert.That (exporter.GeneratedWsdlDocuments.Count, Is.EqualTo (1), label.Get ());
			
			var wsdl = exporter.GeneratedWsdlDocuments [0];
			CheckExport (wsdl, contractName, bindingName, countEndpoints, label);
		}

		public static void CheckExport (
			WS.ServiceDescription wsdl, QName contractName, string bindingName,
			int countEndpoints, TestLabel label)
		{
			label.EnterScope ("ServiceDescription");
			Assert.That (wsdl.TargetNamespace, Is.EqualTo (contractName.Namespace), label.Get ());
			Assert.That (wsdl.Name, Is.EqualTo ("service"), label.Get ());
			label.LeaveScope ();

			label.EnterScope ("Bindings");
			Assert.That (wsdl.Bindings, Is.Not.Null, label.Get ());
			Assert.That (wsdl.Bindings.Count, Is.EqualTo (countEndpoints), label.Get ());
			
			for (int i = 0; i < countEndpoints; i++) {
				label.EnterScope (string.Format ("#{0}", i+1));
				var binding = wsdl.Bindings [i];
				var expectedName = string.Format (
					"{0}_{1}{2}", bindingName, contractName.Name,
					i > 0 ? i.ToString () : "");
				Assert.That (binding.Name, Is.EqualTo (expectedName), label.Get ());
				Assert.That (binding.Type, Is.EqualTo (contractName), label.Get ());
				label.LeaveScope ();
			}
			label.LeaveScope ();
			
			label.EnterScope ("PortTypes");
			Assert.That (wsdl.PortTypes, Is.Not.Null, label.Get ());
			Assert.That (wsdl.PortTypes.Count, Is.EqualTo (1), label.Get ());
			var portType = wsdl.PortTypes [0];
			Assert.That (portType.Name, Is.EqualTo (contractName.Name), label.Get ());
			label.LeaveScope ();
			
			label.EnterScope ("Services");
			Assert.That (wsdl.Services, Is.Not.Null, label.Get ());
			Assert.That (wsdl.Services.Count, Is.EqualTo (1), label.Get ());
			var service = wsdl.Services [0];
			Assert.That (service.Name, Is.EqualTo ("service"), label.Get ());
			label.LeaveScope ();
			
			label.EnterScope ("Ports");
			Assert.That (service.Ports, Is.Not.Null, label.Get ());
			Assert.That (service.Ports.Count, Is.EqualTo (countEndpoints), label.Get ());
			for (int i = 0; i < countEndpoints; i++) {
				label.EnterScope (string.Format ("#{0}", i+1));
				var port = service.Ports [i];
				var expectedName = string.Format (
					"{0}_{1}{2}", bindingName, contractName.Name,
					i > 0 ? i.ToString () : "");
				var qname = new QName (expectedName, contractName.Namespace);
				Assert.That (port.Name, Is.EqualTo (qname.Name), label.Get ());
				Assert.That (port.Binding, Is.EqualTo (qname), label.Get ());
				label.LeaveScope ();
			}
			label.LeaveScope ();
		}

		public static void Run ()
		{
			var test = new TestExport ();
			test.SimpleExport ();
			test.DuplicateContract ();
			test.DuplicateEndpoint ();
			test.DuplicateEndpoint2 ();
		}
	}
}

