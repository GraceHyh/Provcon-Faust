using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace TestAuthentication
{
	public class Service
	{
		static void Main()
		{
			var host = new ServiceHost(typeof(MyService), new Uri("http://localhost:8088/Test"));

			var binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
			binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
			binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

			host.AddServiceEndpoint(typeof(IMyService), binding, "MyService.svc");

			host.Credentials.UserNameAuthentication.UserNamePasswordValidationMode = UserNamePasswordValidationMode.Custom;
			host.Credentials.UserNameAuthentication.CustomUserNamePasswordValidator = new MyUserNameValidator();

			host.Open();
			Console.WriteLine("Service running");
			foreach (var se in host.Description.Endpoints)
				Console.WriteLine(se.Address);
			Console.ReadLine();
			host.Close();
		}
	}
}
