using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.IO;

namespace Hackery {
	public static class Magic {
		public static void UnmanagedDelete<T>(T Obj) where T : UObject {
			if (Obj == null)
				return;

			// Disabled this before i end up writing another garbage collector
			/*FieldInfo[] Fields = Obj.GetType().GetFields();
			for (int i = 0; i < Fields.Length; i++) 
				if (typeof(UObject).IsAssignableFrom(Fields[i].FieldType))
					UnmanagedDelete(Fields[i].GetValue(Obj) as T);*/

			((UObject)Obj).dtor();
			Marshal.FreeHGlobal(Obj.ToObjPointer() - 4);
		}

		public static T UnmanagedNew<T>(out IntPtr Handle) where T : UObject {
			Type ClassType = typeof(T);
			Marshal.PrelinkAll(ClassType);
			IntPtr TypePtr = ClassType.TypeHandle.Value;
			int Size = Marshal.ReadInt32(TypePtr, 4);

			// Allocate
			Handle = Marshal.AllocHGlobal(Size);
			for (int i = 0; i < Size; i++)
				Marshal.WriteByte(Handle + i, 0);

			Marshal.WriteInt32(Handle, 0); // Dunno what this one is
			Marshal.WriteIntPtr(Handle + 4, TypePtr); // Type info
			T Ret = (Handle + 4).ToObject() as T;
			((UObject)Ret).ctor();
			return Ret;
		}

		public static T UnmanagedNew<T>() where T : UObject {
			IntPtr Ptr;
			return UnmanagedNew<T>(out Ptr);
		}

		public unsafe static int Fork() {
			ProcessInfo PInfo = new ProcessInfo();

			//Kernel32.FreeConsole();
			CloneStatus S = NTdll.RtlCloneUserProcess(CloneProcessFlags.CreateSuspended | CloneProcessFlags.InheritHandles, &PInfo);
			/*Kernel32.AllocConsole();
			Console.OpenStandardError();
			Console.OpenStandardInput();
			Console.OpenStandardOutput();*/

			if (S == CloneStatus.Parent) {
				int ChildPID = Kernel32.GetProcessId(PInfo.Process);
				if (ChildPID == 0)
					return -2;

				NTdll.CsrClientCallServer(PInfo.Process, PInfo.Thread, PInfo.CID.ProcessID,
					PInfo.CID.ThreadID);
				Kernel32.ResumeThread(PInfo.Thread);
				Kernel32.CloseHandle(PInfo.Process);
				Kernel32.CloseHandle(PInfo.Thread);
				return ChildPID;
			} else if (S == CloneStatus.Child) {
				Kernel32.FreeConsole();
				Kernel32.AllocConsole();

				Console.SetIn(new StreamReader(Console.OpenStandardInput()));
				StreamWriter OutWriter = new StreamWriter(Console.OpenStandardOutput());
				OutWriter.AutoFlush = true;
				Console.SetOut(OutWriter);
				StreamWriter ErrWriter = new StreamWriter(Console.OpenStandardError());
				ErrWriter.AutoFlush = true;
				Console.SetError(ErrWriter);
				return 0;
			}
			return -1;
		}

		public static int FuncLen(IntPtr FuncPtr) {
			int Len = 0;
			while (Marshal.ReadByte(FuncPtr + Len) != 0xC3)
				Len++;
			return Len + 1;
		}

		public static void Inject(Process Proc, string Module, string Fnc, bool WaitAndFree = false, bool Debug = false) {
			Inject(Proc.Id, Module, Fnc, WaitAndFree, Debug);
		}

