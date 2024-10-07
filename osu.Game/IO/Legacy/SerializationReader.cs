// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace osu.Game.IO.Legacy
{
    /// <summary> SerializationReader.  Extends BinaryReader to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationReader : BinaryReader
    {
        private readonly Stream stream;

        public SerializationReader(Stream s)
            : base(s, Encoding.UTF8)
        {
            stream = s;
        }

        public int RemainingBytes => (int)(stream.Length - stream.Position);

        /// <summary> Reads a string from the buffer.  Overrides the base implementation so it can cope with nulls. </summary>
        public override string ReadString()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            if (ReadByte() == 0) return null;

            return base.ReadString();
        }

        /// <summary> Reads a byte array from the buffer, handling nulls and the array length. </summary>
        public byte[] ReadByteArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadBytes(len);
            if (len < 0) return null;

            return Array.Empty<byte>();
        }

        /// <summary> Reads a char array from the buffer, handling nulls and the array length. </summary>
        public char[] ReadCharArray()
        {
            int len = ReadInt32();
            if (len > 0) return ReadChars(len);
            if (len < 0) return null;

            return Array.Empty<char>();
        }

        /// <summary> Reads a DateTime from the buffer. </summary>
        public DateTime ReadDateTime()
        {
            long ticks = ReadInt64();
            if (ticks < 0) throw new IOException("Bad ticks count read!");

            return new DateTime(ticks, DateTimeKind.Utc);
        }

        /// <summary> Reads a generic list from the buffer. </summary>
        public IList<T> ReadBList<T>(bool skipErrors = false) where T : ILegacySerializable, new()
        {
            int count = ReadInt32();
            if (count < 0) return null;

            IList<T> d = new List<T>(count);

            SerializationReader sr = new SerializationReader(BaseStream);

            for (int i = 0; i < count; i++)
            {
                T obj = new T();

                try
                {
                    obj.ReadFromStream(sr);
                }
                catch (Exception)
                {
                    if (skipErrors)
                        continue;

                    throw;
                }

                d.Add(obj);
            }

            return d;
        }

        /// <summary> Reads a generic list from the buffer. </summary>
        public IList<T> ReadList<T>()
        {
            int count = ReadInt32();
            if (count < 0) return null;

            IList<T> d = new List<T>(count);
            for (int i = 0; i < count; i++) d.Add((T)ReadObject());
            return d;
        }

        /// <summary> Reads a generic Dictionary from the buffer. </summary>
        public IDictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            int count = ReadInt32();
            if (count < 0) return null;

            IDictionary<TKey, TValue> d = new Dictionary<TKey, TValue>();
            for (int i = 0; i < count; i++) d[(TKey)ReadObject()] = (TValue)ReadObject();
            return d;
        }

        /// <summary> Reads an object which was added to the buffer by WriteObject. </summary>
        public object ReadObject()
        {
            ObjType t = (ObjType)ReadByte();

            switch (t)
            {
                case ObjType.BoolType:
                    return ReadBoolean();

                case ObjType.ByteType:
                    return ReadByte();

                case ObjType.UInt16Type:
                    return ReadUInt16();

                case ObjType.UInt32Type:
                    return ReadUInt32();

                case ObjType.UInt64Type:
                    return ReadUInt64();

                case ObjType.SByteType:
                    return ReadSByte();

                case ObjType.Int16Type:
                    return ReadInt16();

                case ObjType.Int32Type:
                    return ReadInt32();

                case ObjType.Int64Type:
                    return ReadInt64();

                case ObjType.CharType:
                    return ReadChar();

                case ObjType.StringType:
                    return base.ReadString();

                case ObjType.SingleType:
                    return ReadSingle();

                case ObjType.DoubleType:
                    return ReadDouble();

                case ObjType.DecimalType:
                    return ReadDecimal();

                case ObjType.DateTimeType:
                    return ReadDateTime();

                case ObjType.ByteArrayType:
                    return ReadByteArray();

                case ObjType.CharArrayType:
                    return ReadCharArray();

                case ObjType.OtherType:
                    throw new IOException("Deserialization of arbitrary type is not supported.");

                default:
                    return null;
            }
        }
    }

    public enum ObjType : byte
    {
        NullType,
        BoolType,
        ByteType,
        UInt16Type,
        UInt32Type,
        UInt64Type,
        SByteType,
        Int16Type,
        Int32Type,
        Int64Type,
        CharType,
        StringType,
        SingleType,
        DoubleType,
        DecimalType,
        DateTimeType,
        ByteArrayType,
        CharArrayType,
        OtherType,
        LegacySerializableType
    }
}
