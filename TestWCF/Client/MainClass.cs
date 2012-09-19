using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestWCF.Client
{
	using ServiceReference;

	public class MainClass
	{
		static void Main()
		{
			var client = new MyServiceClient();
			client.Close();
		}
	}
}
