// Decompiled with JetBrains decompiler
// Type: FM.LiveSwitch.Unity.FastColor32Array
// Assembly: FM.LiveSwitch.Unity, Version=1.12.3.46193, Culture=neutral, PublicKeyToken=null
// MVID: 2583CD92-3F1E-4E7C-87AE-B423E8600AFD
// Assembly location: C:\Cavrnus\cavrnus-client-2\src\Unity\VoiceTest\Assets\LiveSwitch\FM.LiveSwitch.Unity.dll

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Cavrnus.RTC
{

	internal static class FastColor32Array
	{
		public static readonly bool TypeSwapNotSupported;
		private static readonly UIntPtr ByteArrayType;
		private static readonly UIntPtr Color32ArrayType;

		static unsafe FastColor32Array()
		{
			fixed (byte* numPtr = new byte[1])
				FastColor32Array.ByteArrayType = FastColor32Array.GetHeader((void*)numPtr)->Type;
			fixed (Color32* color32Ptr = new Color32[1])
				FastColor32Array.Color32ArrayType = FastColor32Array.GetHeader((void*)color32Ptr)->Type;
			FastColor32Array.TypeSwapNotSupported = FastColor32Array.Color32ArrayType == FastColor32Array.ByteArrayType;
		}

		public static bool AsByteArray(this Color32[] colors, Action<byte[]> action)
		{
			if (colors.HandleNullOrEmptyArray<Color32, byte>(action))
				return false;
			FastColor32Array.Union union = new FastColor32Array.Union()
			{
				Colors = colors
			};
			union.Colors.ToByteArray();
			try
			{
				action(union.Bytes);
			}
			finally
			{
				union.Bytes.ToColorArray();
			}
			return true;
		}

		public static bool AsColorArray(this byte[] bytes, Action<Color32[]> action)
		{
			if (bytes.HandleNullOrEmptyArray<byte, Color32>(action))
				return false;
			FastColor32Array.Union union = new FastColor32Array.Union()
			{
				Bytes = bytes
			};
			union.Bytes.ToColorArray();
			try
			{
				action(union.Colors);
			}
			finally
			{
				union.Colors.ToByteArray();
			}
			return true;
		}

		public static bool HandleNullOrEmptyArray<TSource, TDestination>(
		  this TSource[] array,
		  Action<TDestination[]> action)
		{
			if (array == null)
			{
				action((TDestination[])null);
				return true;
			}
			if (array.Length != 0)
				return false;
			action(new TDestination[0]);
			return true;
		}

		private static unsafe FastColor32Array.ArrayHeader* GetHeader(void* pBytes) => (FastColor32Array.ArrayHeader*)pBytes - 1;

		private static unsafe void ToColorArray(this byte[] bytes)
		{
			fixed (byte* numPtr = bytes)
			{
				FastColor32Array.ArrayHeader* header = FastColor32Array.GetHeader((void*)numPtr);
				UIntPtr color32ArrayType = FastColor32Array.Color32ArrayType;
				header->Type = color32ArrayType;
				UIntPtr num = (UIntPtr)(ulong)(bytes.Length / sizeof(Color32));
				header->Length = num;
			}
		}

		private static unsafe void ToByteArray(this Color32[] colors)
		{
			fixed (Color32* color32Ptr = colors)
			{
				FastColor32Array.ArrayHeader* header = FastColor32Array.GetHeader((void*)color32Ptr);
				UIntPtr byteArrayType = FastColor32Array.ByteArrayType;
				header->Type = byteArrayType;
				UIntPtr num = (UIntPtr)(ulong)(colors.Length * sizeof(Color32));
				header->Length = num;
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct Union
		{
			[FieldOffset(0)]
			public byte[] Bytes;
			[FieldOffset(0)]
			public Color32[] Colors;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct ArrayHeader
		{
			public UIntPtr Type;
			public UIntPtr Length;
		}
	}

}