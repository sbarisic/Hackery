using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Hackery {
	[Flags()]
	public enum MemProtection : uint {
		NoAccess = 0x01,
		ReadOnly = 0x02,
		ReadWrite = 0x04,
		WriteCopy = 0x08,
		Exec = 0x10,
		ExecRead = 0x20,
		ExecReadWrite = 0x40,
		ExecWriteCopy = 0x80,
		PageGuard = 0x100,
		NoCache = 0x200,
		WriteCombine = 0x400
	}

	[Flags()]
	public enum AllocType : uint {
		Commit = 0x1000,
		Reserve = 0x2000,
		Reset = 0x80000,
		LargePages = 0x20000000,
		Physical = 0x400000,
		TopDown = 0x100000,
		WriteWatch = 0x200000
	}

	public enum ProcessAccess : uint {
		AllAccess = 0x1F0FFF
	}

	[Flags()]
	public enum ModuleHandleFlags : uint {
		Pin = 0x1,
		UnchangedRefCount = 0x2,
		FromAddress = 0x4,
	}

	public unsafe static class Kernel32 {
		public const uint INFINITE = 0xFFFFFFFF;

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool VirtualProtect(IntPtr Addr, uint Size, MemProtection NewProtect, out MemProtection OldProtect);

		public static bool VirtualProtect(IntPtr Addr, int Size, MemProtection NewProtect, out MemProtection OldProtect) {
			return VirtualProtect(Addr, (uint)Size, NewProtect, out OldProtect);
		}

		public static bool VirtualProtect(IntPtr Addr, uint Size, MemProtection NewProtect) {
			MemProtection Old;
			return VirtualProtect(Addr, Size, NewProtect, out Old);
		}

		public static bool VirtualProtect(IntPtr Addr, int Size, MemProtection NewProtect) {
			return VirtualProtect(Addr, (uint)Size, NewProtect);
		}

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool AllocateUserPhysicalPages(IntPtr Process, ref uint NumOfPages, IntPtr PFNArray);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool MapUserPhysicalPages(IntPtr Addr, uint NumOfPages, IntPtr PFNArray);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool AllocConsole();

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool FreeConsole();

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool AttachConsole(int PID);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern int GetProcessId(IntPtr Hnd);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern uint ResumeThread(IntPtr Thrd);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool CloseHandle(IntPtr Hnd);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr GetCurrentThread();

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern int WaitForSingleObject(IntPtr Handle, uint MS);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr OpenProcess(ProcessAccess Access, bool InheritHandle, int PID);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr GetProcAddress(IntPtr Lib, string ProcName);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr LoadLibrary(string Name);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool FreeLibrary(IntPtr Lib);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern void FreeLibraryAndExitThread(IntPtr Lib, int Code);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr GetModuleHandle(string ModuleName);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool GetModuleHandleEx(ModuleHandleFlags Flags, string ModuleName, out IntPtr Handle);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr VirtualAllocEx(IntPtr Proc, IntPtr Addr, int Size,
			AllocType AType = AllocType.Commit, MemProtection Prot = MemProtection.ReadWrite);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr VirtualAlloc(IntPtr Addr, int Size, AllocType AType = AllocType.Commit,
			MemProtection Prot = MemProtection.ReadWrite);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool WriteProcessMemory(IntPtr Proc, IntPtr Addr, byte[] Mem, int Size, ref int BytesWritten);

		public static bool WriteProcessMemory(IntPtr Proc, IntPtr Addr, byte[] Mem) {
			int I = 0;
			return WriteProcessMemory(Proc, Addr, Mem, Mem.Length, ref I);
		}

		[DllImport("kernel32", CallingConvention = CallingConvention.Winapi)]
		public static extern int GetLastError();
	}
}