using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackery {
	static class ObjectPointer {
		static ObjectPointer() {
			Magic.Hook(typeof(ObjectPointer).GetStaticMethod("_ToPointer"), typeof(ObjectPointer).GetStaticMethod("_ReturnObject"));
			Magic.Hook(typeof(ObjectPointer).GetStaticMethod("_ToObject"), typeof(ObjectPointer).GetStaticMethod("_ReturnUInt"));
		}

		static object _ReturnObject(object Obj) {
			return Obj;
		}

		static uint _ReturnUInt(uint UInt) {
			return UInt;
		}

		static uint _ToPointer(object Obj) {
			return 0;
		}

		static object _ToObject(uint UInt) {
			return null;
		}

		public static IntPtr ToObjPointer(this object Obj) {
			return new IntPtr(_ToPointer(Obj));
		}

		public static object ToObject(this IntPtr Ptr) {
			return _ToObject((uint)Ptr.ToInt32());
		}
	}
}