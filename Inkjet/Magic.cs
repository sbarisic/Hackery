using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RGiesecke.DllExport;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace Inkjet {
	public class Magic {
		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr Parent, IntPtr Child, string Class, string Window);

		[DllExport("Init", CallingConvention.Cdecl)]
		public static void Init() {
			Process Cur = Process.GetCurrentProcess();
			MessageBox.Show("Magic!", string.Format("Inkjet - {0} ({1})", Cur.ProcessName, Cur.Id));
		}
	}
}