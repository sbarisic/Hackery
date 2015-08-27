using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq.Expressions;
using System.IO;

namespace Hackery {
	class Program {
		static int ReadInt(string Prompt) {
			Console.Write(Prompt);
			return int.Parse(Console.ReadLine());
		}

		static void Main(string[] args) {
			Console.Title = "Hackery";
			Console.WriteLine("PID: {0}", Process.GetCurrentProcess().Id);

			Inject(ReadInt("Enter PID: "), "Inkjet.dll", "Init");

			Console.WriteLine("Done!");
			Console.ReadLine();
		}

		static void Inject(int PID, string Module, string Fnc) {
			IntPtr Kernel = Kernel32.LoadLibrary("kernel32.dll");
			IntPtr Mod = Kernel32.LoadLibrary(Module);
			IntPtr Proc = Kernel32.OpenProcess(ProcessAccess.AllAccess, false, PID);

			IntPtr NameMem = Kernel32.VirtualAllocEx(Proc, IntPtr.Zero, 4096);
			Kernel32.WriteProcessMemory(Proc, NameMem, Encoding.ASCII.GetBytes(Path.GetFullPath(Module)));

			ExecThread(Proc, Kernel32.GetProcAddress(Kernel, "LoadLibraryA"), NameMem, true);
			ExecThread(Proc, Kernel32.GetProcAddress(Mod, Fnc), IntPtr.Zero);

			Kernel32.CloseHandle(Proc);
			Kernel32.FreeLibrary(Kernel);
		}

		static int ExecThread(IntPtr Proc, IntPtr Func, IntPtr Param, bool Wait = false) {
			IntPtr Thread;
			NTdll.RtlCreateUserThread(Proc, Func, Param, out Thread);
			int Ret = 0;
			if (Wait)
				Ret = Kernel32.WaitForSingleObject(Thread, Kernel32.INFINITE);
			Kernel32.CloseHandle(Thread);
			return Ret;
		}
	}
}