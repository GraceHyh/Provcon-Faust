//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin, Inc.
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

// #define FIDDLER

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ProvconFaust.TestProxyAuth
{
	class MainClass
	{
		public static void Main (string[] args)
		{
#if FIDDLER
			Setup (new Uri ("http://192.168.16.104:8888/"));
#else
			Setup (new Uri ("http://192.168.16.101:3128/"));
#endif

			Test ();
			Test2 ();
			TestGet ();
			TestPost ();
		}

		public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, 
		                              SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		static void Setup (Uri proxy_uri)
		{
			ServicePointManager.ServerCertificateValidationCallback = Validator;

			var ntlm_cred = new NetworkCredential ("test", "yeknom", "Provcon-Faust");
			var digest_cred = new NetworkCredential ("mono", "monkey", "Provcon-Faust");

			var cc = new CredentialCache ();
			cc.Add (proxy_uri, "NTLM", ntlm_cred);
			cc.Add (proxy_uri, "Digest", digest_cred);

			var proxy = new WebProxy (proxy_uri, false);
			proxy.Credentials = cc;

			WebRequest.DefaultWebProxy = proxy;
		}

		static void Test ()
		{
			var url = "https://en.wikipedia.org/wiki/Apple";

			var req = (HttpWebRequest)HttpWebRequest.Create (url);
			req.KeepAlive = true;
			req.ProtocolVersion = HttpVersion.Version11;
			req.Timeout = -1;

			var res = (HttpWebResponse)req.GetResponse ();
			Console.WriteLine (res.StatusCode);

			using (var reader = new StreamReader (res.GetResponseStream ())) {
				var text = reader.ReadToEnd ();
				Console.WriteLine ("Read {0} bytes.", text.Length);
			}
		}

		static void Test2 ()
		{
			var req = (HttpWebRequest)HttpWebRequest.Create ("https://github.com/mono/mono");
			req.Timeout = -1;
			
			var res = (HttpWebResponse)req.GetResponse ();
			Console.WriteLine (res.StatusCode);

			using (var reader = new StreamReader (res.GetResponseStream ())) {
				var text = reader.ReadToEnd ();
				Console.WriteLine ("Read {0} bytes.", text.Length);
			}
		}

		static void TestGet ()
		{
			var req = (HttpWebRequest)HttpWebRequest.Create ("https://192.168.16.101/TestWCF/");
			req.Timeout = -1;

			var res = (HttpWebResponse)req.GetResponse ();
			Console.WriteLine ("{0} {1}", (int)res.StatusCode, res.StatusDescription);

			using (var reader = new StreamReader (res.GetResponseStream ())) {
				var text = reader.ReadToEnd ();
				Console.WriteLine (text);
			}
		}

		static void TestPost ()
		{
			var req = (HttpWebRequest)HttpWebRequest.Create ("https://192.168.16.101/TestWCF/MyService.svc/rest/");
			req.Timeout = -1;
			req.Method = "POST";
			req.ContentType = "text/xml";

			using (var writer = new StreamWriter (req.GetRequestStream ())) {
				writer.WriteLine ("<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">Client Data</string>");
			}
			
			var res = (HttpWebResponse)req.GetResponse ();
			Console.WriteLine ("{0} {1}", (int)res.StatusCode, res.StatusDescription);
			
			using (var reader = new StreamReader (res.GetResponseStream ())) {
				var text = reader.ReadToEnd ();
				Console.WriteLine (text);
			}
		}

	}
}
