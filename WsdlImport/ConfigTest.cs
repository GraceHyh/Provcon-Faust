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
using System.Xml.Schema;
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

using MonoTests.System.ServiceModel.MetadataTests;

namespace WsdlImport {

	public class ConfigTest {

		public static void Run ()
		{
			var test = new ConfigTest ();
			var bf = BindingFlags.Instance | BindingFlags.Public;
			foreach (var method in typeof (ConfigTest).GetMethods (bf)) {
				var cattr = method.GetCustomAttribute<TestAttribute> ();
				if (cattr == null)
					continue;
				Console.WriteLine ("Running {0}.", method.Name);
				method.Invoke (test, new object [0]);
			}
		}

		public static void Test ()
		{
		}

		#region Test Framework

		public abstract class ConfigProvider {
			public void Create (string filename)
			{
				if (File.Exists (filename))
					File.Delete (filename);
				
				var settings = new XmlWriterSettings ();
				settings.Indent = true;
				
				using (var writer = XmlTextWriter.Create (filename, settings)) {
					writer.WriteStartElement ("configuration");
					WriteXml (writer);
					writer.WriteEndElement ();
				}
			}

			public abstract bool IsMachineConfig {
				get;
			}

			protected abstract void WriteXml (XmlWriter writer);
		}

		public abstract class MachineConfigProvider : ConfigProvider {
			protected override void WriteXml (XmlWriter writer)
			{
				writer.WriteStartElement ("configSections");
				WriteSections (writer);
				writer.WriteEndElement ();
				WriteValues (writer);
			}

			public override bool IsMachineConfig {
				get { return true; }
			}
			
			protected abstract void WriteSections (XmlWriter writer);

			protected abstract void WriteValues (XmlWriter writer);
		}

		class DefaultMachineConfig : MachineConfigProvider {
			protected override void WriteSections (XmlWriter writer)
			{
				writer.WriteStartElement ("section");
				writer.WriteAttributeString ("name", "my");
				writer.WriteAttributeString ("type", typeof (MySection).AssemblyQualifiedName);
				writer.WriteAttributeString ("allowLocation", "true");
				writer.WriteAttributeString ("allowDefinition", "Everywhere");
				writer.WriteAttributeString ("allowExeDefinition", "MachineToRoamingUser");
				writer.WriteAttributeString ("restartOnExternalChanges", "true");
				writer.WriteAttributeString ("requirePermission", "true");
				writer.WriteEndElement ();
			}

			internal static void WriteConfigSections (XmlWriter writer)
			{
				var provider = new DefaultMachineConfig ();
				writer.WriteStartElement ("configSections");
				provider.WriteSections (writer);
				writer.WriteEndElement ();
			}

			protected override void WriteValues (XmlWriter writer)
			{
				writer.WriteStartElement ("my");
				writer.WriteEndElement ();
			}
		}

		class TestConfig : ConfigProvider {
			public override bool IsMachineConfig {
				get { return false; }
			}

			protected override void WriteXml (XmlWriter writer)
			{
				DefaultMachineConfig.WriteConfigSections (writer);
				writer.WriteStartElement ("my");
				writer.WriteStartElement ("test");
				writer.WriteAttributeString ("Hello", "29");
				writer.WriteEndElement ();
				writer.WriteEndElement ();
			}
		}

		public delegate void TestFunction (Configuration config, TestLabel label);
		public delegate void XmlCheckFunction (XPathNavigator nav, TestLabel label);

		public static void Run (string name, TestFunction func)
		{
			var label = new TestLabel (name);
			var filename = Path.GetTempFileName ();
			
			try {
				File.Delete (filename);
				
				var fileMap = new ExeConfigurationFileMap ();
				fileMap.ExeConfigFilename = filename;
				var config = ConfigurationManager.OpenMappedExeConfiguration (
					fileMap, ConfigurationUserLevel.None);
				
				func (config, label);
			} finally {
				if (File.Exists (filename))
					File.Delete (filename);
			}
		}

		public static void Run<TConfig> (string name, TestFunction func)
			where TConfig : ConfigProvider, new ()
		{
			Run<TConfig> (new TestLabel (name), func, null);
		}

		public static void Run<TConfig> (TestLabel label, TestFunction func)
			where TConfig : ConfigProvider, new ()
		{
			Run<TConfig> (label, func, null);
		}

		public static void Run<TConfig> (
			string name, TestFunction func, XmlCheckFunction check)
			where TConfig : ConfigProvider, new ()
		{
			Run<TConfig> (new TestLabel (name), func, check);
		}

