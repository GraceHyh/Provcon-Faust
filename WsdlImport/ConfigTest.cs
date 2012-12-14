//
// ConfigTest.cs
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
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Configuration;
using System.Collections.Generic;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Configuration;

using NUnit.Framework;
using NUnit.Framework.Constraints;
using NUnit.Framework.SyntaxHelpers;

namespace WsdlImport {

	public class ConfigTest {

		static void Test ()
		{
			var b1 = new BasicHttpBinding ();
			var b2 = new BasicHttpBinding ();
			Assert.AreEqual (b1.TextEncoding, b2.TextEncoding, "#1");

			var element = new BasicHttpBindingElement ();
			Assert.AreEqual (element.TextEncoding, b1.TextEncoding, "#2");
		}

		public static bool ReadConfig (string filename)
		{
			if (!File.Exists (filename))
				return false;

			var doc = new XmlDocument ();
			doc.Load (filename);

			using (var writer = new XmlTextWriter (Console.Out)) {
				writer.Formatting = Formatting.Indented;
				doc.WriteTo (writer);
			}
			Console.WriteLine ();
			return true;
		}

		public static void CreateMachine (string filename)
		{
			if (File.Exists (filename))
				File.Delete (filename);

			using (var writer = XmlTextWriter.Create (filename)) {
				writer.WriteStartElement ("configuration");
				writer.WriteEndElement ();
			}

			var map = new ConfigurationFileMap (filename);
			var config = ConfigurationManager.OpenMappedMachineConfiguration (map);

#if FIXME
			var protectedData = (ProtectedConfigurationSection)config.Sections ["configProtectedData"];
			if (protectedData == null) {
				protectedData = new ProtectedConfigurationSection ();
				config.Sections.Add ("configProtectedData", protectedData);
			}
			var settings = new ProviderSettings ("RsaProtectedConfigurationProvider", typeof (RsaProtectedConfigurationProvider).AssemblyQualifiedName);
			protectedData.Providers.Add (settings);
			protectedData.DefaultProvider = "RsaProtectedConfigurationProvider";
#endif

			var my = new MySection ();
			my.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToRoamingUser;
			config.Sections.Add ("my", my);

			config.Save (ConfigurationSaveMode.Full);
			Utils.Dump (filename);
		}

		public delegate void TestFunction (Configuration config);
		public delegate void XmlCheckFunction (XPathNavigator nav);

		public static void Run (TestFunction func)
		{
			var filename = Path.GetTempFileName ();
			
			try {
				File.Delete (filename);
				
				var fileMap = new ExeConfigurationFileMap ();
				fileMap.ExeConfigFilename = filename;
				var config = ConfigurationManager.OpenMappedExeConfiguration (
					fileMap, ConfigurationUserLevel.None);
				
				func (config);
				
				config.Save (ConfigurationSaveMode.Modified);
				
				Console.WriteLine ();
				Console.WriteLine (filename);
				if (File.Exists (filename))
					Utils.Dump (filename);
				else
					Console.WriteLine ("<empty>");
			} finally {
				if (File.Exists (filename))
					File.Delete (filename);
			}
		}

		public static void RunWithMachineConfig (TestFunction func)
		{
			RunWithMachineConfig (func, null);
		}

		public static void RunWithMachineConfig (TestFunction func, XmlCheckFunction check)
		{
			var machine = Path.GetTempFileName ();
			var filename = Path.GetTempFileName ();

			try {
				File.Delete (machine);
				File.Delete (filename);

				CreateMachine (machine);

				Console.WriteLine (machine);
				Utils.Dump (machine);
				Console.WriteLine ();

				Assert.That (File.Exists (filename), Is.False);
					
				var fileMap = new ExeConfigurationFileMap ();
				fileMap.ExeConfigFilename = filename;
				fileMap.MachineConfigFilename = machine;
				var config = ConfigurationManager.OpenMappedExeConfiguration (
					fileMap, ConfigurationUserLevel.None);

				Assert.That (File.Exists (filename), Is.False);
				
				func (config);

				Console.WriteLine ();
				Console.WriteLine (filename);
				if (File.Exists (filename))
					Utils.Dump (filename);
				else
					Console.WriteLine ("<empty>");

				if (check == null)
					return;

				var xml = new XmlDocument ();
				xml.Load (filename);

				var nav = xml.CreateNavigator ().SelectSingleNode ("/configuration");
				check (nav);
			} finally {
				if (File.Exists (machine))
					File.Delete (machine);
				if (File.Exists (filename))
					File.Delete (filename);
			}
		}

