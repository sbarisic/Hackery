using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Test {
	class Program {
		static string[] GetModules() {
			List<string> Modules = new List<string>();
			Process Cur = Process.GetCurrentProcess();
			foreach (ProcessModule M in Cur.Modules)
				Modules.Add(Path.GetFileName(M.FileName));
			return Modules.ToArray();
		}

		static void Main(string[] args) {
			Console.Title = "Test";
			Console.WriteLine("PID: {0}", Process.GetCurrentProcess().Id);

			Console.ReadLine();
			string[] Modules = GetModules();
			for (int i = 0; i < Modules.Length; i++)
				Console.WriteLine(Modules[i]);
			Console.WriteLine();

			if (Modules.Contains("Inkjet.dll"))
				Console.WriteLine("Inkjet detected!");

			Console.WriteLine("Done!");
			while (true)
				;
		}
	}
}