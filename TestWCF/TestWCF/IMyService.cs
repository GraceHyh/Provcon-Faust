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

	[ServiceContract]
	public interface IMyService
	{
		[WebGet]
		[OperationContract]
		string Hello();

		[OperationContract]
		[FaultContract(typeof(ArgumentFault))]
		void TestException();
	}
}
