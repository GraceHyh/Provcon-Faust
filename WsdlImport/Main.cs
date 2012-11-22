//
// Main.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Discovery;

using System.Collections.Generic;
using System.Runtime.Serialization;

using WS = System.Web.Services.Description;
using MT = MonoTests.System.ServiceModel;

using NUnit.Framework;

namespace WsdlImport {

	class Program {
		static void Main (string[] args)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				Utils.SaveMetadata ();

			MonoTests ();
			TestDefault ();
		}

		static void TestDefault ()
		{
			var test = new Test (Utils.EmbeddedResourceProvider);
			test.BasicHttpBinding ();
			test.BasicHttpsBinding ();
			test.BasicHttpsBinding2 ();
			test.NetTcpBinding ();
			test.BasicHttpBinding_ImportBinding ();
			test.BasicHttpBinding_ImportEndpoint ();
			test.BasicHttpBinding_ImportEndpoints ();
			test.BasicHttpBinding_Error ();
			test.BasicHttpBinding_Error2 ();
		}

		static void MonoTests ()
		{
			var path = Assembly.GetExecutingAssembly ().Location;
			Environment.CurrentDirectory = Path.GetDirectoryName (path);

			var http = new MT.BasicHttpBindingTest ();
			http.ApplyConfiguration ();
			http.Elements_MessageEncodingBindingElement ();
			http.Elements_TransportBindingElement ();
		}
	}
}
