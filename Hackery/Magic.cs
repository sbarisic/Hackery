using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Hackery
{
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

			using (MemoryManagement.Protect(OldFunc, (uint)JMP.Length, MemProtection.ExecReadWrite)) {
				byte[] Orig = new byte[JMP.Length];
				Marshal.Copy(OldFunc, Orig, 0, Orig.Length);
				Marshal.Copy(JMP, 0, OldFunc, JMP.Length);
				return Orig;
			}
		}

		public static HookHandle Hook(MethodInfo OldFunc, IntPtr NewFunc) => new HookHandle(OldFunc, Hook(GetNativePointer(OldFunc), NewFunc));

		public static HookHandle Hook(MethodInfo OldFunc, MethodInfo NewFunc) => Hook(OldFunc, GetNativePointer(NewFunc));

		/// <summary>
		/// Modifies the underlying method of <paramref name="old"/> to immediately jump to <paramref name="new"/>.
		/// </summary>
		/// <typeparam name="T">A delegate type matching the methods to hook together.</typeparam>
		/// <param name="old">An instance of <typeparamref name="T"/> pointing to the method to hook.</param>
		/// <param name="new">An instance of <typeparamref name="T"/> pointing to the hook target.</param>
		public static HookHandle Hook<T>(T old, T @new) {
			if (typeof(Delegate).IsAssignableFrom(typeof(T)) == false) throw new InvalidOperationException("T must be a Delegate type.");
			var oldMethod = ((Delegate)(object)old).Method;
			var newMethod = ((Delegate)(object)@new).Method;

			if (oldMethod.IsStatic != newMethod.IsStatic) throw new ArgumentException("OldFunc and NewFunc must be either both static or both instance methods for this Hook overload.");
			if (oldMethod.IsStatic == false && newMethod.DeclaringType.IsAssignableFrom(oldMethod.DeclaringType) == false) throw new ArgumentException("\"This\" parameter type mismatch.");

			return Hook(oldMethod, newMethod);
		}

		/// <summary>
		/// Modifies the underlying method of <paramref name="old"/> to immediately jump to <paramref name="new"/>.
		/// </summary>
		/// <typeparam name="THooked">A delegate type matching the method to hook.</typeparam>
		/// <typeparam name="THook">A delegate type matching the method hooked into <paramref name="old"/>.</typeparam>
		/// <param name="old">An instance of <typeparamref name="THooked"/> pointing to the method to hook.</param>
		/// <param name="new">An instance of <typeparamref name="THook"/> pointing to the hook target.</param>
		public static HookHandle Hook<THooked, THook>(THooked old, THook @new) {
			if (typeof(Delegate).IsAssignableFrom(typeof(THooked)) == false) throw new InvalidOperationException("THooked must be a Delegate type.");
			if (typeof(Delegate).IsAssignableFrom(typeof(THook)) == false) throw new InvalidOperationException("THook must be a Delegate type.");
			var oldMethod = ((Delegate)(object)old).Method;
			var newMethod = ((Delegate)(object)@new).Method;
			
			if (oldMethod.ReturnType.IsAssignableFrom(newMethod.ReturnType)) throw new ArgumentException($"Return type mismatch: {newMethod.ReturnType} isn't assignable to {oldMethod.ReturnType}.");

			//TODO: Check if this works despite the apparent mismatch. I think it's impossible due to unbalancing the stack or something along those lines, but that may be not be the case.
			// For whatever reason typeof(object).IsAssignableFrom(typeof(void)) is true.
			if (oldMethod.ReturnType != typeof(void) && newMethod.ReturnType == typeof(void)) throw new ArgumentException($"Return type mismatch: Can't hook method returning void with one that returns something.");

			var oldParameters = oldMethod.GetParameters().Select(p => p.ParameterType).ToList();
			if (oldMethod.IsStatic == false) oldParameters.Insert(0, oldMethod.DeclaringType);

			var newParameters = newMethod.GetParameters().Select(p => p.ParameterType).ToList();
			if (newMethod.IsStatic == false) newParameters.Insert(0, newMethod.DeclaringType);

			//TODO: Check whether a method with fewer parameters can safely be hooked into one with more.
			if (oldParameters.Count != newParameters.Count) throw new ArgumentException($"Parameter count (eventually including initial \"this\" parameter) mismatch: Tried to hook method with {oldParameters.Count} with one with {newParameters.Count} parameters.");
			for (int i = 0; i < oldParameters.Count; i++)
			{ if (newParameters[i].IsAssignableFrom(oldParameters[i]) == false) throw new ArgumentException($"Parameter type mismatch: Can't convert assign {oldParameters[i]} to {newParameters[i]} at position {i} (eventually including initial \"this\" parameter)."); }

			return Hook(oldMethod, newMethod);
		}

		public static IntPtr GetNativePointer(MethodInfo M) {
			return M.MethodHandle.GetFunctionPointer();
		}
	}
}