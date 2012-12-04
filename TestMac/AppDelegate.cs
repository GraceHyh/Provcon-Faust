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
using System.Net;
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

		void TestNetwork ()
		{
			var url = NSUrl.FromString ("http://www.heise.de/");
			var request = NSUrlRequest.FromUrl (url);
			var dlg = new ConnectionDelegate ();

			NSError error;
			NSUrlResponse response;

			NSUrlConnection.SendSynchronousRequest (request, out response, out error);

			Console.WriteLine ("GOT RESPONSE: {0} {1}", response, error);
		}

		void TestWebRequest ()
		{
			IntPtr native = _SystemNet.CFNetwork.CFNetworkCopySystemProxySettings ();
			var dict = new NSDictionary (native, true);

			foreach (var key in dict.Keys) {
				var value = dict [key];
				Console.WriteLine ("DICT: {0} {1:x} -> {2}", key, key.Handle.ToInt32 (), value);
			}

			var uri = new Uri ("http://www.heise.de/");
			var proxy = _SystemNet.CFNetwork.GetDefaultProxy ();
			var targetProxy = proxy.GetProxy (uri);
			Console.WriteLine ("PROXY: {0}", targetProxy != null);
		}

		public override void FinishedLaunching (NSObject notification)
		{
			mainWindowController = new MainWindowController ();
			mainWindowController.Window.MakeKeyAndOrderFront (this);

			try {
				// TestNetwork ();
				TestWebRequest ();
			} catch (Exception ex) {
				Console.WriteLine ("NETWORK EX: {0}", ex);
			}
		}

		class ConnectionDelegate : NSUrlConnectionDelegate {
			public override void ReceivedResponse (NSUrlConnection connection, NSUrlResponse response)
			{
				throw new System.NotImplementedException ();
			}

			public override void ReceivedAuthenticationChallenge (NSUrlConnection connection, NSUrlAuthenticationChallenge challenge)
			{
				throw new System.NotImplementedException ();
			}

			public override void ReceivedData (NSUrlConnection connection, NSData data)
			{
				throw new System.NotImplementedException ();
			}
		}

	}
}

