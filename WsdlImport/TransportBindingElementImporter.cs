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
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using WS = System.Web.Services.Description;
using QName = System.Xml.XmlQualifiedName;

namespace WsdlImport {

	public class TransportBindingElementImporter : IWsdlImportExtension {
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
			
			if (ImportBasicHttpEndpoint (context)) {
				Console.WriteLine ("Successfully imported endpoint.");
				return;
			}
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

		#endregion
	}
}

