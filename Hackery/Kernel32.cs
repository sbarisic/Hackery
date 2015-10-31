using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Hackery {
	[Flags]
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

	[Flags]
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

	[Flags]
	public enum ModuleHandleFlags : uint {
		Pin = 0x1,
		UnchangedRefCount = 0x2,
		FromAddress = 0x4,
	}

	[Flags]
	public enum ThreadAccess : int {
		TERMINATE = (0x0001),
		SUSPEND_RESUME = (0x0002),
		GET_CONTEXT = (0x0008),
		SET_CONTEXT = (0x0010),
		SET_INFORMATION = (0x0020),
		QUERY_INFORMATION = (0x0040),
		SET_THREAD_TOKEN = (0x0080),
		IMPERSONATE = (0x0100),
		DIRECT_IMPERSONATION = (0x0200)
	}

	[Flags]
	public enum ProcessCreationFlags : uint {
		ZERO_FLAG = 0x00000000,
		CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
		CREATE_DEFAULT_ERROR_MODE = 0x04000000,
		CREATE_NEW_CONSOLE = 0x00000010,
		CREATE_NEW_PROCESS_GROUP = 0x00000200,
		CREATE_NO_WINDOW = 0x08000000,
		CREATE_PROTECTED_PROCESS = 0x00040000,
		CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
		CREATE_SEPARATE_WOW_VDM = 0x00001000,
		CREATE_SHARED_WOW_VDM = 0x00001000,
		CREATE_SUSPENDED = 0x00000004,
		CREATE_UNICODE_ENVIRONMENT = 0x00000400,
		DEBUG_ONLY_THIS_PROCESS = 0x00000002,
		DEBUG_PROCESS = 0x00000001,
		DETACHED_PROCESS = 0x00000008,
		EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
		INHERIT_PARENT_AFFINITY = 0x00010000
	}

	public struct STARTUPINFO {
		public uint cb;
		public string lpReserved;
		public string lpDesktop;
		public string lpTitle;
		public uint dwX;
		public uint dwY;
		public uint dwXSize;
		public uint dwYSize;
		public uint dwXCountChars;
		public uint dwYCountChars;
		public uint dwFillAttribute;
		public uint dwFlags;
		public short wShowWindow;
		public short cbReserved2;
		public IntPtr lpReserved2;
		public IntPtr hStdInput;
		public IntPtr hStdOutput;
		public IntPtr hStdError;
	}

	public unsafe static class Kernel32 {
		public const uint INFINITE = 0xFFFFFFFF;

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool CreateProcess(string AppName, string CmdLine, IntPtr Attributes,
			IntPtr ThreadAttribs, bool InheritHandles, ProcessCreationFlags CFlags,
			IntPtr Env, string Currentdir, ref STARTUPINFO StInfo, out PROCESS_INFORMATION ProcInfo);

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
		public static extern bool CloseHandle(IntPtr Hnd);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr GetCurrentThread();

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern uint GetCurrentThreadId();

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr OpenThread(ThreadAccess Access, bool InheritHandle, uint ThreadID);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern int SuspendThread(IntPtr HThread);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern int ResumeThread(IntPtr Thrd);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern int WaitForSingleObject(IntPtr Handle, uint MS);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr OpenProcess(ProcessAccess Access, bool InheritHandle, int PID);

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr GetProcAddress(IntPtr Lib, string ProcName);

		public static T GetProcAddress<T>(IntPtr Lib, string ProcName) where T : class {
			if (!typeof(Delegate).IsAssignableFrom(typeof(T)))
				throw new Exception("T has to be a delegate type");
			return Marshal.GetDelegateForFunctionPointer(GetProcAddress(Lib, ProcName), typeof(T)) as T;
		}

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

		[DllImport("kernel32", SetLastError = true)]
		public static extern uint GetModuleFileName(IntPtr Mod, StringBuilder FileName, int Size = 80);

		public static uint GetModuleFileName(IntPtr Mod, StringBuilder FileName) {
			return GetModuleFileName(Mod, FileName, FileName.Capacity);
		}
	}
}