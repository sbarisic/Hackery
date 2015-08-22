using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Linq;
using System.Linq.Expressions;

namespace Hackery {
	/// <summary>
	/// <para>A handle to safely temporarily hook a method.</para>
	/// <para>The hook is reset when the handle is disposed.</para>
	/// </summary>
	public class HookHandle : IDisposable {
		/// <summary>
		/// The hooked method.
		/// </summary>
		public IntPtr HookedFunc {
			get;
			private set;
		}

		public bool Hooked {
			get;
			private set;
		}

		byte[] _OriginalIntro;
		byte[] _HookedIntro;

		void WriteIntro(byte[] Intro) {
			using (MemoryManagement.Protect(HookedFunc, Intro.Length, MemProtection.ExecReadWrite)) {
				Marshal.Copy(Intro, 0, HookedFunc, Intro.Length);
			}
		}

		void Init(IntPtr hookedFunc, byte[] originalIntro, byte[] hookedIntro) {
			HookedFunc = hookedFunc;
			_OriginalIntro = originalIntro;
			_HookedIntro = hookedIntro;
			Hook();
		}

		/// <summary>
		/// Creates a new <see cref="HookHandle"/> instance with the given parameters.
		/// </summary>
		/// <param name="hookedMethod">The hooked method.</param>
		/// <param name="originalIntro">The original intro of the hooked method.</param>
		/// <param name="hookedIntro">The custom into of the hooked method.</param>
		public HookHandle(IntPtr hookedFunc, byte[] originalIntro, byte[] hookedIntro) {
			Init(hookedFunc, originalIntro, hookedIntro);
		}

		public HookHandle(IntPtr hookedFunc, IntPtr newFunc) {
			byte[] NewIntro = new byte[] { 0xE9, 0x90, 0x90, 0x90, 0x90, 0xC3 };
			Array.Copy(BitConverter.GetBytes(newFunc.ToInt32() - hookedFunc.ToInt32() - 5), 0, NewIntro, 1, 4);

			byte[] OrigIntro;
			using (MemoryManagement.Protect(hookedFunc, NewIntro.Length, MemProtection.ExecReadWrite)) {
				OrigIntro = new byte[NewIntro.Length];
				Marshal.Copy(hookedFunc, OrigIntro, 0, OrigIntro.Length);
			}
			Init(hookedFunc, OrigIntro, NewIntro);
		}

		public void Unhook() {
			if (!Hooked)
				return;
			Hooked = false;
			WriteIntro(_OriginalIntro);
		}

		public void Hook() {
			if (Hooked)
				return;
			Hooked = true;
			WriteIntro(_HookedIntro);
		}

		/// <summary>
		/// Restores <see cref="HookedMethod"/> to its previous state.
		/// </summary>
		public void Dispose() {
			Unhook();
		}

		public static void CreateHook() {

		}

		public static HookHandle CreateHook(MethodInfo Old, MethodInfo New) {
			return new HookHandle(Old.MethodHandle.GetFunctionPointer(), New.MethodHandle.GetFunctionPointer());
		}

		public static HookHandle CreateHook(Expression<Action> OldExpr, MethodInfo New) {
			if (OldExpr.Body is MethodCallExpression == false)
				throw new ArgumentException("Expression body isn't a method!", "OldExpr");
			return CreateHook(((MethodCallExpression)OldExpr.Body).Method, New);
		}

		public static HookHandle CreateHook(Expression<Action> OldExpr, Expression<Action> NewExpr) {
			if (NewExpr.Body is MethodCallExpression == false)
				throw new ArgumentException("Expression body isn't a method!", "NewExpr");
			return CreateHook(OldExpr, ((MethodCallExpression)NewExpr.Body).Method);
		}

