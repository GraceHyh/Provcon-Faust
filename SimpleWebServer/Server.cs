//
// SimpleServer.cs
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
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ProvconFaust.SimpleWebServer {

	public class Server {

		public IPEndPoint EndPoint {
			get;
			private set;
		}

		protected Socket Socket {
			get;
			private set;
		}

		protected NetworkStream Stream {
			get;
			private set;
		}

		protected StreamReader Reader {
			get;
			private set;
		}

		protected StreamWriter Writer {
			get;
			private set;
		}

		public Server (IPEndPoint endpoint)
		{
			EndPoint = endpoint;
		}

		public void Log (string message, params object[] args)
		{
			Debug.WriteLine (string.Format (message, args), "SimpleServer");
		}

		public void Run (Action<Request> handler)
		{
			var listener = new TcpListener (EndPoint);
			listener.Start ();
			listener.BeginAcceptSocket (ar => {
				Socket = listener.EndAcceptSocket (ar);
				Log ("GOT SOCKET");

				Stream = new NetworkStream (Socket);
				
				Reader = new StreamReader (Stream);
				Writer = new StreamWriter (Stream);

				while (true) {
					var request = new Request (this);
					handler (request);
				}
			}, null);
		}

		public void Stop ()
		{
			Socket.Close ();
		}

		public class Request {
			public Server Server {
				get;
				private set;
			}

			public Dictionary<string,string> Headers {
				get;
				private set;
			}

			public string Method {
				get;
				private set;
			}

			public string Uri {
				get;
				private set;
			}

			public Version Version {
				get;
				private set;
			}

			public Request (Server server)
			{
				Server = server;

				var request = server.Reader.ReadLine ();

				if (request.EndsWith ("HTTP/1.0"))
					Version = HttpVersion.Version10;
				else if (request.EndsWith ("HTTP/1.1"))
					Version = HttpVersion.Version11;
				else
					throw new WebException ("Invalid request: " + request);

				int pos = request.IndexOf (' ');
				Method = request.Substring (0, pos);
				Uri = request.Substring (pos+1, request.Length-pos-10);

				Server.Log ("GOT REQUEST: |{0}|{1}|{2}|", Method, Uri, Version);

				Headers = new Dictionary<string,string> ();
				do {
					var line = server.Reader.ReadLine ();
					if (line == null || line == string.Empty)
						break;
					
					pos = line.IndexOf (":");
					var key = line.Substring (0, pos).Trim ();
					var value = line.Substring (pos + 1).Trim ();
					Server.Log ("HEADER: |{0}|{1}|", key, value);
					Headers [key] = value;
				} while (true);
				
				Server.Log ("GOT HEADERS: {0}", Headers.ContainsKey ("Content-Length"));
			}

			public string ReadBody ()
			{
				var reader = Server.Reader;
				if (Headers.ContainsKey ("Content-Length")) {
					var length = Int32.Parse (Headers ["Content-Length"]);
					var buffer = new char [length];
					int ret = reader.Read (buffer, 0, length);
					if (ret != length)
						throw new InvalidOperationException ();
					var text = new string (buffer);
					Server.Log ("CONTENTS: |{0}|", text);
					return text;
				}

				var sb = new StringBuilder ();

				while (true) {
					var chunk = reader.ReadLine ();
					Server.Log ("CHUNK: {0}", chunk);
					var length = Int32.Parse (chunk, NumberStyles.HexNumber);

					if (length == 0) {
						reader.ReadLine ();
						return sb.ToString ();
					}

					var buffer = new char [length];
					int ret = reader.Read (buffer, 0, length);
					if (ret != length)
						throw new WebException ("Failed to read chunk");
					var text = new string (buffer);
					Server.Log ("CHUNK CONTENTS: |{0}|", text);
					sb.Append (text);
					if (reader.Peek () == 13)
						reader.Read ();
					if (reader.Peek () == 10)
						reader.Read ();
				}
			}

			public Response CreateResponse (HttpStatusCode status)
			{
				return new Response (this, status);
			}
		}

		public class Response {
			public Server Server {
				get;
				private set;
			}

			public HttpStatusCode Status {
				get;
				private set;
			}

			public Version Version {
				get;
				private set;
			}

			public Dictionary<string,string> Headers {
				get;
				private set;
			}

			public string Body {
				get; set;
			}

			public Response (Request request, HttpStatusCode status)
			{
				Status = status;
				Server = request.Server;
				Version = request.Version;
				Headers = new Dictionary<string, string> ();
			}

			public void Send ()
			{
				var writer = Server.Writer;
				writer.WriteLine ("HTTP/{0} {1} {2}", Version, (int)Status, Status);
				foreach (var header in Headers.Keys)
					writer.WriteLine ("{0}: {1}", header, Headers [header]);

				if ((Body != null) && !Headers.ContainsKey ("Content-Length")) {
					var length = Body.Length + Environment.NewLine.Length;
					writer.WriteLine ("Content-Length: {0}", length);
				}

				writer.WriteLine ();
				if (Body != null)
					writer.WriteLine (Body);

				writer.Flush ();
			}
		}
	}
}

