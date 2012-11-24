//
// Server.cs
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
using System.ServiceModel;
using System.ServiceModel.Description;
using WS = System.Web.Services.Description;

namespace WsdlImport {

	public class Server : IMyService {

		public static void Run ()
		{
			// Open post as non-admin:
			// http://msdn.microsoft.com/en-us/library/ms733768.aspx
			// netsh http add urlacl url=http://+:9999/ user='PROVCON-FAUST\martin'

			var host = new ServiceHost (typeof (Server));
			AddMexEndpoint (host);
			host.AddServiceEndpoint (
				typeof (IMyService), new BasicHttpBinding (),
				new Uri ("http://provcon-faust:9999/service/"));
			host.AddServiceEndpoint (
				typeof (IMyService), new BasicHttpBinding (BasicHttpSecurityMode.Transport),
				new Uri ("https://provcon-faust:9998/secureservice/"));
			AddNetTcp (host);
			host.Open ();

			foreach (var endpoint in host.Description.Endpoints)
				Console.WriteLine (endpoint.Address);

			Console.WriteLine ("Service running.");
			Console.ReadLine ();

			host.Close ();
		}

		static void AddNetTcp (ServiceHost host)
		{
			var binding = new NetTcpBinding (SecurityMode.None);
			binding.Security.Message.ClientCredentialType = MessageCredentialType.None;
			host.AddServiceEndpoint (
				typeof (IMyService), binding,
				new Uri ("net.tcp://provcon-faust:9000/"));
		}

		static void AddNetTcp2 (ServiceHost host)
		{
			var binding = new NetTcpBinding (SecurityMode.None);
			binding.Security.Message.ClientCredentialType = MessageCredentialType.UserName;
			host.AddServiceEndpoint (
				typeof (IMyService), binding,
				new Uri ("net.tcp://provcon-faust:9001/"));
		}

		// http://msdn.microsoft.com/en-us/library/aa738489.aspx
		static void AddMexEndpoint (ServiceHost host)
		{
			var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior> ();
			if (smb == null)
				smb = new ServiceMetadataBehavior ();
			smb.HttpGetEnabled = true;
			smb.HttpGetUrl = new Uri ("http://provcon-faust:9999/");
			// smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
			host.Description.Behaviors.Add (smb);

			// Add MEX endpoint
			host.AddServiceEndpoint (
				ServiceMetadataBehavior.MexContractName,
				MetadataExchangeBindings.CreateMexHttpBinding (),
				"http://provcon-faust:9999/");
		}

		public string Hello ()
		{
			return "Hello World!";
		}

	}
}

