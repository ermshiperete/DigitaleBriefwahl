//
// Carbon.cs
//
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
//       Geoff Norton  <gnorton@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MonoDevelop.MacInterop
{
	internal delegate CarbonEventHandlerStatus EventDelegate (IntPtr callRef, IntPtr eventRef, IntPtr userData);
	internal delegate CarbonEventHandlerStatus AEHandlerDelegate (IntPtr inEvnt, IntPtr outEvt, uint refConst);

	internal static class Carbon
	{
		public const string CarbonLib = "/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon";

		[DllImport (CarbonLib)]
		static extern int Gestalt (int selector, out int result);

		public static int Gestalt (string selector)
		{
			int cc = ConvertCharCode (selector);
			int result;
			int ret = Gestalt (cc, out result);
			CheckReturn (ret);
			return result;
		}

		[DllImport (CarbonLib)]
		public static extern IntPtr GetApplicationEventTarget ();

		#region Event handler installation

		[DllImport (CarbonLib)]
		static extern EventStatus InstallEventHandler (IntPtr target, EventDelegate handler, uint count,
		                                               CarbonEventTypeSpec [] types, IntPtr user_data, out IntPtr handlerRef);

		[DllImport (CarbonLib)]
		public static extern EventStatus RemoveEventHandler (IntPtr handlerRef);

		public static IntPtr InstallEventHandler (IntPtr target, EventDelegate handler, CarbonEventTypeSpec [] types)
		{
			IntPtr handlerRef;
			CheckReturn (InstallEventHandler (target, handler, (uint)types.Length, types, IntPtr.Zero, out handlerRef));
			return handlerRef;
		}

		public static IntPtr InstallApplicationEventHandler (EventDelegate handler, CarbonEventTypeSpec type)
		{
			return InstallEventHandler (GetApplicationEventTarget (), handler, new[] { type });
		}

		#endregion

		#region Event parameter extraction

		[DllImport (CarbonLib)]
		static extern EventStatus GetEventParameter (IntPtr eventRef, CarbonEventParameterName name, CarbonEventParameterType desiredType,
		                                             uint zero, uint size, uint zero2, IntPtr dataBuffer);

		public static T GetEventParameter<T> (IntPtr eventRef, CarbonEventParameterName name, CarbonEventParameterType desiredType) where T : struct
		{
			int len = Marshal.SizeOf (typeof (T));
			IntPtr bufferPtr = Marshal.AllocHGlobal (len);
			CheckReturn (GetEventParameter (eventRef, name, desiredType, 0, (uint)len, 0, bufferPtr));
			T val = (T)Marshal.PtrToStructure (bufferPtr, typeof (T));
			Marshal.FreeHGlobal (bufferPtr);
			return val;
		}

		#endregion

		#region Error checking

		public static void CheckReturn (EventStatus status)
		{
			int intStatus = (int) status;
			if (intStatus < 0)
				throw new EventStatusException (status);
		}

		public static void CheckReturn (int osErr)
		{
			if (osErr != 0) {
				string s = GetMacOSStatusCommentString (osErr);
				throw new SystemException ("Unexpected OS error code " + osErr + ": " + s);
			}
		}

		[DllImport (CarbonLib)]
		static extern string GetMacOSStatusCommentString (int osErr);

		#endregion

		#region Char code conversion

		internal static int ConvertCharCode (string fourcc)
		{
			Debug.Assert (fourcc != null);
			Debug.Assert (fourcc.Length == 4);
			return (fourcc[3]) | (fourcc[2] << 8) | (fourcc[1] << 16) | (fourcc[0] << 24);
		}

		#endregion

		public static Dictionary<string,int> GetFileListFromEventRef (IntPtr eventRef)
		{
			AEDesc list = GetEventParameter<AEDesc> (eventRef, CarbonEventParameterName.DirectObject, CarbonEventParameterType.AEList);
			try {
				int line = 0;
				try {
					SelectionRange range = GetEventParameter<SelectionRange> (eventRef, CarbonEventParameterName.AEPosition, CarbonEventParameterType.Char);
					line = range.lineNum+1;
				} catch {
				}

				var arr = AppleEvent.GetListFromAEDesc<string,FSRef> (ref list, CoreFoundation.FSRefToString,
					(int)CarbonEventParameterType.FSRef);
				var files = new Dictionary<string,int> ();
				foreach (var s in arr) {
					if (!string.IsNullOrEmpty (s))
						files[s] = line;
				}
				return files;
			} finally {
				CheckReturn ((int)AppleEvent.AEDisposeDesc (ref list));
			}
		}

	}

	[StructLayout(LayoutKind.Sequential, Pack = 2, Size = 80)]
	struct FSRef
	{
		//this is an 80-char opaque byte array
		#pragma warning disable 0169
		private byte hidden;
		#pragma warning restore 0169
	}

	[StructLayout(LayoutKind.Sequential)]
	struct SelectionRange
	{
		public short unused1; // 0 (not used)
		public short lineNum; // line to select (<0 to specify range)
		public int startRange; // start of selection range (if line < 0)
		public int endRange; // end of selection range (if line < 0)
		public int unused2; // 0 (not used)
		public int theDate; // modification date/time
	}

	internal enum CarbonEventHandlerStatus //this is an OSStatus
	{
		Handled = 0,
		NotHandled = -9874,
		UserCancelled = -128
	}

	internal enum CarbonEventParameterName : uint
	{
		DirectObject = 757935405, // '----'
		AEPosition = 1802530675 // 'kpos'
	}

	internal enum CarbonEventParameterType : uint
	{
		HICommand = 1751346532, // 'hcmd'
		MenuRef = 1835363957, // 'menu'
		WindowRef = 2003398244, // 'wind'
		Char = 1413830740, // 'TEXT'
		UInt32 = 1835100014, // 'magn'
		UTF8Text = 1970562616, // 'utf8'
		UnicodeText = 1970567284, // 'utxt'
		AEList = 1818850164, // 'list'
		WildCard = 707406378, // '****'
		FSRef = 1718841958 // 'fsrf'
	}

	internal enum CarbonEventClass : uint
	{
		Mouse = 1836021107, // 'mous'
		Keyboard = 1801812322, // 'keyb'
		TextInput = 1952807028, // 'text'
		Application = 1634758764, // 'appl'
		RemoteAppleEvent = 1701867619,  //'eppc' //remote apple event?
		Menu = 1835363957, // 'menu'
		Window = 2003398244, // 'wind'
		Control = 1668183148, // 'cntl'
		Command = 1668113523, // 'cmds'
		Tablet = 1952607348, // 'tblt'
		Volume = 1987013664, // 'vol '
		Appearance = 1634758765, // 'appm'
		Service = 1936028278, // 'serv'
		Toolbar = 1952604530, // 'tbar'
		ToolbarItem = 1952606580, // 'tbit'
		Accessibility = 1633903461, // 'acce'
		HIObject = 1751740258, // 'hiob'
		AppleEvent = 1634039412, // 'aevt'
		Internet = 1196773964 // 'GURL'
	}

	internal enum CarbonEventCommand : uint
	{
		Process = 1,
		UpdateStatus = 2
	}

	internal enum CarbonEventMenu : uint
	{
		BeginTracking = 1,
		EndTracking = 2,
		ChangeTrackingMode = 3,
		Opening = 4,
		Closed = 5,
		TargetItem = 6,
		MatchKey = 7
	}

	internal enum CarbonEventApple
	{
		OpenApplication = 1868656752, // 'oapp'
		ReopenApplication = 1918988400, //'rapp'
		OpenDocuments = 1868853091, // 'odoc'
		PrintDocuments = 188563030, // 'pdoc'
		OpenContents = 1868787566, // 'ocon'
		QuitApplication =  1903520116, // 'quit'
		ShowPreferences = 1886545254, // 'pref'
		ApplicationDied = 1868720500, // 'obit'
		GetUrl = 1196773964 // 'GURL'
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	struct CarbonEventTypeSpec
	{
		public CarbonEventClass EventClass;
		public uint EventKind;

		public CarbonEventTypeSpec (CarbonEventClass eventClass, UInt32 eventKind)
		{
			EventClass = eventClass;
			EventKind = eventKind;
		}

		public CarbonEventTypeSpec (CarbonEventMenu kind) : this (CarbonEventClass.Menu, (uint) kind)
		{
		}

		public CarbonEventTypeSpec (CarbonEventCommand kind) : this (CarbonEventClass.Command, (uint) kind)
		{
		}


		public CarbonEventTypeSpec (CarbonEventApple kind) : this (CarbonEventClass.AppleEvent, (uint) kind)
		{
		}

		public static implicit operator CarbonEventTypeSpec (CarbonEventMenu kind)
		{
			return new CarbonEventTypeSpec (kind);
		}

		public static implicit operator CarbonEventTypeSpec (CarbonEventCommand kind)
		{
			return new CarbonEventTypeSpec (kind);
		}

		public static implicit operator CarbonEventTypeSpec (CarbonEventApple kind)
		{
			return new CarbonEventTypeSpec (kind);
		}
	}

	class EventStatusException : SystemException
	{
		public EventStatusException (EventStatus status)
		{
			StatusCode = status;
		}

		public EventStatus StatusCode {
			get;
		}
	}

	enum EventStatus // this is an OSStatus
	{
		Ok = 0,

		//event manager
		EventAlreadyPostedErr = -9860,
		EventTargetBusyErr = -9861,
		EventClassInvalidErr = -9862,
		EventClassIncorrectErr = -9864,
		EventHandlerAlreadyInstalledErr = -9866,
		EventInternalErr = -9868,
		EventKindIncorrectErr = -9869,
		EventParameterNotFoundErr = -9870,
		EventNotHandledErr = -9874,
		EventLoopTimedOutErr = -9875,
		EventLoopQuitErr = -9876,
		EventNotInQueueErr = -9877,
		EventHotKeyExistsErr = -9878,
		EventHotKeyInvalidErr = -9879
	}

	struct OSType {
		int value;

		public int Value => value;

		public OSType (int value)
		{
			this.value = value;
		}

		public OSType (string fourcc)
		{
			value = Carbon.ConvertCharCode (fourcc);
		}

		public static explicit operator OSType (string fourcc)
		{
			return new OSType (fourcc);
		}

		public static implicit operator int (OSType o)
		{
			return o.value;
		}

		public static implicit operator OSType (int i)
		{
			return new OSType (i);
		}
	}
}
