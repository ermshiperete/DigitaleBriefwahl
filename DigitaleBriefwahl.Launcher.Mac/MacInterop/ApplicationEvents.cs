//
// ApplicationEvents.cs
//
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;

namespace MonoDevelop.MacInterop
{
	public static class ApplicationEvents
	{
		static readonly object lockObj = new object ();

		#region OpenDocuments

		static EventHandler<ApplicationDocumentEventArgs> openDocuments;
		static IntPtr openDocumentsHandlerRef = IntPtr.Zero;

		public static event EventHandler<ApplicationDocumentEventArgs> OpenDocuments {
			add {
				lock (lockObj) {
					openDocuments += value;
					if (openDocumentsHandlerRef == IntPtr.Zero)
						openDocumentsHandlerRef = Carbon.InstallApplicationEventHandler (HandleOpenDocuments, CarbonEventApple.OpenDocuments);
				}
			}
			remove {
				lock (lockObj) {
					openDocuments -= value;
					if (openDocuments == null && openDocumentsHandlerRef != IntPtr.Zero) {
						Carbon.RemoveEventHandler (openDocumentsHandlerRef);
						openDocumentsHandlerRef = IntPtr.Zero;
					}
				}
			}
		}

		static CarbonEventHandlerStatus HandleOpenDocuments (IntPtr callRef, IntPtr eventRef, IntPtr user_data)
		{
			try {
				var docs = Carbon.GetFileListFromEventRef (eventRef);
				var args = new ApplicationDocumentEventArgs (docs);
				openDocuments (null, args);
				return args.HandledStatus;
			} catch (Exception ex) {
				Console.WriteLine (ex);
				return CarbonEventHandlerStatus.NotHandled;
			}
		}

		#endregion
	}

	public class ApplicationEventArgs : EventArgs
	{
		public bool Handled { get; set; }

		internal CarbonEventHandlerStatus HandledStatus {
			get {
				return Handled? CarbonEventHandlerStatus.Handled : CarbonEventHandlerStatus.NotHandled;
			}
		}
	}

	public class ApplicationDocumentEventArgs : ApplicationEventArgs
	{
		public ApplicationDocumentEventArgs (IDictionary<string,int> documents)
		{
			Documents = documents;
		}

		public IDictionary<string,int> Documents { get; private set; }
	}
}

