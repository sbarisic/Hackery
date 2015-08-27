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
	delegate bool Win32Callback(IntPtr Wnd, IntPtr Param);
	static class NPPGameOfLife {
		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		static extern bool AllocConsole();

		[DllImport("kernel32", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
		static extern bool FreeConsole();

		[DllImport("user32", SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr Parent, IntPtr Child, string Class, string Window);

		[DllImport("user32")]
		static extern bool EnumChildWindows(IntPtr Parent, Win32Callback Callback, IntPtr Param);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern int GetClassName(IntPtr Wnd, StringBuilder ClassName, int MaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern int SendMessage(IntPtr Wnd, uint Msg, IntPtr L, IntPtr W);

		static string GetClassName(IntPtr Wind) {
			StringBuilder SB = new StringBuilder();
			SB.Clear();
			GetClassName(Wind, SB, 64);
			return SB.ToString();
		}

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetWindowText(IntPtr Wnd, string Txt);

		static void ForEachControl(IntPtr Parent, Func<IntPtr, string, bool> F) {
			StringBuilder SB = new StringBuilder();
			EnumChildWindows(Parent, (W, P) => {
				SB.Clear();
				GetClassName(W, SB, 64);
				return F(W, SB.ToString());
			}, IntPtr.Zero);
		}

		static IntPtr[] GetChildControls(IntPtr Parent) {
			List<IntPtr> Controls = new List<IntPtr>();
			ForEachControl(Parent, (P, N) => {
				Controls.Add(P);
				return true;
			});
			return Controls.ToArray();
		}

		static IntPtr FindControl(IntPtr Parent, string Name) {
			IntPtr Ret = IntPtr.Zero;
			ForEachControl(Parent, (P, N) => {
				if (N == Name) {
					Ret = P;
					return false;
				}
				return true;
			});
			return Ret;
		}

		static void Sci_AddText(IntPtr Control, string Txt) {
			IntPtr StrP = Marshal.StringToHGlobalAnsi(Txt);
			SendMessage(Control, 2001, new IntPtr(Txt.Length), StrP);
			Marshal.FreeHGlobal(StrP);
		}

		static void Sci_ClearText(IntPtr Control) {
			SendMessage(Control, 2004, IntPtr.Zero, IntPtr.Zero);
		}

		static int[,] Field;

		static int Get(int X, int Y) {
			if (X < 0 || Y < 0 || X >= Field.GetLength(0) || Y >= Field.GetLength(1))
				return 0;
			return Field[X, Y];
		}

		static int CountNeighbours(int X, int Y) {
			int N = 0;
			if (Get(X - 1, Y - 1) != 0)
				N++;
			if (Get(X, Y - 1) != 0)
				N++;
			if (Get(X + 1, Y - 1) != 0)
				N++;
			if (Get(X - 1, Y) != 0)
				N++;
			if (Get(X + 1, Y) != 0)
				N++;
			if (Get(X - 1, Y + 1) != 0)
				N++;
			if (Get(X, Y + 1) != 0)
				N++;
			if (Get(X + 1, Y + 1) != 0)
				N++;
			return N;
		}

		static void Step() {
			int[,] Temp = new int[Field.GetLength(0), Field.GetLength(1)];
			for (int y = 0; y < Field.GetLength(1); y++)
				for (int x = 0; x < Field.GetLength(0); x++) {
					int N = CountNeighbours(x, y);
					Temp[x, y] = Get(x, y);

					if (N < 2 || N > 3)
						Temp[x, y] = 0;
					if (N == 2 && Temp[x, y] != 0)
						Temp[x, y]++;
					if (N == 3)
						Temp[x, y]++;
				}
			Field = Temp;
		}

		static StringBuilder SB = new StringBuilder();
		static string ToString() {
			SB.Clear();
			for (int y = 0; y < Field.GetLength(1); y++) {
				for (int x = 0; x < Field.GetLength(0); x++) {
					if (Field[x, y] != 0)
						SB.Append("[]");
					else
						SB.Append("  ");
				}
				SB.AppendLine("|");
			}
			SB.AppendLine(new string('-', Field.GetLength(0) * 2));
			return SB.ToString();
		}

		public static void Main() {
			AllocConsole();

			IntPtr Wind = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Notepad++", null);
			IntPtr Editor = FindControl(Wind, "Scintilla");

			Field = new int[40, 10];
			for (int i = 0; i < 4; i++) 
				Field[2 + i, 1] = 1;
			Field[1, 2] = Field[5, 2] = Field[5, 3] = Field[1, 4] = Field[4, 4] = 1;

			int Gen = 0;
			while (true) {
				Console.Clear();
				Console.WriteLine("Generation: {0}", Gen++);
				Sci_ClearText(Editor);
				Sci_AddText(Editor, ToString());
				Console.ReadLine();
				Step();
			}

			FreeConsole();
		}
	}
}