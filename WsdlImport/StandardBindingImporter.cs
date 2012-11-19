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
			Console.WriteLine ("BEFORE IMPORT");
		}

		public void ImportContract (WsdlImporter importer, WsdlContractConversionContext contractContext)
		{
			Console.WriteLine ("IMPORT CONTRACT");
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

			Console.WriteLine ("IMPORT ENDPOINT: {0} {1} {2} {3}",
			                   binding, binding.Scheme, qname, context.WsdlPort != null);

			if (context.Endpoint.Binding is CustomBinding) {
				if (!DoImportBinding (context))
					return;
				Console.WriteLine ("Successfully imported binding.");
			}

			// Only import the binding, not the endpoint.
			if (context.WsdlPort == null)
				return;

			if (!DoImportEndpoint (context))
				return;

			Console.WriteLine ("Successfully imported endpoint.");
		}

		bool DoImportBinding (WsdlEndpointConversionContext context)
		{
			if (ImportBasicHttpBinding (context))
				return true;
			return false;
		}

		bool DoImportEndpoint (WsdlEndpointConversionContext context)
		{
			if (ImportBasicHttpEndpoint (context))
				return true;
			return false;
		}

		bool ImportBasicHttpBinding (WsdlEndpointConversionContext context)
		{
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

			var http = new BasicHttpBinding ();

			http.Name = context.Endpoint.Binding.Name;
			http.Namespace = context.Endpoint.Binding.Namespace;

			context.Endpoint.Binding = http;
			return true;
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

		const string TcpTransport = "http://schemas.microsoft.com/soap/tcp";

		bool ImportNetTcp (WS.Binding binding, ServiceEndpoint endpoint)
		{
			WS.Soap12Binding soap = null;
			foreach (var extension in binding.Extensions) {
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

			netTcp.Name = endpoint.Binding.Name;
			netTcp.Namespace = endpoint.Binding.Namespace;

			endpoint.Binding = netTcp;
			return true;
		}

		#endregion

		public StandardBindingImporter ()
		{
		}
	}
}

