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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using WS = System.Web.Services.Description;

namespace WsdlImport {

	public class Client {

		static MetadataSet LoadMetadata (Uri uri)
		{
			var filename = Path.Combine ("Resources", "Server.wsdl");
			if (!File.Exists (filename)) {
				Console.WriteLine ("Downloading service metadata ...");
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

		public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, 
		                              SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		// http://blogs.msdn.com/b/james_osbornes_blog/archive/2010/12/10/selfhosting-a-wcf-service-over-https.aspx
		static void GetThumbprint ()
		{
			X509Certificate2 certificate = new X509Certificate2 ("Resources/Server.cer");
			Console.WriteLine ("THUMBPRINT: {0}", certificate.Thumbprint);
			Console.WriteLine ("NEW GUID: {0}", Guid.NewGuid ());

			// $ netsh http add sslcert ipport=0.0.0.0:9998 certhash=EC4A1E4DB064D96AFD3EA345551C822556146C13 appid={f7ae9a2e-de9a-43ec-95ab-8f3a94932ed1}

			// netsh http add sslcert ipport=0.0.0.0:<port> certhash={<thumbprint>} appid={<some GUID>}
			// Process bindPortToCertificate = new Process();
			// bindPortToCertificate.StartInfo.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "netsh.exe");
			// bindPortToCertificate.StartInfo.Arguments = string.Format("http add sslcert ipport=0.0.0.0:{0} certhash={1} appid={{{2}}}", port, certificate.Thumbprint, Guid.NewGuid());
			// bindPortToCertificate.Start();
			// bindPortToCertificate.WaitForExit();
		}
		
		public static void Run (Uri uri)
		{
			ServicePointManager.ServerCertificateValidationCallback = Validator;

			var doc = LoadMetadata (uri);
			var importer = new WsdlImporter (doc);

			var bindings = importer.ImportAllBindings ();
			var endpoints = importer.ImportAllEndpoints ();

			foreach (var error in importer.Errors) {
				if (error.IsWarning)
					Console.WriteLine ("WARNING: {0}", error.Message);
				else
					Console.WriteLine ("ERROR: {0}", error.Message);
			}

			Console.WriteLine ("DONE IMPORTING: {0} {1}", bindings.Count, endpoints.Count);

			foreach (var binding in bindings)
				Console.WriteLine ("BINDING: {0}", binding);
			foreach (var endpoint in endpoints)
				Console.WriteLine ("ENDPOINT: {0}", endpoint.Address);

			foreach (var endpoint in endpoints) {
				try {
					Run (endpoint);
				} catch (Exception ex) {
					Console.WriteLine ("ERROR ({0}): {1}", endpoint.Address, ex);
				}
			}
		}

		static void Run (ServiceEndpoint endpoint)
		{
			Console.WriteLine ("TRYING ENDPOINT: {0}", endpoint.Address);
			var channel = ChannelFactory<IMyService>.CreateChannel (
				endpoint.Binding, endpoint.Address);
			Console.WriteLine ("GOT CHANNEL: {0}", channel);

			var hello = channel.Hello ();
			Console.WriteLine ("HELLO: {0}", hello);
		}
	}
}

