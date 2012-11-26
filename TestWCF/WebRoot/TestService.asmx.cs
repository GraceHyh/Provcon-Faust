using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using QName = System.Xml.XmlQualifiedName;

namespace WebRoot
{
	/// <summary>
	/// Summary description for TestService
	/// </summary>
	[WebService(Namespace = TestService.Namespace)]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[ScriptService]
	public class TestService : System.Web.Services.WebService
	{
		const string Namespace = "http://provcon-faust/TestWCF/TestService.asmx/";

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

		[WebMethod]
		[ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Xml)]
		public string TestFault()
		{
			throw new SoapException("Testing Soap Fault", new QName("TestFault", Namespace));
		}
	}
}
