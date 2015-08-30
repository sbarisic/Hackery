using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using RGiesecke.DllExport;

namespace Inkjet {
	public class Magic {
		[DllExport("Init", CallingConvention.Winapi)]
		public static void Init() {
			Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();
			for (int i = 0; i < AllTypes.Length; i++)
				Marshal.PrelinkAll(AllTypes[i]);
			Program.Main();
			Program.Unload();
		}
	}

	static class Program {
		[Flags()]
		public enum ModuleHandleFlags : uint {
			Pin = 0x1,
			UnchangedRefCount = 0x2,
			FromAddress = 0x4,
		}

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool FreeLibrary(IntPtr Lib);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern void FreeLibraryAndExitThread(IntPtr Lib, int Code);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool GetModuleHandleEx(ModuleHandleFlags Flags, string ModuleName, out IntPtr Handle);

		public static bool Unload() {
			IntPtr This;
			if (!GetModuleHandleEx(ModuleHandleFlags.UnchangedRefCount, "Inkjet.dll", out This))
				return false;
			while (FreeLibrary(This))
				;
			return true;
		}

		public static void Main() {
			Process Cur = Process.GetCurrentProcess();
			string Fmt = string.Format("{0} ({1})", Cur.ProcessName, Cur.Id);
			//File.WriteAllText("E:\\Projects\\Hackery\\bin\\HAAX.txt", Fmt);
			MessageBox.Show("Magic!", Fmt);
		}
	}
}