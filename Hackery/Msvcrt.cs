using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Hackery {
	static class Msvcrt {
		[DllImport("msvcrt", SetLastError = true)]
		public static extern IntPtr freopen(string Filename, string Mode, IntPtr Stream);
	}
}