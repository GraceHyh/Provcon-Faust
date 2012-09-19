using System;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestWCF.Model
{
	[DataContract]
	public class ArgumentFault
	{
		[DataMember]
		public string ArgumentName
		{
			get;
			set;
		}
	}

	public class ArgumentFaultException
	    : FaultException<ArgumentFault>
	{
		public ArgumentFaultException(string message, string argumentName)
			: base(new ArgumentFault { ArgumentName = argumentName },
			new FaultReason(new FaultReasonText(
			    message
			    , new System.Globalization.CultureInfo("en-US"))),
			new FaultCode("ArgumentFault"))
		{ }

		public ArgumentFaultException(string argumentName)
			: this(String.Format("The value of the argument {0} is invalid!",
					     argumentName), argumentName)
		{ }
	}
}
