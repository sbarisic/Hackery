using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;

namespace Hackery {
	[StructLayout(LayoutKind.Sequential)]
	public struct ProcessUtils {
		internal IntPtr Reserved1;
		internal IntPtr PebBaseAddress;
		internal IntPtr Reserved2_0;
		internal IntPtr Reserved2_1;
		internal IntPtr UniqueProcessId;
		internal IntPtr InheritedFromUniqueProcessId;

		[DllImport("ntdll.dll")]
		private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ProcessUtils processInformation, int processInformationLength, out int returnLength);

		public static Process GetParentProcess() {
			return GetParentProcess(Process.GetCurrentProcess().Handle);
		}

		public static Process GetParentProcess(int id) {
			Process process = Process.GetProcessById(id);
			return GetParentProcess(process.Handle);
		}

		public static Process GetParentProcess(IntPtr handle) {
			ProcessUtils pbi = new ProcessUtils();
			int returnLength;
			int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
			if (status != 0)
				throw new Win32Exception(status);

			try {
				return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
			} catch (ArgumentException) {
				// not found
				return null;
			}
		}
	}
}