		public static void TestNotModified ()
		{
			RunWithMachineConfig (config => {
				var my = config.Sections ["my"] as MySection;
				Assert.That (my, Is.Not.Null, "#1");
				Assert.That (my.IsModified, Is.False, "#2");
				Assert.That (my.List, Is.Not.Null, "#3");
				Assert.That (my.List.Collection.Count, Is.EqualTo (0), "#4");
				Assert.That (my.List.IsModified, Is.False, "#5");

				Assert.That (File.Exists (config.FilePath), Is.False, "#6");

				config.Save (ConfigurationSaveMode.Minimal);
				Assert.That (File.Exists (config.FilePath), Is.False, "#7");
			});
		}

		public static void TestNotModifiedAfterSave ()
		{
			RunWithMachineConfig (config => {
				var my = config.Sections ["my"] as MySection;
				Assert.That (my, Is.Not.Null, "#1a");
				Assert.That (my.IsModified, Is.False, "#1b");
				Assert.That (my.List, Is.Not.Null, "#1c");
				Assert.That (my.List.Collection.Count, Is.EqualTo (0), "#1d");
				Assert.That (my.List.IsModified, Is.False, "#1e");

				var element = my.List.Collection.AddElement ();
				Assert.That (my.IsModified, Is.True, "#2a");
				Assert.That (my.List.IsModified, Is.True, "#2b");
				Assert.That (my.List.Collection.IsModified, Is.True, "#2c");
				Assert.That (element.IsModified, Is.False, "#2d");

				config.Save ();
				Assert.That (File.Exists (config.FilePath), Is.True, "#3");

				Assert.That (my.IsModified, Is.False, "#3a");
				Assert.That (my.List.IsModified, Is.False, "#3b");
				Assert.That (my.List.Collection.IsModified, Is.False, "#3c");

				element.Hello = 1;
				Assert.That (element.IsModified, Is.True, "#4a");
				Assert.That (my.List.Collection.IsModified, Is.True, "#4b");
				Assert.That (my.List.IsModified, Is.True, "#4c");
				Assert.That (my.IsModified, Is.True, "#4d");

				config.Save ();
				Assert.That (element.IsModified, Is.False, "#5a");
				Assert.That (my.List.Collection.IsModified, Is.False, "#5b");
				Assert.That (my.List.IsModified, Is.False, "#5c");
				Assert.That (my.IsModified, Is.False, "#5d");
			});
		}

		public static void TestAddSection ()
		{
			Run (config => {
				config.Sections.Add ("my2", new MySection ());
				config.Save ();

				Assert.That (File.Exists (config.FilePath), Is.True, "#1");
			});
		}

		public static void TestAddElement_Modified ()
		{
			RunWithMachineConfig (config => {
				var my = config.Sections ["my"] as MySection;
				Assert.That (my, Is.Not.Null, "#c1a");
				Assert.That (my.IsModified, Is.False, "#c1b");

				my.List.DefaultCollection.AddElement ();
				config.Save (ConfigurationSaveMode.Modified);

				Assert.That (File.Exists (config.FilePath), Is.True, "#c2");
			}, nav => {
				Assert.That (nav.HasChildren, Is.True, "#x1");
				var iter = nav.SelectChildren (XPathNodeType.Element);

				Assert.That (iter.Count, Is.EqualTo (1), "#x2");
				Assert.That (iter.MoveNext (), Is.True, "#x2b");

				var my = iter.Current;
				Assert.That (my.Name, Is.EqualTo ("my"), "#x2c");
				Assert.That (my.HasAttributes, Is.False, "#x2d");
				Assert.That (my.HasChildren, Is.False, "#x2e");
			});
		}

		public static void TestAddElement_Minimal ()
		{
			RunWithMachineConfig (config => {
				var my = config.Sections ["my"] as MySection;
				Assert.That (my, Is.Not.Null, "#c1a");
				Assert.That (my.IsModified, Is.False, "#c1b");
				
				my.List.DefaultCollection.AddElement ();
				config.Save (ConfigurationSaveMode.Minimal);
				
				Assert.That (File.Exists (config.FilePath), Is.False, "#c2");
			});
		}

