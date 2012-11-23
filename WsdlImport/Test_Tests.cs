//
// Testcases.cs
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
using System.Net;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

using WS = System.Web.Services.Description;

namespace WsdlImport {

	[TestFixture]
	public partial class Test {

		[Test]
		public void BasicHttpBinding ()
		{
			var doc = MetadataProvider.Get ("http.xml");
			
			var label = new TestLabel ("BasicHttpBinding");
			BasicHttpBinding (doc, BasicHttpSecurityMode.None, label);
		}
		
		[Test]
		public void BasicHttpBinding2 ()
		{
			var doc = MetadataProvider.Get ("http2.xml");
			
			var label = new TestLabel ("BasicHttpBinding2");
			BasicHttpBinding (doc, BasicHttpSecurityMode.Transport, label);
		}
		
		[Test]
		public void BasicHttpBinding3 ()
		{
			var doc = MetadataProvider.Get ("http3.xml");
			
			var label = new TestLabel ("BasicHttpBinding3");
			BasicHttpBinding (doc, BasicHttpSecurityMode.Message, label);
		}
		
		[Test]
		public void BasicHttpBinding4 ()
		{
			var doc = MetadataProvider.Get ("http4.xml");
			
			var label = new TestLabel ("BasicHttpBinding4");
			BasicHttpBinding (doc, BasicHttpSecurityMode.TransportWithMessageCredential, label);
		}
		
		[Test]
		public void BasicHttpBinding5 ()
		{
			var doc = MetadataProvider.Get ("http5.xml");
			
			var label = new TestLabel ("BasicHttpBinding5");
			BasicHttpBinding (doc, WSMessageEncoding.Mtom, label);
		}
		
		[Test]
		public void BasicHttpsBinding ()
		{
			var doc = MetadataProvider.Get ("https.xml");
			var label = new TestLabel ("BasicHttpsBinding");
			var binding = BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.None, AuthenticationSchemes.Anonymous,
				label);
		}
		
		[Test]
		public void BasicHttpsBinding2 ()
		{
			var doc = MetadataProvider.Get ("https2.xml");
			var label = new TestLabel ("BasicHttpsBinding2");
			var binding = BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Ntlm, AuthenticationSchemes.Ntlm,
				label);
		}
		
		[Test]
		public void BasicHttpsBinding3 ()
		{
			var doc = MetadataProvider.Get ("https3.xml");
			var label = new TestLabel ("BasicHttpsBinding3");
			BasicHttpsBinding (
				doc, BasicHttpSecurityMode.Transport, WSMessageEncoding.Text,
				HttpClientCredentialType.Certificate, AuthenticationSchemes.Anonymous,
				label);
		}
		
		[Test]
		public void BasicHttpsBinding4 ()
		{
			var doc = MetadataProvider.Get ("https4.xml");
			var label = new TestLabel ("BasicHttpsBinding4");
			BasicHttpsBinding (
				doc, BasicHttpSecurityMode.TransportWithMessageCredential,
				WSMessageEncoding.Text, HttpClientCredentialType.None,
				AuthenticationSchemes.Anonymous, label);
		}
		
		[Test]
		public void NetTcpBinding ()
		{
			var doc = MetadataProvider.Get ("net-tcp.xml");
			var label = new TestLabel ("NetTcpBinding");
			NetTcpBinding (
				doc, SecurityMode.None, false, TransferMode.Buffered, label);
		}
		
		[Test]
		public void NetTcpBinding2 ()
		{
			var doc = MetadataProvider.Get ("net-tcp2.xml");
			var label = new TestLabel ("NetTcpBinding2");
			NetTcpBinding (
				doc, SecurityMode.Transport, false,
				TransferMode.Buffered, label);
		}
		
		[Test]
		public void NetTcpBinding3 ()
		{
			var doc = MetadataProvider.Get ("net-tcp3.xml");
			var label = new TestLabel ("NetTcpBinding3");
			NetTcpBinding (
				doc, SecurityMode.None, true,
				TransferMode.Buffered, label);
		}
		
		[Test]
		public void NetTcpBinding4 ()
		{
			var doc = MetadataProvider.Get ("net-tcp4.xml");
			var label = new TestLabel ("NetTcpBinding4");
			NetTcpBinding (
				doc, SecurityMode.None, false,
				TransferMode.Streamed, label);
		}
		

	}

}

