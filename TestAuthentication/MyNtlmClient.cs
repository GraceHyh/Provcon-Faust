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
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using Mono.Security.Protocol.Ntlm;

namespace ProvconFaust.TestAuthentication {

#if MY_NTLM_CLIENT

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

			var t2_bytes = Convert.FromBase64String (challenge.Substring (5));
			var type2 = new Type2Message (t2_bytes);
			Utils.HexDump (t2_bytes);
			Console.WriteLine ("TYPE2: {0}", type2.TargetName);

			var bytes = Convert.FromBase64String (result.Substring (3));
			var type3 = new Type3Message (bytes);

			long timestamp;
			byte[] nonce, namesBlob;
			Dump (type2, type3, out timestamp, out nonce, out namesBlob);

			byte[] nt = ChallengeResponse2.Compute_NTLMv2 (
				type2, "test", "yeknom", timestamp, nonce);

			var ok = Utils.Compare (type3.NT, nt);
			Console.WriteLine ("COMPARE: {0} - {1:x}", ok, type3.Flags);
			type3.NT = nt;

			// Dump (type2, type3);

			type3.Flags = (NtlmFlags)0x00088201;

			var bytes2 = type3.GetBytes2 ();

			var auth = ntlm.Authenticate (challenge, request, credentials);
			Console.WriteLine (auth.Message);

			var bytes3 = Convert.FromBase64String (auth.Message.Substring (5));
			var test = new Type3Message (bytes3);

			Console.WriteLine ("TEST: {0:x} - {1} {2}", test.Flags,
			                   bytes2.Length, bytes3.Length);

			Console.WriteLine ("TEST #1: {0} {1} {2} - {3} {4} {5}",
			                   type3.Host, type3.Domain, type3.Username,
			                   test.Host, test.Domain, test.Username);

			Console.WriteLine ("TEST #2: {0} {1} {2}", test.LM != null, test.NT.Length,
			                   type3.NT.Length);

			// test.NT = type3.NT;

			// test.Host = type3.Host;
			test.Domain = type3.Domain;
			// test.Flags = type3.Flags;

			var bytes4 = test.GetBytes2 ();

			// Utils.HexDump (bytes);
			// Utils.HexDump (bytes2);
			// Utils.Compare (bytes2, bytes3);

			// auth = new Authorization ("NTLM " + Convert.ToBase64String (bytes4));
			Console.WriteLine (auth.Message);
			return auth;
		}

		static void Dump (Type2Message type2, Type3Message type3,
		                  out long timestamp, out byte[] nonce, out byte[] namesBlob)
		{
			Console.WriteLine ();
			Console.WriteLine ("DUMP:");
			Console.WriteLine ("=====");
			var ntlm_hash = ChallengeResponse2.Compute_NTLM_Password ("yeknom");
			Utils.HexDump ("NTLM HASH", ntlm_hash);
			
			var ubytes = Encoding.Unicode.GetBytes ("TEST");
			var tbytes = Encoding.Unicode.GetBytes ("PROVCON-FAUST");
			
			var bytes = new byte [ubytes.Length + tbytes.Length];
			ubytes.CopyTo (bytes, 0);
			Array.Copy (tbytes, 0, bytes, ubytes.Length, tbytes.Length);
			
			var md5 = new HMACMD5 (ntlm_hash);
			var ntlmv2_hash = md5.ComputeHash (bytes);
			Utils.HexDump ("NTLM V2 HASH", ntlmv2_hash);

			var ntlmv2_md5 = new HMACMD5 (ntlmv2_hash);

			Utils.HexDump (type3.NT);
			var br = new BinaryReader (new MemoryStream (type3.NT));
			var hash = br.ReadBytes (16);
			Utils.HexDump (hash);

			if (br.ReadInt32 () != 0x0101)
				throw new InvalidDataException ();
			if (br.ReadInt32 () != 0)
				throw new InvalidDataException ();

			timestamp = br.ReadInt64 ();
			var ticks = timestamp + 504911232000000000;
			Console.WriteLine ("TIMESTAMP: {0} {1}", timestamp, new DateTime (ticks));

			nonce = br.ReadBytes (8);
			Utils.HexDump ("NONCE", nonce);

			br.ReadInt32 ();

			var pos = br.BaseStream.Position;

			while (true) {
				var type = br.ReadInt16 ();
				var length = br.ReadInt16 ();
				Console.WriteLine ("NAMES BLOB: {0:x} {1:x}", type, length);
				if (type == 0)
					break;
				var contents = br.ReadBytes (length);
				Utils.HexDump (contents);
			}

			namesBlob = new byte [br.BaseStream.Position - pos];
			Array.Copy (type3.NT, pos, namesBlob, 0, namesBlob.Length);

			var blob = new byte [type3.NT.Length - 16];
			Array.Copy (type3.NT, 16, blob, 0, blob.Length);

			Utils.HexDump ("TYPE 2 CHALLENGE", type2.Nonce);

			var buffer = new byte [type2.Nonce.Length + blob.Length];
			type2.Nonce.CopyTo (buffer, 0);
			blob.CopyTo (buffer, type2.Nonce.Length);

			Utils.HexDump (blob);

			var test = ntlmv2_md5.ComputeHash (buffer);
			Utils.HexDump ("THE HASH", test);
			var ok = Utils.Compare (hash, test);
			Console.WriteLine (ok);

			Console.WriteLine ();
			Console.WriteLine ("==========");
			Console.WriteLine ();
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

			// Console.WriteLine (pipe.Id);
			// Console.ReadLine ();

			pipe.StandardInput.WriteLine ("SF NTLMSSP_NEGOTIATE_56");
			var result = pipe.StandardOutput.ReadLine ();
			Console.WriteLine (result);
		}
	}
#endif
}

