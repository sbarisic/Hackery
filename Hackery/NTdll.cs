using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Hackery {
	[StructLayout(LayoutKind.Sequential)]
	struct CLIENT_ID {
		public IntPtr UniqueProcess;
		public IntPtr UniqueThread;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct SectionImageInfo {
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
	struct PROCESS_INFORMATION {
		public IntPtr Process;
		public IntPtr Thread;
		public CLIENT_ID CID;

		public PROCESS_INFORMATION(IntPtr Process, IntPtr Thread, IntPtr PID, IntPtr TID) {
			this.Process = Process;
			this.Thread = Thread;
			this.CID = new CLIENT_ID();
			this.CID.UniqueProcess = PID;
			this.CID.UniqueThread = TID;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	struct ProcessInfo {
		public uint Size;
		public IntPtr Process;
		public IntPtr Thread;
		public CLIENT_ID CID;
		public SectionImageInfo ImageInfo;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct CSRSS_MESSAGE {
		public uint Unknown1;
		public uint Opcode;
		public uint Status;
		public uint Unknown2;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct PORT_MESSAGE {
		public uint Unknown1;
		public uint Unknown2;
		public CLIENT_ID ClientID;
		public uint MessageID;
		public uint CallbackID;
	}

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct CSRMsg {
		public PORT_MESSAGE PortMsg;
		public CSRSS_MESSAGE CSRSSMsg;
		public PROCESS_INFORMATION ProcessInfo;
		public CLIENT_ID CID;
		public uint CreationFlags;
		public fixed uint VdmInfo[2];
	}

	[Flags()]
	enum CloneProcessFlags : uint {
		CreateSuspended = 0x1,
		InheritHandles = 0x2,
		NoSync = 0x4,
	}

	enum CloneStatus : int {
		Parent = 0,
		Child = 297
	}

	unsafe static class NTdll {
		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern CloneStatus RtlCloneUserProcess(CloneProcessFlags Flags, IntPtr ProcSecDesc, IntPtr ThreadSecDesc,
			IntPtr DebugPort, ProcessInfo* ProcessInfo);

		public static CloneStatus RtlCloneUserProcess(CloneProcessFlags Flags, ProcessInfo* ProcessInfo) {
			return RtlCloneUserProcess(Flags, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ProcessInfo);
		}

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern int RtlCreateUserThread(IntPtr Proc, IntPtr SecDesc, bool CreateSuspended, uint StackZeroBits,
			uint StackReserved, uint StackCommit, IntPtr StartAddr, IntPtr StartParam, IntPtr Thread, CLIENT_ID* Result);

		public static int RtlCreateUserThread(IntPtr P, IntPtr Fnc, IntPtr Data, out IntPtr Thread) {
			Thread = IntPtr.Zero;
			fixed (IntPtr* ThrdPtr = &Thread)
				return RtlCreateUserThread(P, IntPtr.Zero,
					false, 0, 0, 0, Fnc, Data, new IntPtr(ThrdPtr), (CLIENT_ID*)0);
		}

		public static int RtlCreateUserThread(Process P, IntPtr Fnc, IntPtr Data, out IntPtr Thread) {
			IntPtr Proc = Kernel32.OpenProcess(ProcessAccess.AllAccess, false, P.Id);
			int Ret = RtlCreateUserThread(Proc, Fnc, Data, out Thread);
			Kernel32.CloseHandle(Proc);
			return Ret;
		}

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern void RtlExitUserThread(int Status);

		[DllImport("ntdll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		public static extern void CsrClientCallServer(CSRMsg* Msg, int A = 0, int B = 0x10000, int C = 0x24);

		public static void CsrClientCallServer(IntPtr Process, IntPtr Thread, int PID, int TID) {
			CsrClientCallServer(Process, Thread, new IntPtr(PID), new IntPtr(TID));
		}

		public static void CsrClientCallServer(IntPtr Process, IntPtr Thread, IntPtr PID, IntPtr TID) {
			CSRMsg CSRMessage = new CSRMsg();
			CSRMessage.ProcessInfo = new PROCESS_INFORMATION(Process, Thread, PID, TID);
			NTdll.CsrClientCallServer(&CSRMessage);
		}
	}
}