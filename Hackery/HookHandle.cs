using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Hackery
{
	/// <summary>
	/// <para>A handle to safely temporarily hook a method.</para>
	/// <para>The hook is reset when the handle is disposed.</para>
	/// </summary>
	public class HookHandle : IDisposable
	{
		/// <summary>
		/// The hooked method.
		/// </summary>
		public MethodInfo HookedMethod { get; }
		readonly byte[] _originalIntro;

		/// <summary>
		/// Creates a new <see cref="HookHandle"/> instance with the given parameters.
		/// </summary>
		/// <param name="hookedMethod">The hooked method.</param>
		/// <param name="originalIntro">The original intro of the hooked method.</param>
		public HookHandle(MethodInfo hookedMethod, byte[] originalIntro)
		{
			HookedMethod = hookedMethod;
			_originalIntro = originalIntro;
		}

		/// <summary>
		/// Restores <see cref="HookedMethod"/> to its previous state.
		/// </summary>
		public void Dispose()
		{
			var functionPointer = HookedMethod.MethodHandle.GetFunctionPointer();
			using (MemoryManagement.Protect(functionPointer, (uint)_originalIntro.Length, MemProtection.ExecReadWrite))
			{ Marshal.Copy(_originalIntro, 0, functionPointer, _originalIntro.Length); }
		}
	}
}