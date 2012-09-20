using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Selectors;

namespace TestAuthentication
{
	public class MyUserNameValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			Console.WriteLine("VALIDATE: {0} {1}", userName, password);
		}
	}
}