		/// <summary>
		/// Modifies the underlying method of <paramref name="old"/> to immediately jump to <paramref name="new"/>.
		/// </summary>
		/// <typeparam name="T">A delegate type matching the methods to hook together.</typeparam>
		/// <param name="old">An instance of <typeparamref name="T"/> pointing to the method to hook.</param>
		/// <param name="new">An instance of <typeparamref name="T"/> pointing to the hook target.</param>
		public static HookHandle CreateHook<T>(T old, T @new) {
			if (typeof(Delegate).IsAssignableFrom(typeof(T)) == false)
				throw new InvalidOperationException("T must be a Delegate type.");
			var oldMethod = ((Delegate)(object)old).Method;
			var newMethod = ((Delegate)(object)@new).Method;

			if (oldMethod.IsStatic != newMethod.IsStatic)
				throw new ArgumentException("OldFunc and NewFunc must be either both static or both instance methods for this Hook overload.");
			if (oldMethod.IsStatic == false && newMethod.DeclaringType.IsAssignableFrom(oldMethod.DeclaringType) == false)
				throw new ArgumentException("\"This\" parameter type mismatch.");

			return CreateHook(oldMethod, newMethod);
		}

		/// <summary>
		/// Modifies the underlying method of <paramref name="old"/> to immediately jump to <paramref name="new"/>.
		/// </summary>
		/// <typeparam name="THooked">A delegate type matching the method to hook.</typeparam>
		/// <typeparam name="THook">A delegate type matching the method hooked into <paramref name="old"/>.</typeparam>
		/// <param name="old">An instance of <typeparamref name="THooked"/> pointing to the method to hook.</param>
		/// <param name="new">An instance of <typeparamref name="THook"/> pointing to the hook target.</param>
		public static HookHandle CreateHook<THooked, THook>(THooked old, THook @new) {
			if (typeof(Delegate).IsAssignableFrom(typeof(THooked)) == false)
				throw new InvalidOperationException("THooked must be a Delegate type.");
			if (typeof(Delegate).IsAssignableFrom(typeof(THook)) == false)
				throw new InvalidOperationException("THook must be a Delegate type.");
			var oldMethod = ((Delegate)(object)old).Method;
			var newMethod = ((Delegate)(object)@new).Method;

			if (oldMethod.ReturnType.IsAssignableFrom(newMethod.ReturnType))
				throw new ArgumentException("Return type mismatch: " + newMethod.ReturnType + " isn't assignable to " + oldMethod.ReturnType + ".");

			// TODO: Check if this works despite the apparent mismatch.
			// I think it's impossible due to unbalancing the stack or something along those lines,
			// but that may be not be the case.
			// For whatever reason typeof(object).IsAssignableFrom(typeof(void)) is true.
			if (oldMethod.ReturnType != typeof(void) && newMethod.ReturnType == typeof(void))
				throw new ArgumentException("Return type mismatch: Can't hook method returning void with one that returns something.");

			var oldParameters = oldMethod.GetParameters().Select(p => p.ParameterType).ToList();
			if (oldMethod.IsStatic == false)
				oldParameters.Insert(0, oldMethod.DeclaringType);

			var newParameters = newMethod.GetParameters().Select(p => p.ParameterType).ToList();
			if (newMethod.IsStatic == false)
				newParameters.Insert(0, newMethod.DeclaringType);

			//TODO: Check whether a method with fewer parameters can safely be hooked into one with more.
			if (oldParameters.Count != newParameters.Count)
				throw new ArgumentException("Parameter count (eventually including initial \"this\" parameter) mismatch: Tried to hook method with " +
					oldParameters.Count + " with one with " + newParameters.Count + " parameters.");
			for (int i = 0; i < oldParameters.Count; i++) {
				if (newParameters[i].IsAssignableFrom(oldParameters[i]) == false)
					throw new ArgumentException("Parameter type mismatch: Can't convert assign " +
						oldParameters[i] + " to " + newParameters[i] + " at position " + i +
						" (eventually including initial \"this\" parameter).");
			}

			return CreateHook(oldMethod, newMethod);
		}
	}
}