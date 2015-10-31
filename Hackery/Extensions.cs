using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Hackery {
	public static class Extensions {
		public static MethodInfo GetStaticMethod(this Type T, string Name) {
			return T.GetMethod(Name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
		}
	}
}