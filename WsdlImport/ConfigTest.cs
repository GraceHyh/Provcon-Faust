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
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Configuration;
using System.ComponentModel;
using System.ServiceModel;
using System.ServiceModel.Configuration;

using NUnit.Framework;

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

		public static void Run (string filename)
		{
#if FIXME
			if (ReadConfig (filename))
				return;
#endif

			if (File.Exists (filename))
				File.Delete (filename);
			var fileMap = new ExeConfigurationFileMap ();
			fileMap.ExeConfigFilename = filename;
			var config = ConfigurationManager.OpenMappedExeConfiguration (
				fileMap, ConfigurationUserLevel.None);

			MySection my;
			my = (MySection)config.Sections ["my"];
			if (my == null) {
				my = new MySection ();
				config.Sections.Add ("my", my);
			}

			// my.Hello = 11;
			// my.TextEncoding = Encoding.UTF8;

			config.Save (ConfigurationSaveMode.Minimal);
			
			Console.WriteLine ("TEST: {0}", config.FilePath);
			
			Utils.Dump (filename);
		}

		public class MySection : ConfigurationSection {

			ConfigurationPropertyCollection _properties;

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

			[TypeConverter (typeof (EncodingConverter))]
			[ConfigurationProperty ("textEncoding",
			                        DefaultValue = "utf-8",
			                        Options = ConfigurationPropertyOptions.None)]
			public Encoding TextEncoding {
				get { return (Encoding) this ["textEncoding"]; }
				set { this ["textEncoding"] = value; }
			}

			protected override ConfigurationPropertyCollection Properties {
				get {
					if (_properties == null) {
						_properties = base.Properties;
						_properties.Add (new ConfigurationProperty ("textEncoding", typeof (Encoding), "utf-8", EncodingConverter.Instance, null, ConfigurationPropertyOptions.None));
					}
					return _properties;
				}
			}
		}

		sealed class EncodingConverter : TypeConverter
		{
			static EncodingConverter _instance = new EncodingConverter ();
			
			public static EncodingConverter Instance {
				get { return _instance; }
			}
			
			public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
				return sourceType == typeof (string);
			}
			
			public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value) {
				string encString = (string) value;
				Encoding encoding;
				
				switch (encString.ToLower (CultureInfo.InvariantCulture)) {
				case "utf-16le":
				case "utf-16":
				case "ucs-2":
				case "unicode":
				case "iso-10646-ucs-2":
					encoding = new UnicodeEncoding (false, true);
					break;
				case "utf-16be":
				case "unicodefffe":
					encoding = new UnicodeEncoding (true, true);
					break;
				case "utf-8":
				case "unicode-1-1-utf-8":
				case "unicode-2-0-utf-8":
				case "x-unicode-1-1-utf-8":
				case "x-unicode-2-0-utf-8":
					encoding = Encoding.UTF8;
					break;
				default:
					encoding = Encoding.GetEncoding (encString);
					break;
				}
				
				return encoding;
			}
			
			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
				Encoding encoding = (Encoding) value;
				return encoding.WebName;
			}
		}
	}
}

