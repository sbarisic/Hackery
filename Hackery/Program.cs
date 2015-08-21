using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace Hackery {
	unsafe class UClass : UObject {
		public override void ctor() {

		}

		public override void dtor() {

		}
	}

	unsafe class Program {
		static void Main(string[] args) {
			Console.Title = "Hackery";

			using (UClass Wat = Magic.UnmanagedNew<UClass>()) {
				Console.WriteLine(Wat);
			}

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}