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
	class Magic {
		[DllExport("Init", CallingConvention.Winapi)]
		public static void Init() {
			Type[] AllTypes = Assembly.GetExecutingAssembly().GetTypes();
			for (int i = 0; i < AllTypes.Length; i++)
				Marshal.PrelinkAll(AllTypes[i]);
			Program.Main();
			Program.Unload();
		}
	}

	unsafe static class Program {
		static IntPtr ThisModule;

		public static bool Unload() {
			if (ThisModule == IntPtr.Zero)
				return false;
			while (Native.FreeLibrary(ThisModule))
				;
			return true;
		}

		public static void Main() {
			if (!Native.GetModuleHandleEx(ModuleHandleFlags.UnchangedRefCount, "Inkjet.dll", out ThisModule))
				ThisModule = IntPtr.Zero;

			IntPtr ExeHandle;
			List<string> ExportNames = new List<string>();
			if (Native.GetModuleHandleEx(ModuleHandleFlags.UnchangedRefCount, null, out ExeHandle)) {
				IMAGE_DOS_HEADER* DosHeader = (IMAGE_DOS_HEADER*)ExeHandle;
				IMAGE_NT_HEADERS* Header = (IMAGE_NT_HEADERS*)(ExeHandle + (int)DosHeader->LFaNew);
				IMAGE_EXPORT_DIRECTORY* Exports = (IMAGE_EXPORT_DIRECTORY*)(ExeHandle +
					Header->OptionalHeader.ExportTable.VirtualAddress);

				IntPtr Names = ExeHandle + Exports->AddressOfNames;
				for (int i = 0; i < Exports->NumberOfNames; i++) {
					string Name = Marshal.PtrToStringAnsi(ExeHandle + ((int*)Names)[i]);
					if (Name.Trim().Length == 0)
						continue;
					ExportNames.Add(Name);
				}
			}

			File.WriteAllText("E:\\Projects\\Hackery\\bin\\HAAX.txt", string.Join("\n", ExportNames));

			Process Cur = Process.GetCurrentProcess();
			MessageBox.Show("Magic!", string.Format("{0} ({1})", Cur.ProcessName, Cur.Id));
		}
	}
}