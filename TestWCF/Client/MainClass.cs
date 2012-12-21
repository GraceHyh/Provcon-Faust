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
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace TestWCF.Client
{
	using Service;

	public class MainClass
	{
		static void Main()
		{
			Setup();
			TestService();
			TestRestService(new Uri("http://provcon-faust/TestWCF/RestService/MyRestService.svc/"));
			TestRestService(new Uri("https://provcon-faust/TestWCF/RestService/MyRestService.svc/"));
			Console.WriteLine("Done!");
		}

		public static bool Validator (object sender, X509Certificate certificate, X509Chain chain, 
		                              SslPolicyErrors sslPolicyErrors)
		{
			return true;
		}

		static void Setup()
		{
			// ServicePointManager.ServerCertificateValidationCallback = Validator;
			// WebRequest.DefaultWebProxy = new WebProxy("192.168.16.104", 3128);
		}

		static void TestService()
		{
			var client = new MyServiceClient();
			var hello = client.Hello();
			Console.WriteLine(hello);
			client.Close();
		}

		static void TestRestService(Uri uri)
		{
			var getReq = HttpWebRequest.Create(uri);
			var getRes = getReq.GetResponse();

			string hello;
			using (var reader = new StreamReader(getRes.GetResponseStream()))
				hello = reader.ReadToEnd();

			var putReq = HttpWebRequest.Create(uri);
			putReq.ContentType = "text/xml";
			putReq.Method = "POST";

			using (var writer = new StreamWriter(putReq.GetRequestStream()))
				writer.WriteLine("<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">Client Data</string>");

			var putRes = (HttpWebResponse)putReq.GetResponse();
			Console.WriteLine(putRes.StatusCode);

			string response;
			using (var reader = new StreamReader(putRes.GetResponseStream()))
				response = reader.ReadToEnd();

			Console.WriteLine(response);
		}
	}
}
