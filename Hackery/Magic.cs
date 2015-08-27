using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Hackery {
	static class Magic {
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

				NTdll.CsrClientCallServer(PInfo.Process, PInfo.Thread, PInfo.CID.UniqueProcess,
					PInfo.CID.UniqueThread);
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
	}
}