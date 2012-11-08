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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Mono.Security.Protocol.Ntlm;

namespace ProvconFaust.TestAuthentication
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			// Decode_Type2 (type2_message);
			// Decode_Type3 (type3_message);

			Setup (new Uri ("http://192.168.16.101:8888/"));
			Test ();

			// Compute_Type3 ();
		}

		// Host: PROVCON-FAUST
		// Domain: <empty>
		// Username: test
		// Password: yeknom

		const string type2_message = "TlRMTVNTUAACAAAAGgAaADgAAAAFgoqi" +
			"MXN7SA+F8Z0AAAAAAAAAAIgAiABSAAAABgGxHQAAAA9QAFIATwBWAE" +
			"MATwBOAC0ARgBBAFUAUwBUAAIAGgBQAFIATwBWAEMATwBOAC0ARgBB" +
			"AFUAUwBUAAEAGgBQAFIATwBWAEMATwBOAC0ARgBBAFUAUwBUAAQAGg" +
			"BQAHIAbwB2AGMAbwBuAC0ARgBhAHUAcwB0AAMAGgBQAHIAbwB2AGMA" +
			"bwBuAC0ARgBhAHUAcwB0AAcACABGXoF/hbnNAQAAAAA=";

		const string type3_message = "TlRMTVNTUAADAAAAGAAYAHoAAAA0ATQBk" +
			"gAAAAAAAABYAAAACAAIAFgAAAAaABoAYAAAAAAAAADGAQAABYKIogYB" +
			"sR0AAAAPsWcYJbDdinmFcsHxB8UHVXQAZQBzAHQAUABSAE8AVgBDAE8" +
			"ATgAtAEYAQQBVAFMAVAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABGgc" +
			"ZC7Ijvcj8k7ZFQG3lIAQEAAAAAAABGXoF/hbnNAZJ12yGYs6TTAAAAA" +
			"AIAGgBQAFIATwBWAEMATwBOAC0ARgBBAFUAUwBUAAEAGgBQAFIATwBW" +
			"AEMATwBOAC0ARgBBAFUAUwBUAAQAGgBQAHIAbwB2AGMAbwBuAC0ARgB" +
			"hAHUAcwB0AAMAGgBQAHIAbwB2AGMAbwBuAC0ARgBhAHUAcwB0AAcACA" +
			"BGXoF/hbnNAQYABAACAAAACAAwADAAAAAAAAAAAAAAAAAwAACQeoq9X" +
			"sdVRq7asvn+HAO4IBXY6F0iUDsYp0Er34UjmwoAEAAAAAAAAAAAAAAA" +
			"AAAAAAAACQAkAEgAVABUAFAALwBwAHIAbwB2AGMAbwBuAC0AZgBhAHU" +
			"AcwB0AAAAAAAAAAAAAAAAAA==";

		public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, 
		                              SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		static void HexDump (byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++) {
				if ((i % 8) == 0) {
					Console.WriteLine ();
					Console.Write ("{0:x4} ", i);
				}
				Console.Write ("{0:x2} ", buffer [i]);
			}
			Console.WriteLine ();
			Console.WriteLine ();
		}

		static void HexDump (string name, byte[] buffer)
		{
			Console.Write ("{0}: ", name);
			for (int i = 0; i < buffer.Length; i++) {
				Console.Write ("{0:x2} ", buffer [i]);
			}
			Console.WriteLine ();
		}
		
		static void Decode_Type2 (string text)
		{
			var bytes = Convert.FromBase64String (text);
			HexDump (bytes);
			var message = new Type2Message (bytes);
			Console.WriteLine ("TYPE 2: {0:x} {1:x}",
			                   message.Type, message.Flags);
		}

		static void Decode_Type3 (string text)
		{
			var bytes = Convert.FromBase64String (text);
			HexDump (bytes);
			var message = new Type3Message (bytes);
			Console.WriteLine ("TYPE 3: {0:x} {1:x}",
			                   message.Type, message.Flags);

			HexDump ("LM", message.LM);
			HexDump ("LT", message.NT);
		}

		static void Compute_Type3 ()
		{
			Decode_Type3 (type3_message);
			Console.WriteLine ();

			var bytes = Convert.FromBase64String (type2_message);
			var message = new Type2Message (bytes);
			Compute_Type3 (message);
		}

		static void Compute_Type3 (Type2Message type2)
		{
			Type3Message type3 = new Type3Message ();
			type3.Domain = "";
			type3.Host = "PROVCON-FAUST";
			type3.Username = "test";
			type3.Challenge = type2.Nonce;
			type3.Password = "yeknom";

			HexDump ("CHALLENGE", type2.Nonce);

			var bytes = type3.GetBytes ();

			var message = new Type3Message (bytes);
			HexDump ("LM", message.LM);
			HexDump ("NT", message.NT);
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
			// proxy.Credentials = cc;

			WebRequest.DefaultWebProxy = proxy;

			IAuthenticationModule ntlm = null;

			var modules = AuthenticationManager.RegisteredModules;
			while (modules.MoveNext ()) {
				var module = (IAuthenticationModule)modules.Current;
				if (module.AuthenticationType == "NTLM") {
					ntlm = module;
					break;
				}
			}
			if (ntlm == null)
				throw new InvalidOperationException ();

			AuthenticationManager.Register (new MyNtlmClient (ntlm));
		}

		static void Test ()
		{
			var uri = new Uri ("http://provcon-faust:81/");

			var req = (HttpWebRequest)HttpWebRequest.Create (uri);
			req.KeepAlive = true;
			req.ProtocolVersion = HttpVersion.Version11;
			req.Timeout = -1;

			var cc = new CredentialCache ();
			var cred = new NetworkCredential ("test", "yeknom");
			cc.Add (uri, "NTLM", cred);
			req.Credentials = cred;

			var res = (HttpWebResponse)req.GetResponse ();
			Console.WriteLine (res.StatusCode);

			using (var reader = new StreamReader (res.GetResponseStream ())) {
				var text = reader.ReadToEnd ();
				Console.WriteLine ("Read {0} bytes.", text.Length);
			}
		}
	}
}