		public static void Run<TConfig> (
			TestLabel label, TestFunction func, XmlCheckFunction check)
			where TConfig : ConfigProvider, new ()
		{
			var parent = Path.GetTempFileName ();
			var filename = Path.GetTempFileName ();

			try {
				File.Delete (parent);
				File.Delete (filename);

				var provider = new TConfig ();
				provider.Create (parent);

				Assert.That (File.Exists (filename), Is.False);

				ConfigurationUserLevel level;
				var fileMap = new ExeConfigurationFileMap ();
				if (provider.IsMachineConfig) {
					fileMap.ExeConfigFilename = filename;
					fileMap.MachineConfigFilename = parent;
					level = ConfigurationUserLevel.None;
				} else {
					fileMap.ExeConfigFilename = parent;
					fileMap.RoamingUserConfigFilename = filename;
					level = ConfigurationUserLevel.PerUserRoaming;
				}

				var config = ConfigurationManager.OpenMappedExeConfiguration (
					fileMap, level);

				Assert.That (File.Exists (filename), Is.False);

				try {
					label.EnterScope ("config");
					func (config, label);
				} catch (Exception ex) {
					Console.WriteLine ("ERROR: {0}", ex);
					return;
				} finally {
					label.LeaveScope ();
				}

				if (check == null)
					return;

				var xml = new XmlDocument ();
				xml.Load (filename);

				var nav = xml.CreateNavigator ().SelectSingleNode ("/configuration");
				try {
					label.EnterScope ("xml");
					check (nav, label);
				} catch (Exception ex) {
					Console.WriteLine ("ERROR CHECKING XML: {0}", ex);
					Console.WriteLine (xml.OuterXml);
					Console.WriteLine ();
				} finally {
					label.LeaveScope ();
				}
			} finally {
				if (File.Exists (parent))
					File.Delete (parent);
				if (File.Exists (filename))
					File.Delete (filename);
			}
		}

		#endregion

		#region Assertion Helpers

		static void AssertNotModified (MySection my, TestLabel label)
		{
			label.EnterScope ("modified");
			Assert.That (my, Is.Not.Null, label.Get ());
			Assert.That (my.IsModified, Is.False, label.Get ());
			Assert.That (my.List, Is.Not.Null, label.Get ());
			Assert.That (my.List.Collection.Count, Is.EqualTo (0), label.Get ());
			Assert.That (my.List.IsModified, Is.False, label.Get ());
			label.LeaveScope ();
		}

		static void AssertListElement (XPathNavigator nav, TestLabel label)
		{
			Assert.That (nav.HasChildren, Is.True, label.Get ());
			var iter = nav.SelectChildren (XPathNodeType.Element);
			
			Assert.That (iter.Count, Is.EqualTo (1), label.Get ());
			Assert.That (iter.MoveNext (), Is.True, label.Get ());
			
			var my = iter.Current;
			label.EnterScope ("my");
			Assert.That (my.Name, Is.EqualTo ("my"), label.Get ());
			Assert.That (my.HasAttributes, Is.False, label.Get ());
			
			label.EnterScope ("children");
			Assert.That (my.HasChildren, Is.True, label.Get ());
			var iter2 = my.SelectChildren (XPathNodeType.Element);
			Assert.That (iter2.Count, Is.EqualTo (1), label.Get ());
			Assert.That (iter2.MoveNext (), Is.True, label.Get ());
			
			var test = iter2.Current;
			label.EnterScope ("test");
			Assert.That (test.Name, Is.EqualTo ("test"), label.Get ());
			Assert.That (test.HasChildren, Is.False, label.Get ());
			Assert.That (test.HasAttributes, Is.True, label.Get ());
			
			var attr = test.GetAttribute ("Hello", string.Empty);
			Assert.That (attr, Is.EqualTo ("29"), label.Get ());
			label.LeaveScope ();
			label.LeaveScope ();
			label.LeaveScope ();
		}
		
		#endregion

		#region Tests

