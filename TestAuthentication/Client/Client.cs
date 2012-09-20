using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace TestAuthentication
{
	public class MainClass
	{
		static void Main()
		{
			var address = new EndpointAddress(new Uri("http://localhost:8088/Test/MyService.svc"));
			var binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
			binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

			var factory = new ChannelFactory<IMyService>(binding, address);
			factory.Credentials.UserName.UserName = "test";
			factory.Credentials.UserName.Password = "monkey";
			var proxy = factory.CreateChannel();
			var result = proxy.Hello();
			Console.WriteLine(result);
		}
	}
}
