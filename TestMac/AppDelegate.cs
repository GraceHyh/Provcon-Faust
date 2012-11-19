//
// Authors:
//      Martin Baulig (martin.baulig@xamarin.com)
//
// Copyright 2012 Xamarin Inc. (http://www.xamarin.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Drawing;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.ObjCRuntime;

namespace TestMac {

	public partial class AppDelegate : NSApplicationDelegate {
		MainWindowController mainWindowController;
		
		public AppDelegate ()
		{
		}

		public override void FinishedLaunching (NSObject notification)
		{
			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);

			var path = Path.Combine (NSBundle.MainBundle.BundlePath, "Contents", "Resources");
			var filePath = Path.Combine (path, "Neptune.jpg");

			var image = new NSImage (filePath);
			Console.WriteLine (image);

			SaveImage (image, "output.jpg", 0.0f);
			SaveImage (image, "output.jpg", 0.25f);
			SaveImage (image, "output.jpg", 0.5f);
			SaveImage (image, "output.jpg", 1.0f);
		}

		public void SaveImage (NSImage image, string location, float quality) 
		{
			var brep = (NSBitmapImageRep)image.Representations ()[0];

			var dict = NSDictionary.FromObjectAndKey (
				NSNumber.FromFloat (quality), NSBitmapImageRep.CompressionFactor);

			var data = brep.RepresentationUsingTypeProperties (NSBitmapImageFileType.Jpeg, dict);
			Console.WriteLine (data.Length);
		} 
	}
}