		[Test]
		public void DefaultValues ()
		{
			Run<DefaultMachineConfig> ("DefaultValues", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());

				config.Save (ConfigurationSaveMode.Minimal);
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				label.LeaveScope ();
			});
		}

		[Test]
		public void AddDefaultListElement ()
		{
			Run<DefaultMachineConfig> ("AddDefaultListElement", (config,label) => {
				var my = config.Sections ["my"] as MySection;
				
				AssertNotModified (my, label);
				
				label.EnterScope ("add");
				var element = my.List.Collection.AddElement ();
				Assert.That (my.IsModified, Is.True, label.Get ());
				Assert.That (my.List.IsModified, Is.True, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.True, label.Get ());
				Assert.That (element.IsModified, Is.False, label.Get ());
				label.LeaveScope ();
				
				config.Save (ConfigurationSaveMode.Minimal);
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
			});
		}
		
		[Test]
		public void AddDefaultListElement2 ()
		{
			Run<DefaultMachineConfig> ("AddDefaultListElement2", (config,label) => {
				var my = config.Sections ["my"] as MySection;
				
				AssertNotModified (my, label);
				
				label.EnterScope ("add");
				var element = my.List.Collection.AddElement ();
				Assert.That (my.IsModified, Is.True, label.Get ());
				Assert.That (my.List.IsModified, Is.True, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.True, label.Get ());
				Assert.That (element.IsModified, Is.False, label.Get ());
				label.LeaveScope ();
				
				config.Save (ConfigurationSaveMode.Modified);
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
			}, (nav,label) => {
				Assert.That (nav.HasChildren, Is.True, label.Get ());
				var iter = nav.SelectChildren (XPathNodeType.Element);
				
				Assert.That (iter.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter.MoveNext (), Is.True, label.Get ());
				
				var my = iter.Current;
				label.EnterScope ("my");
				Assert.That (my.Name, Is.EqualTo ("my"), label.Get ());
				Assert.That (my.HasAttributes, Is.False, label.Get ());
				Assert.That (my.HasChildren, Is.False, label.Get ());
				label.LeaveScope ();
			});
		}

		[Test]
		public void AddDefaultListElement3 ()
		{
			Run<DefaultMachineConfig> ("AddDefaultListElement3", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				label.EnterScope ("add");
				var element = my.List.Collection.AddElement ();
				Assert.That (my.IsModified, Is.True, label.Get ());
				Assert.That (my.List.IsModified, Is.True, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.True, label.Get ());
				Assert.That (element.IsModified, Is.False, label.Get ());
				label.LeaveScope ();
				
				config.Save (ConfigurationSaveMode.Full);
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
			}, (nav,label) => {
				Assert.That (nav.HasChildren, Is.True, label.Get ());
				var iter = nav.SelectChildren (XPathNodeType.Element);
				
				Assert.That (iter.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter.MoveNext (), Is.True, label.Get ());
				
				var my = iter.Current;
				label.EnterScope ("my");
				Assert.That (my.Name, Is.EqualTo ("my"), label.Get ());
				Assert.That (my.HasAttributes, Is.False, label.Get ());
				
				label.EnterScope ("children");
				Assert.That (my.HasChildren, Is.True, label.Get ());
				var iter2 = my.SelectChildren (XPathNodeType.Element);
				Assert.That (iter2.Count, Is.EqualTo (2), label.Get ());

				label.EnterScope ("list");
				var iter3 = my.Select ("list/*");
				Assert.That (iter3.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter3.MoveNext (), Is.True, label.Get ());
				var collection = iter3.Current;
				Assert.That (collection.Name, Is.EqualTo ("collection"), label.Get ());
				Assert.That (collection.HasChildren, Is.False, label.Get ());
				Assert.That (collection.HasAttributes, Is.True, label.Get ());
				var hello = collection.GetAttribute ("Hello", string.Empty);
				Assert.That (hello, Is.EqualTo ("8"), label.Get ());
				var world = collection.GetAttribute ("World", string.Empty);
				Assert.That (world, Is.EqualTo ("0"), label.Get ());
				label.LeaveScope ();

				label.EnterScope ("test");
				var iter4 = my.Select ("test");
				Assert.That (iter4.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter4.MoveNext (), Is.True, label.Get ());
				var test = iter4.Current;
				Assert.That (test.Name, Is.EqualTo ("test"), label.Get ());
				Assert.That (test.HasChildren, Is.False, label.Get ());
				Assert.That (test.HasAttributes, Is.True, label.Get ());
				
				var hello2 = test.GetAttribute ("Hello", string.Empty);
				Assert.That (hello2, Is.EqualTo ("8"), label.Get ());
				var world2 = test.GetAttribute ("World", string.Empty);
				Assert.That (world2, Is.EqualTo ("0"), label.Get ());
				label.LeaveScope ();
				label.LeaveScope ();
				label.LeaveScope ();
			});
		}

		[Test]
		public void AddListElement ()
		{
			Run<DefaultMachineConfig> ("AddListElement", (config,label) => {
				var my = config.Sections ["my"] as MySection;
				
				AssertNotModified (my, label);
				
				my.Test.Hello = 29;
				
				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				
				config.Save (ConfigurationSaveMode.Minimal);
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
				label.LeaveScope ();
			}, (nav,label) => {
				AssertListElement (nav, label);
			});
		}
		
		[Test]
		public void NotModifiedAfterSave ()
		{
			Run<DefaultMachineConfig> ("NotModifiedAfterSave", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				label.EnterScope ("add");
				var element = my.List.Collection.AddElement ();
				Assert.That (my.IsModified, Is.True, label.Get ());
				Assert.That (my.List.IsModified, Is.True, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.True, label.Get ());
				Assert.That (element.IsModified, Is.False, label.Get ());
				label.LeaveScope ();

				label.EnterScope ("1st-save");
				config.Save (ConfigurationSaveMode.Minimal);
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				config.Save (ConfigurationSaveMode.Modified);
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				label.LeaveScope ();

				label.EnterScope ("modify");
				element.Hello = 12;
				Assert.That (my.IsModified, Is.True, label.Get ());
				Assert.That (my.List.IsModified, Is.True, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.True, label.Get ());
				Assert.That (element.IsModified, Is.True, label.Get ());
				label.LeaveScope ();

				label.EnterScope ("2nd-save");
				config.Save (ConfigurationSaveMode.Modified);
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());

				Assert.That (my.IsModified, Is.False, label.Get ());
				Assert.That (my.List.IsModified, Is.False, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.False, label.Get ());
				Assert.That (element.IsModified, Is.False, label.Get ());
				label.LeaveScope (); // 2nd-save
			});
		}

		[Test]
		public void AddSection ()
		{
			Run ("AddSection", (config,label) => {
				Assert.That (config.Sections ["my"], Is.Null, label.Get ());

				var my = new MySection ();
				config.Sections.Add ("my2", my);
				config.Save (ConfigurationSaveMode.Full);

				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
			});
		}

		[Test]
		public void AddElement ()
		{
			Run<DefaultMachineConfig> ("AddElement", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				var element = my.List.DefaultCollection.AddElement ();
				element.Hello = 12;

				config.Save (ConfigurationSaveMode.Modified);

				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.True, "#c2");
				label.LeaveScope ();
			}, (nav,label) => {
				Assert.That (nav.HasChildren, Is.True, label.Get ());
				var iter = nav.SelectChildren (XPathNodeType.Element);
				
				Assert.That (iter.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter.MoveNext (), Is.True, label.Get ());
				
				var my = iter.Current;
				label.EnterScope ("my");
				Assert.That (my.Name, Is.EqualTo ("my"), label.Get ());
				Assert.That (my.HasAttributes, Is.False, label.Get ());
				Assert.That (my.HasChildren, Is.True, label.Get ());

				label.EnterScope ("children");
				var iter2 = my.SelectChildren (XPathNodeType.Element);
				Assert.That (iter2.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter2.MoveNext (), Is.True, label.Get ());

				var list = iter2.Current;
				label.EnterScope ("list");
				Assert.That (list.Name, Is.EqualTo ("list"), label.Get ());
				Assert.That (list.HasChildren, Is.False, label.Get ());
				Assert.That (list.HasAttributes, Is.True, label.Get ());

				var attr = list.GetAttribute ("Hello", string.Empty);
				Assert.That (attr, Is.EqualTo ("12"), label.Get ());
				label.LeaveScope ();
				label.LeaveScope ();
				label.LeaveScope ();
			});
		}

		// [Test]
		public void ModifyListElement ()
		{
			Run<TestConfig> ("ModifyListElement", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);
				
				my.Test.Hello = 29;
				
				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				
				config.Save (ConfigurationSaveMode.Minimal);
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				label.LeaveScope ();
			});
		}

		// [Test]
		public void ModifyListElement2 ()
		{
			Run<TestConfig> ("ModifyListElement2", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);
				
				my.Test.Hello = 29;
				
				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				
				config.Save (ConfigurationSaveMode.Modified);
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
				label.LeaveScope ();
			}, (nav,label) => {
				AssertListElement (nav, label);
			});
		}

		#endregion

		#region Configuration Classes

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

			public void RemoveElement (T element)
			{
				BaseRemove (GetElementKey (element));
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

			[ConfigurationProperty ("test", Options = ConfigurationPropertyOptions.None)]
			public MyElement Test {
				get { return (MyElement) this ["test"]; }
			}

			new public bool IsModified {
				get { return base.IsModified (); }
			}
		}

		#endregion
	}
}

