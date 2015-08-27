using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using RGiesecke.DllExport;

namespace Inkjet {
	public class Magic {
		[DllExport]
		public static void Init() {
			Marshal.PrelinkAll(typeof(Magic));
			Marshal.PrelinkAll(typeof(NPPGameOfLife));
			NPPGameOfLife.Main();
		}
	}
}