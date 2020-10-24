// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

// ReSharper disable ConditionIsAlwaysTrueOrFalse (we're allowing nulls to be passed to the writer where the underlying class doesn't).
// ReSharper disable HeuristicUnreachableCode

namespace osu.Game.IO.Legacy
{
    /// <summary> SerializationWriter.  Extends BinaryWriter to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationWriter : BinaryWriter
    {
        public SerializationWriter(Stream s)
            : base(s, Encoding.UTF8)
        {
        }

        /// <summary> Static method to initialise the writer with a suitable MemoryStream. </summary>
        public static SerializationWriter GetWriter()
        {
            MemoryStream ms = new MemoryStream(1024);
            return new SerializationWriter(ms);
        }

        /// <summary> Writes a string to the buffer.  Overrides the base implementation so it can cope with nulls </summary>
        public override void Write(string str)
        {
            if (str == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {
                Write((byte)ObjType.stringType);
                base.Write(str);
            }
        }

        /// <summary> Writes a byte array to the buffer.  Overrides the base implementation to
        /// send the length of the array which is needed when it is retrieved </summary>
        public override void Write(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }

        /// <summary> Writes a char array to the buffer.  Overrides the base implementation to
        /// sends the length of the array which is needed when it is read. </summary>
        public override void Write(char[] c)
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                int len = c.Length;
                Write(len);
                if (len > 0) base.Write(c);
            }
        }

        /// <summary>
        /// Writes DateTime to the buffer.
        /// </summary>
        /// <param name="dt"></param>
        public void Write(DateTime dt)
        {
            Write(dt.ToUniversalTime().Ticks);
        }

        /// <summary> Writes a generic ICollection (such as an IList(T)) to the buffer.</summary>
        public void Write<T>(List<T> c) where T : ILegacySerializable
        {
            if (c == null)
            {
                Write(-1);
            }
            else
            {
                int count = c.Count;
                Write(count);
                for (int i = 0; i < count; i++)
                    c[i].WriteToStream(this);
            }
        }

        /// <summary> Writes a generic IDictionary to the buffer. </summary>
        public void Write<TKey, TValue>(IDictionary<TKey, TValue> d)
        {
            if (d == null)
            {
                Write(-1);
            }
            else
            {
                Write(d.Count);

                foreach (KeyValuePair<TKey, TValue> kvp in d)
                {
                    WriteObject(kvp.Key);
                    WriteObject(kvp.Value);
                }
            }
        }

        /// <summary> Writes an arbitrary object to the buffer.  Useful where we have something of type "object"
        /// and don't know how to treat it.  This works out the best method to use to write to the buffer. </summary>
        public void WriteObject(object obj)
        {
            if (obj == null)
            {
                Write((byte)ObjType.nullType);
            }
            else
            {
                switch (obj)
                {
                    case bool boolObj:
                        Write((byte)ObjType.boolType);
                        Write(boolObj);
                        break;

                    case byte byteObj:
                        Write((byte)ObjType.byteType);
                        Write(byteObj);
                        break;

                    case ushort ushortObj:
                        Write((byte)ObjType.uint16Type);
                        Write(ushortObj);
                        break;

                    case uint uintObj:
                        Write((byte)ObjType.uint32Type);
                        Write(uintObj);
                        break;

                    case ulong ulongObj:
                        Write((byte)ObjType.uint64Type);
                        Write(ulongObj);
                        break;

                    case sbyte sbyteObj:
                        Write((byte)ObjType.sbyteType);
                        Write(sbyteObj);
                        break;

                    case short shortObj:
                        Write((byte)ObjType.int16Type);
                        Write(shortObj);
                        break;

                    case int intObj:
                        Write((byte)ObjType.int32Type);
                        Write(intObj);
                        break;

                    case long longObj:
                        Write((byte)ObjType.int64Type);
                        Write(longObj);
                        break;

                    case char charObj:
                        Write((byte)ObjType.charType);
                        base.Write(charObj);
                        break;

                    case string stringObj:
                        Write((byte)ObjType.stringType);
                        base.Write(stringObj);
                        break;

                    case float floatObj:
                        Write((byte)ObjType.singleType);
                        Write(floatObj);
                        break;

                    case double doubleObj:
                        Write((byte)ObjType.doubleType);
                        Write(doubleObj);
                        break;

                    case decimal decimalObj:
                        Write((byte)ObjType.decimalType);
                        Write(decimalObj);
                        break;

                    case DateTime dateTimeObj:
                        Write((byte)ObjType.dateTimeType);
                        Write(dateTimeObj);
                        break;

                    case byte[] byteArray:
                        Write((byte)ObjType.byteArrayType);
                        base.Write(byteArray);
                        break;

                    case char[] charArray:
                        Write((byte)ObjType.charArrayType);
                        base.Write(charArray);
                        break;

                    default:
                        Write((byte)ObjType.otherType);
                        BinaryFormatter b = new BinaryFormatter
                        {
                            // AssemblyFormat = FormatterAssemblyStyle.Simple,
                            TypeFormat = FormatterTypeStyle.TypesWhenNeeded
                        };
                        b.Serialize(BaseStream, obj);
                        break;
                } // switch
            } // if obj==null
        } // WriteObject

        /// <summary> Adds the SerializationWriter buffer to the SerializationInfo at the end of GetObjectData(). </summary>
        public void AddToInfo(SerializationInfo info)
        {
            byte[] b = ((MemoryStream)BaseStream).ToArray();
            info.AddValue("X", b, typeof(byte[]));
        }

        public void WriteRawBytes(byte[] b)
        {
            base.Write(b);
        }

        public void WriteByteArray(byte[] b)
        {
            if (b == null)
            {
                Write(-1);
            }
            else
            {
                int len = b.Length;
                Write(len);
                if (len > 0) base.Write(b);
            }
        }

        public void WriteUtf8(string str)
        {
            WriteRawBytes(Encoding.UTF8.GetBytes(str));
        }
    }
}
