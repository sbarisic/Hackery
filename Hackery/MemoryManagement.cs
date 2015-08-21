using System;

namespace Hackery
{
	/// <summary>
	/// Provides memory management convenience functions.
	/// </summary>
	static class MemoryManagement
	{
		/// <summary>
		/// <para>A handle to safely temporarily change memory protection.</para>
		/// <para>The protection is reset when the handle is disposed.</para>
		/// </summary>
		public class VirtualProtectHandle : IDisposable
		{
			/// <summary>
			/// The address of the first byte that had its protection changed.
			/// </summary>
			public IntPtr Address { get; }

			/// <summary>
			/// The size of the memory range with changed protection.
			/// </summary>
			public uint Size { get; }

			/// <summary>
			/// The previous protection setting.
			/// </summary>
			public MemProtection OldProtection { get; }

			/// <summary>
			/// Creates a new <see cref="VirtualProtectHandle"/> instance with the given parameters.
			/// </summary>
			/// <param name="address">The address of the first byte that had its protection changed.</param>
			/// <param name="size">The size of the memory range with changed protection.</param>
			/// <param name="oldProtection">The previous protection setting.</param>
			public VirtualProtectHandle(IntPtr address, uint size, MemProtection oldProtection)
			{
				Address = address;
				Size = size;
				OldProtection = oldProtection;
			}

			/// <summary>
			/// Resets the memory protection to its previous value.
			/// </summary>
			public void Dispose()
			{ Kernel32.VirtualProtect(Address, Size, OldProtection); }
		}

		public static VirtualProtectHandle Protect(IntPtr address, uint size, MemProtection newProtection)
		{
			MemProtection oldProtection;
			Kernel32.VirtualProtect(address, size, newProtection, out oldProtection);
			return new VirtualProtectHandle(address, size, oldProtection);
		}
	}
}
