using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TestWCF
{
	using Model;

	public class MyService : IMyService
	{
		public string Hello()
		{
			return "World";
		}

		public void TestException()
		{
			throw new ArgumentFaultException("Test");
		}
	}
}
