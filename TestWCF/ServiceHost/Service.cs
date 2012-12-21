//
// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace TestWCF.ServiceHost
{
	class Host
	{
		static void Main()
		{
			var host = new System.ServiceModel.ServiceHost(
				typeof(MyService), new Uri ("http://localhost:9999/MyService"));
			host.AddServiceEndpoint(
				typeof(IMyService), new BasicHttpBinding(), "");

			var smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
			if (smb == null)
			{
				smb = new ServiceMetadataBehavior();
				smb.HttpGetEnabled = true;
				smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
				host.Description.Behaviors.Add(smb);
			}
			host.AddServiceEndpoint(
				ServiceMetadataBehavior.MexContractName,
				MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

			host.Open();
			Console.WriteLine("Service running");
			foreach (var se in host.Description.Endpoints)
				Console.WriteLine(se.Address);
			Console.ReadLine();
			host.Close();
		}
	}
}
