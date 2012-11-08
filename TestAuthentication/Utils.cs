//
// Utils.cs
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

namespace ProvconFaust.TestAuthentication {

	public static class Utils {

		public static void HexDump (byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++) {
				if ((i % 8) == 0) {
					Console.WriteLine ();
					Console.Write ("{0:x4} ", i);
				}
				Console.Write ("{0:x2} ", buffer [i]);
			}
			Console.WriteLine ();
			Console.WriteLine ();
		}
		
		public static void HexDump (string name, byte[] buffer)
		{
			Console.Write ("{0}: ", name);
			for (int i = 0; i < buffer.Length; i++) {
				Console.Write ("{0:x2} ", buffer [i]);
			}
			Console.WriteLine ();
		}

		public static void Compare (byte[] a, byte[] b)
		{
			var length = Math.Min (a.Length, b.Length);
			for (int i = 0; i < length; i++) {
				if (a[i] == b[i])
					continue;
				Console.WriteLine ("{0:x4}: {1:x2} - {2:x2}", i, a[i], b[i]);
			}
		}
	}
}

