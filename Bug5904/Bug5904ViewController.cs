//
// Bug5904ViewController.cs
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
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Drawing;
using System.Xml.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Bug5904 {

	public partial class Bug5904ViewController : UIViewController {
		static bool UserInterfaceIdiomIsPhone {
			get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
		}

		public Bug5904ViewController ()
			: base (UserInterfaceIdiomIsPhone ? "Bug5904ViewController_iPhone" : "Bug5904ViewController_iPad", null)
		{
		}

		#region Test Code

		// public const string FeedURI = "https://github.com/baulig.atom";
		// public const string FeedURI = "http://www.go-mono.com/monologue/index.rss";
		public const string FeedURI = "http://feeds.feedburner.com/baulig?format=xml";

		void Test ()
		{
			var doc = XDocument.Load (FeedURI);
			Console.WriteLine ("LOADED: {0}", doc);

			var service = new TestService ();
			var hello = service.HelloWorld ();
			Console.WriteLine ("HELLO WORLD: {0}", hello);

			for (int i = 0; i < 500; i++) {
				try {
					var fault = service.TestFault ();
					Console.WriteLine ("OOPS: {0}", fault);
					break;
				} catch (SoapException ex) {
					Console.WriteLine ("GOT SOAP FAULT: {0}", ex.Code);
				}
			}
		}

		#endregion
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.

			Test ();
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			if (UserInterfaceIdiomIsPhone) {
				return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
			} else {
				return true;
			}
		}
	}
}

