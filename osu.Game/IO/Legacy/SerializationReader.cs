// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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

        /// <summary> Static method to take a SerializationInfo object (an input to an ISerializable constructor)
        /// and produce a SerializationReader from which serialized objects can be read </summary>.
        public static SerializationReader GetReader(SerializationInfo info)
        {
            byte[] byteArray = (byte[])info.GetValue("X", typeof(byte[]));
            MemoryStream ms = new MemoryStream(byteArray);
            return new SerializationReader(ms);
        }

        /// <summary> Reads a string from the buffer.  Overrides the base implementation so it can cope with nulls. </summary>
        public override string ReadString()
        {
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
        public IDictionary<T, U> ReadDictionary<T, U>()
        {
            int count = ReadInt32();
            if (count < 0) return null;

            IDictionary<T, U> d = new Dictionary<T, U>();
            for (int i = 0; i < count; i++) d[(T)ReadObject()] = (U)ReadObject();
            return d;
        }

        /// <summary> Reads an object which was added to the buffer by WriteObject. </summary>
        public object ReadObject()
        {
            ObjType t = (ObjType)ReadByte();

            switch (t)
            {
                case ObjType.boolType:
                    return ReadBoolean();

                case ObjType.byteType:
                    return ReadByte();

                case ObjType.uint16Type:
                    return ReadUInt16();

                case ObjType.uint32Type:
                    return ReadUInt32();

                case ObjType.uint64Type:
                    return ReadUInt64();

                case ObjType.sbyteType:
                    return ReadSByte();

                case ObjType.int16Type:
                    return ReadInt16();

                case ObjType.int32Type:
                    return ReadInt32();

                case ObjType.int64Type:
                    return ReadInt64();

                case ObjType.charType:
                    return ReadChar();

                case ObjType.stringType:
                    return base.ReadString();

                case ObjType.singleType:
                    return ReadSingle();

                case ObjType.doubleType:
                    return ReadDouble();

                case ObjType.decimalType:
                    return ReadDecimal();

                case ObjType.dateTimeType:
                    return ReadDateTime();

                case ObjType.byteArrayType:
                    return ReadByteArray();

                case ObjType.charArrayType:
                    return ReadCharArray();

                case ObjType.otherType:
                    return DynamicDeserializer.Deserialize(BaseStream);

                default:
                    return null;
            }
        }

        public class DynamicDeserializer
        {
            private static VersionConfigToNamespaceAssemblyObjectBinder versionBinder;
            private static BinaryFormatter formatter;

            private static void initialize()
            {
                versionBinder = new VersionConfigToNamespaceAssemblyObjectBinder();
                formatter = new BinaryFormatter
                {
                    // AssemblyFormat = FormatterAssemblyStyle.Simple,
                    Binder = versionBinder
                };
            }

            public static object Deserialize(Stream stream)
            {
                if (formatter == null)
                    initialize();

                Debug.Assert(formatter != null, "formatter != null");

                // ReSharper disable once PossibleNullReferenceException
                return formatter.Deserialize(stream);
            }

            #region Nested type: VersionConfigToNamespaceAssemblyObjectBinder

            public sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
            {
                private readonly Dictionary<string, Type> cache = new Dictionary<string, Type>();

                public override Type BindToType(string assemblyName, string typeName)
                {
                    Type typeToDeserialize;

                    if (cache.TryGetValue(assemblyName + typeName, out typeToDeserialize))
                        return typeToDeserialize;

                    List<Type> tmpTypes = new List<Type>();
                    Type genType = null;

                    if (typeName.Contains("System.Collections.Generic") && typeName.Contains("[["))
                    {
                        string[] splitTyps = typeName.Split('[');

                        foreach (string typ in splitTyps)
                        {
                            if (typ.Contains("Version"))
                            {
                                string asmTmp = typ.Substring(typ.IndexOf(',') + 1);
                                string asmName = asmTmp.Remove(asmTmp.IndexOf(']')).Trim();
                                string typName = typ.Remove(typ.IndexOf(','));
                                tmpTypes.Add(BindToType(asmName, typName));
                            }
                            else if (typ.Contains("Generic"))
                            {
                                genType = BindToType(assemblyName, typ);
                            }
                        }

                        if (genType != null && tmpTypes.Count > 0)
                        {
                            return genType.MakeGenericType(tmpTypes.ToArray());
                        }
                    }

                    string toAssemblyName = assemblyName.Split(',')[0];
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                    foreach (Assembly a in assemblies)
                    {
                        if (a.FullName.Split(',')[0] == toAssemblyName)
                        {
                            typeToDeserialize = a.GetType(typeName);
                            break;
                        }
                    }

                    cache.Add(assemblyName + typeName, typeToDeserialize);

                    return typeToDeserialize;
                }
            }

            #endregion
        }
    }

    public enum ObjType : byte
    {
        nullType,
        boolType,
        byteType,
        uint16Type,
        uint32Type,
        uint64Type,
        sbyteType,
        int16Type,
        int32Type,
        int64Type,
        charType,
        stringType,
        singleType,
        doubleType,
        decimalType,
        dateTimeType,
        byteArrayType,
        charArrayType,
        otherType,
        ILegacySerializableType
    }
}
