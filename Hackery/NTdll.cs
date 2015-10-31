using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Hackery {
	[StructLayout(LayoutKind.Sequential)]
	public struct CLIENT_ID {
		public uint ProcessID;
		public uint ThreadID;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SectionImageInfo {
		public IntPtr EntryPoint;
		public uint StackZeroBits;
		public uint StackReserved;
		public uint StackCommit;
		public uint ImageSubsystem;
		public ushort SubSysVerLow;
		public ushort SubSysVerHigh;
		public uint U1;
		public uint ImageStats;
		public uint ImageMachineType;
		public fixed uint U2[3];
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PROCESS_INFORMATION {
		public IntPtr Process;
		public IntPtr Thread;
		public CLIENT_ID ClientId;

		public PROCESS_INFORMATION(IntPtr Process, IntPtr Thread, uint PID, uint TID) {
			this.Process = Process;
			this.Thread = Thread;
			this.ClientId = new CLIENT_ID();
			this.ClientId.ProcessID = PID;
			this.ClientId.ThreadID = TID;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ProcessInfo {
		public uint Size;
		public IntPtr Process;
		public IntPtr Thread;
		public CLIENT_ID CID;
		public SectionImageInfo ImageInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct CSRSS_MESSAGE {
		public uint Unknown1;
		public uint Opcode;
		public uint Status;
		public uint Unknown2;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PORT_MESSAGE {
		public uint Unknown1;
		public uint Unknown2;
		public CLIENT_ID ClientID;
		public uint MessageID;
		public uint CallbackID;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct CSRMsg {
		public PORT_MESSAGE PortMsg;
		public CSRSS_MESSAGE CSRSSMsg;
		public PROCESS_INFORMATION ProcessInfo;
		public CLIENT_ID CID;
		public uint CreationFlags;
		public fixed uint VdmInfo[2];
	}

	[Flags()]
	public enum CloneProcessFlags : uint {
		CreateSuspended = 0x1,
		InheritHandles = 0x2,
		NoSync = 0x4,
	}

	public enum CloneStatus : int {
		Parent = 0,
		Child = 297
	}

	public unsafe static class NTdll {
		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern CloneStatus RtlCloneUserProcess(CloneProcessFlags Flags, IntPtr ProcSecDesc, IntPtr ThreadSecDesc,
			IntPtr DebugPort, ProcessInfo* ProcessInfo);

		public static CloneStatus RtlCloneUserProcess(CloneProcessFlags Flags, ProcessInfo* ProcessInfo) {
			return RtlCloneUserProcess(Flags, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ProcessInfo);
		}

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern bool RtlCreateUserThread(IntPtr Proc, IntPtr SecDesc, bool CreateSuspended, uint StackZeroBits,
			uint StackReserved, uint StackCommit, IntPtr StartAddr, IntPtr StartParam, IntPtr Thread, CLIENT_ID* Result);

		public static bool RtlCreateUserThread(IntPtr P, IntPtr Fnc, IntPtr Data, out IntPtr Thread) {
			Thread = IntPtr.Zero;
			fixed (IntPtr* ThrdPtr = &Thread)
				return RtlCreateUserThread(P, IntPtr.Zero,
					false, 0, 0, 0, Fnc, Data, new IntPtr(ThrdPtr), (CLIENT_ID*)0);
		}

		public static bool RtlCreateUserThread(Process P, IntPtr Fnc, IntPtr Data, out IntPtr Thread) {
			IntPtr Proc = Kernel32.OpenProcess(ProcessAccess.AllAccess, false, P.Id);
			bool Ret = RtlCreateUserThread(Proc, Fnc, Data, out Thread);
			Kernel32.CloseHandle(Proc);
			return Ret;
		}

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern void RtlExitUserThread(int Status);

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern void RtlExitUserProcess(int Status);

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern uint ZwAllocateVirtualMemory(IntPtr Proc, ref IntPtr Addr, int ZeroBits, ref IntPtr RegionSize,
			AllocType AType = AllocType.Commit | AllocType.Reserve, MemProtection Prot = MemProtection.ReadWrite);

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern void CsrClientCallServer(CSRMsg* Msg, int A = 0, int B = 0x10000, int C = 0x24);

		public static void CsrClientCallServer(IntPtr Process, IntPtr Thread, int PID, int TID) {
			CsrClientCallServer(Process, Thread, (uint)PID, (uint)TID);
		}

		public static void CsrClientCallServer(IntPtr Process, IntPtr Thread, uint PID, uint TID) {
			CSRMsg CSRMessage = new CSRMsg();
			CSRMessage.ProcessInfo = new PROCESS_INFORMATION(Process, Thread, PID, TID);
			NTdll.CsrClientCallServer(&CSRMessage);
		}
	}
}