//
// Main.cs
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
using System.Text;
using System.Diagnostics;

namespace ProvconFaust.SimpleWebServer {

	class MainClass {
		public static void Main (string[] args)
		{
			Debug.Listeners.Add (new ConsoleTraceListener ());

			var main = new MainClass ("127.0.0.1", 8000);
			main.Start ();
			main.Post (main.RootUri, false);
			main.Post (main.RedirectUri, false);
			main.Post (main.RedirectContinueUri, false);
			main.Post (main.RootUri, true);
			main.Post (main.RedirectUri, true);
			main.Post (main.RedirectContinueUri, true);
			main.Stop ();
		}

		static string Encode (string format, params object[] args)
		{
			return string.Format (format, args);
		}
		
		static void Send (Stream stream, string format, params object[] args)
		{
			byte [] bytes = Encoding.ASCII.GetBytes (Encode (format, args));
			stream.Write (bytes, 0, bytes.Length);
		}

		public MainClass (string address, int port)
			: this (new IPEndPoint (IPAddress.Parse (address), port))
		{
		}

		public MainClass (IPEndPoint endpoint)
		{
			this.EndPoint = endpoint;

			RootUri = string.Format ("http://{0}:{1}/", endpoint.Address, endpoint.Port);
			RedirectUri = RootUri + "Redirect/";
			RedirectContinueUri = RootUri + "Redirect/Continue";

			Server = new Server (EndPoint);
		}

		public void Start ()
		{
			Server.Run (RequestHandler);
		}

		public void Stop ()
		{
			Server.Stop ();
		}

		public IPEndPoint EndPoint {
			get;
			private set;
		}

		public string RootUri {
			get;
			private set;
		}

		public string RedirectUri {
			get;
			private set;
		}

		public string RedirectContinueUri {
			get;
			private set;
		}

		public Server Server {
			get;
			private set;
		}

		public void RequestHandler (Server.Request request)
		{
			Server.Response response;
			
			if (request.Uri.StartsWith ("/Redirect/")) {
				var redirectUri = request.Uri.Substring (9);
				request.Server.Log ("REDIRECT: |{0}|", redirectUri);

				response = request.CreateResponse (HttpStatusCode.TemporaryRedirect);
				response.Headers ["Location"] = redirectUri;
				response.Headers ["Content-Type"] = "text/plain";
				response.Body = "Redirected";
				
				response.Send ();

				request.ReadBody ();
				return;
			}

			if (request.Uri.Contains ("Continue")) {
				response = request.CreateResponse (HttpStatusCode.Continue);
				response.Send ();
			}

			request.ReadBody ();
			
			response = request.CreateResponse (HttpStatusCode.OK);
			response.Headers ["Content-Type"] = "text/plain";
			response.Body = "TEST";
			
			response.Send ();
		}
		
		public void Post (string uri, bool chunked)
		{
			var req = (HttpWebRequest)HttpWebRequest.Create (uri);
			req.ProtocolVersion = HttpVersion.Version11;
			req.AllowAutoRedirect = true;
			req.ContentType = "application/x-www-form-urlencoded";
			req.Method = "POST";
			req.SendChunked = chunked;
			// req.AllowWriteStreamBuffering = false;
			
			var body = "body=Client Data";
			
			using (var stream = req.GetRequestStream ()) {
				Send (stream, body);
				stream.Flush ();
				stream.Close ();
			}
			
			var res = (HttpWebResponse)req.GetResponse ();
			Console.WriteLine (res.StatusCode);
			
			using (var reader = new StreamReader (res.GetResponseStream ())) {
				var text = reader.ReadToEnd ();
				Console.WriteLine (text);
				if (text.Trim () != "TEST")
					throw new WebException ("Got unexpected response.");
			}
		}

	}
}
