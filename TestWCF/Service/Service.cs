using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace TestWCF.Service
{
	class Service
	{
		static void Main()
		{
			var host = new ServiceHost(typeof(MyService));
			host.Open();
			Console.WriteLine("Service running");
			foreach (var se in host.Description.Endpoints)
				Console.WriteLine(se.Address);
			Console.ReadLine();
			host.Close();
		}
	}
}
