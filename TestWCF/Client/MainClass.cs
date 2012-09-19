using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace TestWCF.Client
{
	using ServiceReference;
	using Model;

	public class MainClass
	{
		static void Main()
		{
			var client = new MyServiceClient();
			var hello = client.Hello();
			Console.WriteLine("HELLO: {0}", hello);

			try
			{
				client.TestException();
			}
			catch (ArgumentFaultException ex)
			{
				Console.WriteLine("GOT CUSTOM ARGUMENT FAULT EX: {0}", ex.Message);
			}
			catch (FaultException<ArgumentFault> ex)
			{
				Console.WriteLine("GOT ARGUMENT FAULT EX: {0}", ex.Message);
			}
			catch (FaultException ex)
			{
				Console.WriteLine("GOT FAULT EX: {0}", ex.Message);
			}
			client.Close();
		}
	}
}
