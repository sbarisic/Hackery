using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackery {
	class UObject : IDisposable {
		public UObject() {
			throw new Exception("UObjects can only be instantiated in unmanaged memory");
		}

		public virtual void ctor() {
		}

		public virtual void dtor() {
		}

		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			return GetHashCode() == obj.GetHashCode();
		}

		public override int GetHashCode() {
			return this.ToObjPointer().ToInt32();
		}

		public override string ToString() {
			return GetType().Name + " @ 0x" + this.ToObjPointer().ToInt32().ToString("X");
		}

		public void Dispose() {
			Magic.UnmanagedDelete(this);
		}

		public static bool operator ==(UObject A, UObject B) {
			return A.Equals(B);
		}

		public static bool operator !=(UObject A, UObject B) {
			return !A.Equals(B);
		}
	}
}