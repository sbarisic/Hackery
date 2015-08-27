using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace Test {
	class Program {
		static void Main(string[] args) {
			Console.Title = "Test";
			Console.WriteLine("PID: {0}", Process.GetCurrentProcess().Id);

			Console.ReadLine();
			Process Cur = Process.GetCurrentProcess();
			foreach (ProcessModule M in Cur.Modules)
				Console.WriteLine(Path.GetFileName(M.FileName));

			Console.WriteLine("\nDone!");
			while (true)
				;
		}
	}
}