		public static void Inject(int PID, string Module, string Fnc, bool WaitAndFree = false, bool Debug = false) {
			if (Debug)
				try {
					Process.EnterDebugMode();
				} catch (Win32Exception) {
				}

			IntPtr Kernel = Kernel32.LoadLibrary("kernel32.dll");
			if (Kernel == IntPtr.Zero)
				throw new Win32Exception();

			IntPtr Mod = Kernel32.LoadLibrary(Module);
			if (Mod == IntPtr.Zero)
				throw new Win32Exception();

			IntPtr Proc = Kernel32.OpenProcess(ProcessAccess.AllAccess, false, PID);
			if (Proc == IntPtr.Zero)
				throw new Win32Exception();

			IntPtr ProcMemory = Kernel32.VirtualAllocEx(Proc, IntPtr.Zero, 4096);
			if (ProcMemory == IntPtr.Zero)
				throw new Win32Exception();

			if (!Kernel32.WriteProcessMemory(Proc, ProcMemory, Encoding.ASCII.GetBytes(Path.GetFullPath(Module))))
				throw new Win32Exception();

			ExecThread(Proc, Kernel32.GetProcAddress(Kernel, "LoadLibraryA"), ProcMemory, true);
			ExecThread(Proc, Kernel32.GetProcAddress(Mod, Fnc), IntPtr.Zero, WaitAndFree);
			if (WaitAndFree)
				ExecThread(Proc, Kernel32.GetProcAddress(Kernel, "FreeLibrary"), Mod, true);

			if (!Kernel32.CloseHandle(Proc))
				throw new Win32Exception();
			if (!Kernel32.FreeLibrary(Kernel))
				throw new Win32Exception();
			if (!Kernel32.FreeLibrary(Mod))
				throw new Win32Exception();
			if (Debug)
				try {
					Process.LeaveDebugMode();
				} catch (Win32Exception) {
				}
		}

		public static int ExecThread(IntPtr Proc, IntPtr Func, IntPtr Param, bool Wait = false) {
			IntPtr Thread;
			NTdll.RtlCreateUserThread(Proc, Func, Param, out Thread);
			int Ret = 0;
			if (Wait)
				Ret = Kernel32.WaitForSingleObject(Thread, Kernel32.INFINITE);
			if (!Kernel32.CloseHandle(Thread))
				throw new Win32Exception();
			return Ret;
		}

		public static ProcessThread GetCurrentProcessThread() {
			//Thread.BeginThreadAffinity();
			int CurThreadID = (int)Kernel32.GetCurrentThreadId();
			//Thread.EndThreadAffinity();

			Process CurProc = Process.GetCurrentProcess();
			foreach (ProcessThread PThread in CurProc.Threads)
				if (PThread.Id == CurThreadID)
					return PThread;
			return null;
		}

		public static void SuspendProcess(Process Proc, ProcessThread Except = null) {
			if (string.IsNullOrEmpty(Proc.ProcessName))
				return;

			foreach (ProcessThread PThread in Proc.Threads) {
				if (PThread == Except)
					continue;
				IntPtr OpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)PThread.Id);
				if (OpenThread == IntPtr.Zero)
					continue;


				Kernel32.SuspendThread(OpenThread);
				Kernel32.CloseHandle(OpenThread);
			}
		}

		public static void ResumeProcess(Process Proc) {
			if (string.IsNullOrEmpty(Proc.ProcessName))
				return;

			foreach (ProcessThread ProcessThread in Proc.Threads) {
				IntPtr OpenThread = Kernel32.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)ProcessThread.Id);

				if (OpenThread == IntPtr.Zero)
					continue;

				/*int Suspendcount = 0;
				do {
					Suspendcount = Kernel32.ResumeThread(OpenThread);
				} while (Suspendcount > 0);*/
				Kernel32.ResumeThread(OpenThread);
				Kernel32.CloseHandle(OpenThread);
			}
		}

		public static Process CreateProcess(string Pth, string CmdLine, ProcessCreationFlags Flags) {
			STARTUPINFO SInf = new STARTUPINFO();
			PROCESS_INFORMATION PInfo = new PROCESS_INFORMATION();
			Kernel32.CreateProcess(Pth, CmdLine, IntPtr.Zero, IntPtr.Zero, false, Flags, IntPtr.Zero, null, ref SInf, out PInfo);
			return Process.GetProcessById((int)PInfo.ClientId.ProcessID);
		}
	}
}