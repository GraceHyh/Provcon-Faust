//
// Client.cs
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
using System.Net;
using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Description;
using WS = System.Web.Services.Description;

namespace WsdlImport {

	public class Client {

		static MetadataSet LoadMetadata (Uri uri)
		{
			var filename = Path.Combine ("Resources", "Server.wsdl");
			if (!File.Exists (filename)) {
				var wc = new WebClient ();
				wc.DownloadFile (uri, filename);
				Console.WriteLine ("Downloaded service metadata into {0}.", filename);
			} else {
				Console.WriteLine ("Loading cached service metadata from {0}.", filename);
			}

			using (var stream = new StreamReader (filename)) {
				var doc = new MetadataSet ();
				var service = WS.ServiceDescription.Read (stream);
				var sect = new MetadataSection (
					"http://schemas.xmlsoap.org/wsdl/", "http://tempuri.org/", service);
				doc.MetadataSections.Add (sect);
				return doc;
			}
		}

		public static void Run (Uri uri)
		{
			var doc = LoadMetadata (uri);
			var importer = new WsdlImporter (doc);

			var bindings = importer.ImportAllBindings ();
			var endpoints = importer.ImportAllEndpoints ();

			foreach (var binding in bindings)
				Console.WriteLine ("BINDING: {0}", binding);
			foreach (var endpoint in endpoints)
				Console.WriteLine ("ENDPOINT: {0}", endpoint.Address);

			var channel = ChannelFactory<IMyService>.CreateChannel (
				bindings [0], endpoints [0].Address);
			Console.WriteLine ("GOT CHANNEL: {0}", channel);

			var hello = channel.Hello ();
			Console.WriteLine ("HELLO: {0}", hello);
		}
	}
}

