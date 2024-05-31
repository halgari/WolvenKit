using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using WolvenKit.Core.Attributes;
using WolvenKit.Core.CRC;

namespace WolvenKit.Core.Extensions
{
    public static class StreamExtensions
    {
        //https://stackoverflow.com/a/13022108
        public static void CopyToWithLength(this Stream input, Stream output, int bytes)
        {
            var buffer = new byte[4096];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }

        public static byte[] ToByteArray(this Stream input, bool keepPosition = false)
        {
            if (input is MemoryStream memoryStream)
            {
                return memoryStream.ToArray();
            }
            else
            {
                using var ms = new MemoryStream();
                if (!keepPosition)
                {
                    input.Position = 0;
                }
                input.CopyTo(ms);
                return ms.ToArray();

            }
        }

        public static uint PeekFourCC(this Stream m_stream)
        {
            var startPos = m_stream.Position;
            var fourcc = m_stream.ReadStruct<uint>();
            m_stream.Position = startPos;

            return fourcc;
        }

        public static T ReadStruct<T>(this Stream m_stream, Crc32Algorithm? crc32 = null) where T : struct
        {
            var size = Marshal.SizeOf<T>();

            var m_temp = new byte[size];
            m_stream.Read(m_temp, 0, size);

            var handle = GCHandle.Alloc(m_temp, GCHandleType.Pinned);
            var item = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());

            crc32?.Append(m_temp);

            handle.Free();

            return item;
        }

        public static void WriteStruct<T>(this Stream m_stream, T value, Crc32Algorithm? crc32 = null) where T : struct
        {
            var m_temp = new byte[Marshal.SizeOf<T>()];
            var handle = GCHandle.Alloc(m_temp, GCHandleType.Pinned);

            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), true);
            m_stream.Write(m_temp, 0, m_temp.Length);

            crc32?.Append(m_temp);

            handle.Free();
        }

        public static T[] ReadStructs<T>(this Stream m_stream, uint count, Crc32Algorithm? crc32 = null) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var items = new T[count];

            var m_temp = new byte[size];
            for (uint i = 0; i < count; i++)
            {
                m_stream.Read(m_temp, 0, size);

                var handle = GCHandle.Alloc(m_temp, GCHandleType.Pinned);
                items[i] = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());

                crc32?.Append(m_temp);

                handle.Free();
            }

            return items;
        }

        public static void WriteStructs<T>(this Stream m_stream, T[] array, Crc32Algorithm? crc32 = null) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var m_temp = new byte[size];
            for (var i = 0; i < array.Length; i++)
            {
                var handle = GCHandle.Alloc(m_temp, GCHandleType.Pinned);

                Marshal.StructureToPtr(array[i], handle.AddrOfPinnedObject(), true);
                m_stream.Write(m_temp, 0, m_temp.Length);

                crc32?.Append(m_temp);

                handle.Free();
            }
        }

        #region BitField

        public static T ReadBitfield<T>(this Stream stream) where T : struct
        {
            var fields = GetFieldInfos<T>();

            byte byteReadJustNow = 0;
            byte bitsLeft = 0;

            object result = default(T);

            foreach (var (fieldInfo, length) in fields)
            {
                ulong tmp = 0;

                for (byte i = 0; i < length; i++)
                {
                    if (bitsLeft == 0)
                    {
                        var r = stream.ReadByte();
                        if (r == -1)
                        {
                            throw new EndOfStreamException();
                        }

                        byteReadJustNow = (byte)r;
                        bitsLeft = 8;
                    }

                    tmp |= (ulong)((byteReadJustNow >> (8 - bitsLeft--)) & 1) << i;
                }

                object tmp2;
                switch (fieldInfo.FieldType.Name)
                {
                    case "Byte":
                        tmp2 = Convert.ToByte(tmp);
                        break;

                    case "UInt16":
                        tmp2 = Convert.ToUInt16(tmp);
                        break;

                    case "UInt32":
                        tmp2 = Convert.ToUInt32(tmp);
                        break;

                    case "UInt64":
                        tmp2 = Convert.ToUInt64(tmp);
                        break;

                    default:
                        throw new Exception();
                }

                fieldInfo.SetValue(result, tmp2);
            }

            return (T)result;
        }

        public static void WriteBitfield<T>(this Stream stream, T data) where T : struct
        {
            var fields = GetFieldInfos<T>();

            ulong byteToWrite = 0;
            byte bitsUsed = 0;

            foreach (var (fieldInfo, length) in fields)
            {
                var val = Convert.ToUInt64(fieldInfo.GetValue(data));
                // TODO: Check if numberOfBits(val) <= length, currently every bit above gets just cut off

                for (byte i = 0; i < length; i++)
                {
                    if (bitsUsed == 8)
                    {
                        stream.WriteByte((byte)byteToWrite);

                        byteToWrite = 0;
                        bitsUsed = 0;
                    }

                    byteToWrite |= ((val >> i) & 1) << bitsUsed++;
                }
            }

            if (bitsUsed != 8)
            {
                throw new Exception();
            }

            stream.WriteByte((byte)byteToWrite);
        }

        private static Dictionary<FieldInfo, uint> GetFieldInfos<T>()
        {
            var fields = new Dictionary<FieldInfo, uint>();

            foreach (var fieldInfo in typeof(T).GetFields())
            {
                if (fieldInfo.GetCustomAttribute<BitfieldLength>() is not { Length: var length })
                {
                    throw new Exception();
                }

                if (length is <= 0 or > 64)
                {
                    throw new Exception();
                }

                var sizeOfType = Marshal.SizeOf(fieldInfo.FieldType) * 8;
                if (sizeOfType < length)
                {
                    throw new Exception();
                }

                fields.Add(fieldInfo, length);
            }

            if (fields.Values.Sum(x => x) % 8 != 0)
            {
                throw new Exception();
            }

            return fields;
        }

        #endregion BitField
    }
}