		public static void TestAddElement2 ()
		{
			RunWithMachineConfig (config => {
				var my = config.Sections ["my"] as MySection;
				Assert.That (my, Is.Not.Null, "#c1a");
				Assert.That (my.IsModified, Is.False, "#c1b");
				
				var element = my.List.DefaultCollection.AddElement ();
				element.Hello = 1;

				config.Save (ConfigurationSaveMode.Modified);
				
				Assert.That (File.Exists (config.FilePath), Is.True, "#c2");
			}, nav => {
				Assert.That (nav.HasChildren, Is.True, "#x1");
				var iter = nav.SelectChildren (XPathNodeType.Element);
				
				Assert.That (iter.Count, Is.EqualTo (1), "#x2");
				Assert.That (iter.MoveNext (), Is.True, "#x2b");
				
				var my = iter.Current;
				Assert.That (my.Name, Is.EqualTo ("my"), "#x2c");
				Assert.That (my.HasAttributes, Is.False, "#x2d");
				Assert.That (my.HasChildren, Is.True, "#x2e");

				var iter2 = my.SelectChildren (XPathNodeType.Element);
				Assert.That (iter2.Count, Is.EqualTo (1), "#x3a");
				Assert.That (iter2.MoveNext (), Is.True, "#x3b");

				var list = iter2.Current;
				Assert.That (list.Name, Is.EqualTo ("list"), "#x4a");
				Assert.That (list.HasChildren, Is.False, "#x4b");
				Assert.That (list.HasAttributes, Is.True, "#x4c");

				var attr = list.GetAttribute ("Hello", string.Empty);
				Assert.That (attr, Is.EqualTo ("1"), "#x4d");
			});
		}

		public static void Run ()
		{
			TestAddSection ();

			TestModified ();
			TestNotModified ();
			TestNotModifiedAfterSave ();

			TestAddElement_Minimal ();
			TestAddElement_Modified ();
			TestAddElement2 ();
		}

		public static void Run (string filename, string filename2)
		{
			CreateMachine (filename2);

			if (File.Exists (filename))
				File.Delete (filename);
			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			fileMap.MachineConfigFilename = filename2;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);

			var my = (MySection)config.Sections ["my"];
			// my.List.Collection.AddElement ().Hello = 12;

			config.Save (ConfigurationSaveMode.Modified);
			
			Console.WriteLine ("TEST: {0}", config.FilePath);
			
			Utils.Dump (filename);
		}

		public static void TestModified ()
		{
			var my = new MySection ();
			Assert.That (my.IsModified, Is.False, "#1");
			Assert.That (my.List, Is.Not.Null, "#2");
			Assert.That (my.List.IsModified, Is.False, "#3");
			Assert.That (my.List.Collection.IsModified, Is.False, "#4");
			Assert.That (my.List.DefaultCollection.IsModified, Is.False, "#5");

			var element = my.List.Collection.AddElement ();
			Assert.That (element.IsModified, Is.False, "#6");
			Assert.That (my.List.Collection.IsModified, Is.True, "#7");
			Assert.That (my.List.DefaultCollection.IsModified, Is.False, "#8");
			Assert.That (my.List.IsModified, Is.True, "#9");
			Assert.That (my.IsModified, Is.True, "#10");

			element.Hello = 8;
			Assert.That (element.IsModified, Is.True, "#11");
		}

		public class MyElement : ConfigurationElement {
			[ConfigurationProperty ("Hello", DefaultValue = 8)]
			public int Hello {
				get { return (int)base ["Hello"]; }
				set { base ["Hello"] = value; }
			}

			[ConfigurationProperty ("World", IsRequired = false)]
			public int World {
				get { return (int)base ["World"]; }
				set { base ["World"] = value; }
			}

			new public bool IsModified {
				get { return base.IsModified (); }
			}
		}

		public class MyCollection<T> : ConfigurationElementCollection
			where T : ConfigurationElement, new ()
		{
			#region implemented abstract members of ConfigurationElementCollection
			protected override ConfigurationElement CreateNewElement ()
			{
				return new T ();
			}
			protected override object GetElementKey (ConfigurationElement element)
			{
				return ((T)element).GetHashCode ();
			}
			#endregion

			public override ConfigurationElementCollectionType CollectionType {
				get {
					return ConfigurationElementCollectionType.BasicMap;
				}
			}

			public T AddElement ()
			{
				var element = new T ();
				BaseAdd (element);
				return element;
			}

			public new bool IsModified {
				get { return base.IsModified (); }
			}
		}

		public class MyCollectionElement<T> : ConfigurationElement
			where T : ConfigurationElement, new ()
		{
			[ConfigurationProperty ("",
			                        Options = ConfigurationPropertyOptions.IsDefaultCollection,
			                        IsDefaultCollection = true)]
			public MyCollection<T> DefaultCollection {
				get { return (MyCollection<T>)this [String.Empty]; }
				set { this [String.Empty] = value; }
			}

			[ConfigurationProperty ("collection", Options = ConfigurationPropertyOptions.None)]
			public MyCollection<T> Collection {
				get { return (MyCollection<T>)this ["collection"]; }
				set { this ["collection"] = value; }
			}

			public new bool IsModified {
				get { return base.IsModified (); }
			}
		}

		public class MySection : ConfigurationSection {
			[ConfigurationProperty ("list", Options = ConfigurationPropertyOptions.None)]
			public MyCollectionElement<MyElement> List {
				get { return (MyCollectionElement<MyElement>) this ["list"]; }
			}

			new public bool IsModified {
				get { return base.IsModified (); }
			}
		}
	}
}

