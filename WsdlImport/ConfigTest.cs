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

using MonoTests.System.ServiceModel.MetadataTests;

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

		public abstract class MachineConfigProvider {
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

			protected virtual void WriteXml (XmlWriter writer)
			{
				writer.WriteStartElement ("configSections");
				WriteSections (writer);
				writer.WriteEndElement ();
				WriteValues (writer);
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

			protected override void WriteValues (XmlWriter writer)
			{
				writer.WriteStartElement ("my");
				writer.WriteEndElement ();
			}
		}

		class EmptySectionMachineConfig : DefaultMachineConfig {
			protected override void WriteValues (XmlWriter writer)
			{
				writer.WriteStartElement ("my");
				writer.WriteEndElement ();
			}
		}

		public static void CreateMachine (string filename)
		{
			if (File.Exists (filename))
				File.Delete (filename);

			var settings = new XmlWriterSettings ();
			settings.Indent = true;

			using (var writer = XmlTextWriter.Create (filename, settings)) {
				writer.WriteStartElement ("configuration");
				writer.WriteStartElement ("my");
				writer.WriteEndElement ();
				writer.WriteEndElement ();
			}

#if REALLY_FIXME
			var protectedData = (ProtectedConfigurationSection)config.Sections ["configProtectedData"];
			if (protectedData == null) {
				protectedData = new ProtectedConfigurationSection ();
				config.Sections.Add ("configProtectedData", protectedData);
			}
			var settings = new ProviderSettings ("RsaProtectedConfigurationProvider", typeof (RsaProtectedConfigurationProvider).AssemblyQualifiedName);
			protectedData.Providers.Add (settings);
			protectedData.DefaultProvider = "RsaProtectedConfigurationProvider";
#endif

#if FIXME
			Console.WriteLine ();
			Console.WriteLine ("MACHINE CONFIG: {0}", filename);
			Console.WriteLine ("==================================");
			Utils.Dump (filename);
			Console.WriteLine ("==================================");
			Console.WriteLine ();
#endif
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

		public static void Run<TMachine> (string name, TestFunction func)
			where TMachine : MachineConfigProvider, new ()
		{
			Run<TMachine> (new TestLabel (name), func, null);
		}

		public static void Run<TMachine> (TestLabel label, TestFunction func)
			where TMachine : MachineConfigProvider, new ()
		{
			Run<TMachine> (label, func, null);
		}

		public static void Run<TMachine> (
			string name, TestFunction func, XmlCheckFunction check)
			where TMachine : MachineConfigProvider, new ()
		{
			Run<TMachine> (new TestLabel (name), func, check);
		}

		public static void Run<TMachine> (
			TestLabel label, TestFunction func, XmlCheckFunction check)
			where TMachine : MachineConfigProvider, new ()
		{
			var machine = Path.GetTempFileName ();
			var filename = Path.GetTempFileName ();

			try {
				File.Delete (machine);
				File.Delete (filename);

				var machineProvider = new TMachine ();
				machineProvider.Create (machine);

				Assert.That (File.Exists (filename), Is.False);
					
				var fileMap = new ExeConfigurationFileMap ();
				fileMap.ExeConfigFilename = filename;
				fileMap.MachineConfigFilename = machine;
				var config = ConfigurationManager.OpenMappedExeConfiguration (
					fileMap, ConfigurationUserLevel.None);

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
				} finally {
					label.LeaveScope ();
				}
			} finally {
				if (File.Exists (machine))
					File.Delete (machine);
				if (File.Exists (filename))
					File.Delete (filename);
			}
		}

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

		public static void TestNotModified ()
		{
			Run<DefaultMachineConfig> ("NotModified", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());

				config.Save (ConfigurationSaveMode.Minimal);
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				label.LeaveScope ();
			});
		}

		public static void TestNotModifiedAfterSave ()
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

				config.Save ();

				label.EnterScope ("1st-save");
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());

				Assert.That (my.IsModified, Is.False, label.Get ());
				Assert.That (my.List.IsModified, Is.False, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.False, label.Get ());

				element.Hello = 1;
				label.EnterScope ("modified");
				Assert.That (element.IsModified, Is.True, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.True, label.Get ());
				Assert.That (my.List.IsModified, Is.True, label.Get ());
				Assert.That (my.IsModified, Is.True, label.Get ());
				label.LeaveScope ();
				label.LeaveScope ();

				config.Save ();
				label.EnterScope ("2nd-save");
				Assert.That (element.IsModified, Is.False, label.Get ());
				Assert.That (my.List.Collection.IsModified, Is.False, label.Get ());
				Assert.That (my.List.IsModified, Is.False, label.Get ());
				Assert.That (my.IsModified, Is.False, label.Get ());
				label.LeaveScope ();
			});
		}

		public static void TestAddSection ()
		{
			Run ("AddSection", (config,label) => {
				Assert.That (config.Sections ["my"], Is.Null, label.Get ());

				var my = new MySection ();
				config.Sections.Add ("my2", my);
				config.Save (ConfigurationSaveMode.Full);

				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
			});
		}

		public static void TestAddElement_Modified ()
		{
			Run<EmptySectionMachineConfig> ("AddElement_Modified", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				label.EnterScope ("add-element");
				my.List.DefaultCollection.AddElement ();
				Assert.That (my.IsModified, Is.True, label.Get ());
				label.LeaveScope ();

				config.Save (ConfigurationSaveMode.Modified);

				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
				label.LeaveScope ();
			}, (nav,label) => {
				Console.WriteLine ("ADD ELEMENT MODIFIED: {0}", nav.OuterXml);

				Assert.That (nav.HasChildren, Is.True, label.Get ());
				var iter = nav.SelectChildren (XPathNodeType.Element);

				Assert.That (iter.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter.MoveNext (), Is.True, label.Get ());

				var my = iter.Current;
				label.EnterScope ("my");
				Assert.That (my.Name, Is.EqualTo ("my"), label.Get ());
				Assert.That (my.HasAttributes, Is.False, label.Get ());

				// FIXME: Fails
				label.EnterScope ("children");
				Assert.That (my.HasChildren, Is.False, label.Get ());
				label.LeaveScope ();
				label.LeaveScope ();
			});
		}

		public static void TestAddElement_Modified2 ()
		{
			Run<EmptySectionMachineConfig> ("AddElement_Modified2", (config, label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				my.List.DefaultCollection.AddElement ();

				var element2 = my.List.DefaultCollection.AddElement ();
				element2.Hello = 1;
				my.List.DefaultCollection.RemoveElement (element2);

				config.Save (ConfigurationSaveMode.Modified);

				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.True, label.Get ());
				label.LeaveScope ();
			}, (nav,label) => {
				Console.WriteLine ("ADD ELEMENT MODIFIED: {0}", nav.OuterXml);
				
				Assert.That (nav.HasChildren, Is.True, label.Get ());
				var iter = nav.SelectChildren (XPathNodeType.Element);
				
				Assert.That (iter.Count, Is.EqualTo (1), label.Get ());
				Assert.That (iter.MoveNext (), Is.True, label.Get ());
				
				var my = iter.Current;
				label.EnterScope ("my");
				Assert.That (my.Name, Is.EqualTo ("my"), label.Get ());
				Assert.That (my.HasAttributes, Is.False, label.Get ());
				
				// FIXME: Fails
				label.EnterScope ("children");
				Assert.That (my.HasChildren, Is.False, label.Get ());
				label.LeaveScope ();
				label.LeaveScope ();
			});
		}

		public static void TestAddElement_Minimal ()
		{
			Run<EmptySectionMachineConfig> ("AddElement_Minimal", (config,label) => {
				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				label.EnterScope ("add-element");
				my.List.DefaultCollection.AddElement ();
				Assert.That (my.IsModified, Is.True, label.Get ());
				label.LeaveScope ();

				config.Save (ConfigurationSaveMode.Minimal);

				// FIXME: Fails
				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.False, label.Get ());
				label.LeaveScope ();
			});
		}

		public static void TestAddElement2 ()
		{
			Run<EmptySectionMachineConfig> ("AddElement2", (config,label) => {
				Console.WriteLine ("ADD ELEMENT 2");

				var my = config.Sections ["my"] as MySection;

				AssertNotModified (my, label);

				var element = my.List.DefaultCollection.AddElement ();
				element.Hello = 1;

				config.Save (ConfigurationSaveMode.Modified);

				label.EnterScope ("file");
				Assert.That (File.Exists (config.FilePath), Is.True, "#c2");
				label.LeaveScope ();
			}, (nav,label) => {
				Console.WriteLine ("ADD ELEMENT 2: {0}", nav.OuterXml);

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
				Assert.That (attr, Is.EqualTo ("1"), label.Get ());
				label.LeaveScope ();
				label.LeaveScope ();
				label.LeaveScope ();
			});
		}

		public static void Run ()
		{
#if WORKING
			TestAddSection ();

			TestModified ();
			TestNotModified ();
			TestNotModifiedAfterSave ();
#endif

			TestAddElement_Minimal ();
			TestAddElement_Modified ();
			TestAddElement_Modified2 ();

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

			new public bool IsModified {
				get { return base.IsModified (); }
			}
		}
	}
}

