//
// NtlmAuthHelper.cs
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
using System.Diagnostics;
using System.Collections.Generic;
using Mono.Security.Protocol.Ntlm;

namespace ProvconFaust.TestAuthentication {

	public class NtlmAuthHelper {
		Process helper;

		public NtlmAuthHelper ()
			: this ("/Workspace/samba-3.6.9/source3/bin/ntlm_auth", "squid-2.5-ntlmssp",
			        "--configfile=/usr/local/etc/smb.conf")
		{
		}

		public NtlmAuthHelper (string path, string protocol, params string[] extra_args)
		{
			var args = new List<string> ();
			args.Add ("--helper-protocol=" + protocol);
			// args.Add ("--diagnostics");
			// args.Add ("--username=test");
			// args.Add ("--password=yeknom");
			args.Add ("--domain=PROVCON-FAUST");
			args.Add ("--workstation=PROVCON-FAUST");
			args.AddRange (extra_args);
			var psi = new ProcessStartInfo (path, string.Join (" ", args));

			psi.RedirectStandardError = false;
			psi.RedirectStandardInput = true;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;
			
			helper = Process.Start (psi);
			Console.WriteLine (helper.Id);
			Console.ReadLine ();
		}

		public void Run (string username, string password)
		{
			Console.WriteLine ("=========");

			helper.StandardInput.WriteLine ("SF NTLMSSP_FEATURE_SESSION_KEY");
			var sf_response = helper.StandardOutput.ReadLine ();
			Console.WriteLine (sf_response);
			if (sf_response != "OK")
				throw new InvalidDataException (sf_response);

			var pw_bytes = Encoding.ASCII.GetBytes (password);
			helper.StandardInput.WriteLine ("PW " + Convert.ToBase64String (pw_bytes));
			var pw_result = helper.StandardOutput.ReadLine ();
			if (pw_result != "OK")
				throw new InvalidDataException (pw_result);

			var type1 = new Type1Message ();
			type1.Flags |= NtlmFlags.NegotiateNtlm2Key;
			helper.StandardInput.WriteLine ("KK " + Convert.ToBase64String (type1.GetBytes ()));
			var type1_res = helper.StandardOutput.ReadLine ();
			if (!type1_res.StartsWith ("TT "))
				throw new InvalidDataException ();

			var type2 = new Type2Message (Convert.FromBase64String (type1_res.Substring (3)));
			Console.WriteLine ("TYPE2: {0:x} {1}", type2.Flags, type2.Flags);

			var type3 = new Type3Message (type2);
			type3.Domain = "SOL";
			type3.Host = "PROVCON-FAUST";
			type3.Username = username;
			type3.Password = password;

			var bytes = type3.GetBytes ();

			helper.StandardInput.WriteLine ("KK {0}", Convert.ToBase64String (bytes));

			var response2 = helper.StandardOutput.ReadLine ();
			Console.WriteLine (response2);
			if (!response2.StartsWith ("AF "))
				throw new InvalidDataException (response2);
		}
	}
}

