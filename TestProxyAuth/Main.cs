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

using System;
using System.Net;

namespace ProvconFaust.TestProxyAuth
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Setup ();
			Test ();
		}

		static void Setup ()
		{
			var proxy_uri = new Uri ("http://192.168.16.101:3128/");

			var ntlm_cred = new NetworkCredential ("test", "monkey", "Provcon-Faust");
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
			var req = (HttpWebRequest)HttpWebRequest.Create ("https://www.google.com/");

			try {
				var res = (HttpWebResponse)req.GetResponse ();
				Console.WriteLine (res.StatusCode);
			} catch (Exception ex) {
				Console.WriteLine ("EX: {0}", ex);
			}
		}
	}
}
