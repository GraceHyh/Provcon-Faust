using System;
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace DelegateTest
{
	public class MyDelegate : NSTextViewDelegate
	{
		public override void DidChangeSelection (NSNotification notification)
		{
			Console.WriteLine ("DID CHANGE SELECTION!");
		}

		public override void TextDidChange (NSNotification notification)
		{
			Console.WriteLine ("TEXT DID CHANGE!");
		}
	}
}

