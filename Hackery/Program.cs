using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace Hackery {
	class UClass : UObject {
	}

	unsafe class Program {
		static void Main(string[] args) {
			Console.Title = "Hackery";

			string Str = "Hello Universe!";

			// Immutable strings? Pfft.
			char* StrPtr = (char*)Str.ToObjPointer().ToPointer();
			char* World = (char*)Marshal.StringToHGlobalAuto("World!").ToPointer();
			for (int i = 0; i < 6; i++)
				StrPtr[10 + i] = World[i];

			Console.WriteLine(Str);



			Console.WriteLine("Done!");
			Debugger.Break();
			Console.ReadLine();
		}
	}
}