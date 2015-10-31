using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Hackery {
	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_NT_HEADERS {
		public short Signature;
		public IMAGE_FILE_HEADER FileHeader;
		public IMAGE_OPTIONAL_HEADER OptionalHeader;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_FILE_HEADER {
		public short Machine;
		public short NumberOfSections;
		public int TimeDateStamp;
		public int PointerToSymbolTable;
		public int NumberOfSymbols;
		public short SizeOfOptionalHeader;
		public short Characteristics;
	}

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct IMAGE_BASE {
		[FieldOffset(0)]
		public int BaseOfData;
		[FieldOffset(sizeof(int))]
		public int ImageBase32;
		[FieldOffset(0)]
		public long ImageBase64;
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct IMAGE_OPTIONAL_HEADER {
		public short Magic;
		public byte MajorLinkerVersion;
		public byte MinorLinkerVersion;
		public int SizeOfCode;
		public int SizeOfInitializedData;
		public int SizeOfUninitializedData;
		public int AddressOfEntryPoint;
		public int BaseOfCode;
		public IMAGE_BASE ImageBase;
		public int SectionAlignment;
		public int FileAlignment;
		public short MajorOperatingSystemVersion;
		public short MinorOperatingSystemVersion;
		public short MajorImageVersion;
		public short MinorImageVersion;
		public short MajorSubsystemVersion;
		public short MinorSubsystemVersion;
		public int Win32VersionValue;
		public int SizeOfImage;
		public int SizeOfHeaders;
		public int CheckSum;
		public short Subsystem;
		public short DllCharacteristics;
		public IntPtr SizeOfStackReserve;
		public IntPtr SizeOfStackCommit;
		public IntPtr SizeOfHeapReserve;
		public IntPtr SizeOfHeapCommit;
		public int LoaderFlags;
		public int NumberOfRvaAndSizes;
		public IMAGE_DATA_DIRECTORY ExportTable;
		public IMAGE_DATA_DIRECTORY ImportTable;
		public IMAGE_DATA_DIRECTORY ResourceTable;
		public IMAGE_DATA_DIRECTORY ExceptionTable;
		public IMAGE_DATA_DIRECTORY CertificateTable;
		public IMAGE_DATA_DIRECTORY BaseRelocationTable;
		public IMAGE_DATA_DIRECTORY Debug;
		public IMAGE_DATA_DIRECTORY Arch;
		public IMAGE_DATA_DIRECTORY GlobalPtr;
		public IMAGE_DATA_DIRECTORY TLSTable;
		public IMAGE_DATA_DIRECTORY LoadConfigTable;
		public IMAGE_DATA_DIRECTORY BoundImport;
		public IMAGE_DATA_DIRECTORY IAT;
		public IMAGE_DATA_DIRECTORY DelayImportDescriptor;
		public IMAGE_DATA_DIRECTORY CLRRuntimeHeader;
		public IMAGE_DATA_DIRECTORY Reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_DATA_DIRECTORY {
		public int VirtualAddress;
		public int Size;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct IMAGE_EXPORT_DIRECTORY {
		public int Characteristics;
		public int TimeDateStamp;
		public short MajorVersion;
		public short MinorVersion;
		public int Name;
		public int Base;
		public int NumberOfFunctions;
		public int NumberOfNames;
		public int AddressOfFunctions;     // RVA from base of image
		public int AddressOfNames;     // RVA from base of image
		public int AddressOfNameOrdinals;  // RVA from base of image
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct IMAGE_DOS_HEADER {
		public ushort Magic;
		public ushort CBLP;
		public ushort CP;
		public ushort CRLc;
		public ushort CPARHdr;
		public ushort MinAlloc;
		public ushort MaxAlloc;
		public ushort SS;
		public ushort SP;
		public ushort CSum;
		public ushort IP;
		public ushort CS;
		public ushort LFarLc;
		public ushort OVNO;
		public fixed ushort Res[4];
		public ushort OEMId;
		public ushort OEMInfo;
		public fixed ushort Res2[10];
		public uint LFaNew;
	}
}