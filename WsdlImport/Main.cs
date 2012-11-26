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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Discovery;

using System.Collections.Generic;
using System.Runtime.Serialization;

using WS = System.Web.Services.Description;

using NUnit.Framework;
using Mono.Options;

namespace WsdlImport {

	class Program {
		enum Mode {
			Default,
			Export,
			Server,
			Client
		}

		static void Main (string[] args)
		{
			string cache = null;
			Mode mode = Mode.Default;
			var options = new OptionSet ();
			options.Add ("mode=", m => mode = (Mode)Enum.Parse (typeof (Mode), m, true));
			options.Add ("cache=", c => cache = c);
			options.Parse (args);

			switch (mode) {
			case Mode.Export:
				MetadataSamples.Export ();
				return;

			case Mode.Server:
				Server.Run ();
				return;

			case Mode.Client:
				Client.Run (new Uri ("http://provcon-faust:9999/?singleWsdl"), cache);
				return;

			default:
				TestDefault ();
				TestConfig ();
				return;
			}
		}

		static void TestConfig ()
		{
			var binding = new BasicHttpBinding ();
			Console.WriteLine ("TEST: {0}", binding.Name);
			Utils.CreateConfig (binding, "test.config");

		}

		static void TestDefault ()
		{
			var test = new Test ();
			test.BasicHttp ();
			test.BasicHttp_Mtom ();
			test.BasicHttp_TransportSecurity ();
			test.BasicHttp_NtlmAuth ();

			test.NetTcp ();
			test.NetTcp_TransferMode ();
			test.NetTcp_TransportSecurity ();
			// test.NetTcp_MessageSecurity ();
			// test.NetTcp_Binding ();
			// test.NetTcp_TransportWithMessageCredential ();

#if NET_4_5
			test.BasicHttps ();
			test.BasicHttps_NtlmAuth ();
			test.BasicHttps_Certificate ();
#endif

#if FIXME
			test.BasicHttpBinding ();
			test.BasicHttpBinding2 ();
			test.BasicHttpBinding3 ();
			test.BasicHttpBinding4 ();
			test.BasicHttpBinding5 ();
			test.BasicHttpsBinding ();
			test.BasicHttpsBinding2 ();
			test.BasicHttpsBinding3 ();
			test.BasicHttpsBinding4 ();
			test.NetTcp ();
			test.NetTcp_TransportSecurity ();
			test.NetTcpBinding3 ();
			test.NetTcp_TransferMode ();
			test.BasicHttpBinding_ImportBinding ();
			test.BasicHttpBinding_ImportEndpoint ();
			test.BasicHttpBinding_ImportEndpoints ();
			test.BasicHttpBinding_Error ();
			test.BasicHttpBinding_Error2 ();
#endif
		}
	}
}
