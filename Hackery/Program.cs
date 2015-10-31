using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq.Expressions;
using System.ComponentModel;
using System.IO;

namespace Hackery {
	unsafe class Program {
		static int ReadInt(string Prompt) {
			Console.Write(Prompt);
			return int.Parse(Console.ReadLine());
		}

		unsafe static void Main(string[] args) {
			Console.Title = "Hackery";

			while (true) {
				Process P = Process.GetProcessById(ReadInt("Enter PID: "));
				Console.WriteLine("Injecting into {0}", P.Id);

				Magic.Inject(P.Id, "Inkjet.dll", "Init", false, true);
				Console.WriteLine("Done!");
			}

			Console.WriteLine("Done!");
			Console.ReadLine();
			Environment.Exit(0);
		}
	}
}