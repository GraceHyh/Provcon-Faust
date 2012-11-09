using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;

namespace WebRoot
{
	/// <summary>
	/// Summary description for TestService
	/// </summary>
	[WebService(Namespace = "http://provcon-faust/TestWCF/TestService.asmx/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[ScriptService]
	public class TestService : System.Web.Services.WebService
	{

		[WebMethod]
		[ScriptMethod (UseHttpGet=true, ResponseFormat=ResponseFormat.Xml)]
		public string HelloWorld()
		{
			return "Hello World";
		}

		[WebMethod]
		[ScriptMethod (ResponseFormat=ResponseFormat.Xml)]
		public string TestPost(string body)
		{
			var result = string.Format("Hello {0}", body);
			Console.WriteLine(result);
			return result;
		}
	}
}
