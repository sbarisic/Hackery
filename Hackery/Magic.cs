using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;

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

		public static byte[] Hook(IntPtr OldFunc, IntPtr NewFunc) {
			byte[] JMP = new byte[] { 0xE9, 0x90, 0x90, 0x90, 0x90, 0xC3 };
			Array.Copy(BitConverter.GetBytes(NewFunc.ToInt32() - OldFunc.ToInt32() - 5), 0, JMP, 1, 4);

			MemProtection Old;
			Kernel32.VirtualProtect(OldFunc, (uint)JMP.Length, MemProtection.ExecReadWrite, out Old);
			byte[] Orig = new byte[JMP.Length];
			Marshal.Copy(OldFunc, Orig, 0, Orig.Length);
			Marshal.Copy(JMP, 0, OldFunc, JMP.Length);
			Kernel32.VirtualProtect(OldFunc, (uint)JMP.Length, Old);
			return Orig;
		}

		public static void Hook(MethodInfo OldFunc, IntPtr NewFunc) {
			Hook(GetNativePointer(OldFunc), NewFunc);
		}

		public static void Hook(MethodInfo OldFunc, MethodInfo NewFunc) {
			Hook(OldFunc, GetNativePointer(NewFunc));
		}

		public static IntPtr GetNativePointer(MethodInfo M) {
			return M.MethodHandle.GetFunctionPointer();
		}
	}
}