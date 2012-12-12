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
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Discovery;

using System.Collections.Generic;
using System.Runtime.Serialization;

using WS = System.Web.Services.Description;

using NUnit.Framework;
using Mono.Options;

namespace WsdlImport {

	class Program {
		enum Mode {
			Default,
			Export,
			Server,
			Client
		}

		static void Main (string[] args)
		{
			string cache = null;
			Mode mode = Mode.Default;
			var options = new OptionSet ();
			options.Add ("mode=", m => mode = (Mode)Enum.Parse (typeof (Mode), m, true));
			options.Add ("cache=", c => cache = c);
			options.Parse (args);

			switch (mode) {
			case Mode.Export:
				// MetadataSamples.Export ();
				return;

			case Mode.Server:
				Server.Run ();
				return;

			case Mode.Client:
				Client.Run (new Uri ("http://provcon-faust:9999/?singleWsdl"), cache);
				return;

			default:
				TestConfig ();
				ConfigTest.Run ("my.config");
				return;
			}
		}

		static void TestConfig ()
		{
			var binding = new BasicHttpBinding ();
			binding.Name = "Test";
			Utils.CreateConfig (binding, "test.config");
			// Utils.NormalizeConfig ("test.config");
			// CheckConfig ();
		}

		static void CheckConfig ()
		{
			var doc = new XPathDocument ("test2.config");
			var nav = doc.CreateNavigator ();

			var iter = nav.Select ("/configuration/system.serviceModel/bindings/*");
			Console.WriteLine ("SELECT: {0} - {1} {2}", iter, iter.Count, iter.Current);
			foreach (XPathNavigator binding in iter) {
				Console.WriteLine ("TEST: {0} - {1} {2} {3} {4}", binding.GetType (),
				                   binding.HasChildren, binding.OuterXml, binding.Value,
				                   binding.HasAttributes);
			}
		}

		static void TestConfigSection ()
		{
			var filename = "test.config";
			if (File.Exists (filename))
				File.Delete (filename);
			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);
			
			var section = (BindingsSection)config.GetSection ("system.serviceModel/bindings");
			var element = new BasicHttpBindingElement ("Test");
			section.BasicHttpBinding.Bindings.Add (element);
			
			config.Save (ConfigurationSaveMode.Minimal);

			Console.WriteLine ("TEST: {0}", config.FilePath);

			Utils.Dump (filename);
		}
	}
}
