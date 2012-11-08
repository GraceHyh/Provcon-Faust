//
// MyNtlmClient.cs
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
using System.Diagnostics;
using System.Net;
using Mono.Security.Protocol.Ntlm;

namespace ProvconFaust.TestAuthentication {

	public class MyNtlmClient : IAuthenticationModule {

		IAuthenticationModule ntlm;
		Process pipe;

		#region IAuthenticationModule implementation

		public Authorization Authenticate (string challenge, WebRequest request,
		                                   ICredentials credentials)
		{
			if (!challenge.StartsWith ("NTLM"))
				return null;

			Console.WriteLine ("AUTHENTICATE: {0} {1}", challenge, request);

			pipe.StandardInput.WriteLine ("TT{0}", challenge.Substring (4));
			var result = pipe.StandardOutput.ReadLine ();
			Console.WriteLine (result);

			if (result.StartsWith ("YR "))
				return ntlm.Authenticate (challenge, request, credentials);

			if (!result.StartsWith ("AF "))
				return null;

			var bytes = Convert.FromBase64String (result.Substring (3));
			var type3 = new Type3Message (bytes);
			Console.WriteLine (type3.LM.Length);

			var bytes2 = type3.GetBytes2 ();

			Utils.HexDump (bytes);
			Utils.HexDump (bytes2);

			Utils.Compare (bytes, bytes2);

			var auth = new Authorization ("NTLM " + Convert.ToBase64String (bytes2));
			Console.WriteLine (auth.Message);
			return auth;
		}

		public Authorization PreAuthenticate (WebRequest request, ICredentials credentials)
		{
			Console.WriteLine ("PRE AUTHENTICATE");
			return null;
		}

		public string AuthenticationType {
			get {
				return "NTLM";
			}
		}

		public bool CanPreAuthenticate {
			get {
				return false;
			}
		}

		#endregion

		public MyNtlmClient (IAuthenticationModule ntlm)
		{
			this.ntlm = ntlm;

			var path = "/Workspace/samba-3.6.9/source3/bin/ntlm_auth";

			var psi = new ProcessStartInfo (
				path, "--helper-protocol=ntlmssp-client-1 --debuglevel=10 " +
				"--diagnostics --username=test --password=yeknom --domain=PROVCON-FAUST " +
				"--workstation=PROVCON-FAUST --configfile=/usr/local/etc/smb.conf");
			psi.RedirectStandardError = false;
			psi.RedirectStandardInput = true;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;

			pipe = Process.Start (psi);

			Console.WriteLine (pipe.Id);
			// Console.ReadLine ();

			pipe.StandardInput.WriteLine ("SF NTLMSSP_NEGOTIATE_56");
			var result = pipe.StandardOutput.ReadLine ();
			Console.WriteLine (result);
		}
	}
}

