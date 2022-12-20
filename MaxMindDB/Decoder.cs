using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace MaxMind.MaxMindDb
{
    /// <summary>
    /// Enumeration representing the types of objects read from the database
    /// </summary>
    internal enum ObjectType
    {
        Extended, Pointer, Utf8String, Double, Bytes, Uint16, Uint32, Map, Int32, Uint64, Uint128, Array, Container, EndMarker, Boolean, Float
    }

    /// <summary>
    /// A data structure to store an object read from the database
    /// </summary>
    internal class Result
    {
        /// <summary>
        /// The object read from the database
        /// </summary>
        internal JToken Node { get; set; }

        /// <summary>
        /// The offset
        /// </summary>
        internal int Offset { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="offset">The offset.</param>
        internal Result(JToken node, int offset)
        {
            Node = node;
            Offset = offset;
        }
    }

    /// <summary>
    /// Given a stream, this class decodes the object graph at a particular location
    /// </summary>
    internal class Decoder
    {
        private byte[] fs = null;
        private long pointerBase = -1;
        private int[] pointerValueOffset = { 0, 0, 1 << 11, (1 << 19) + ((1) << 11), 0 };

        /// <summary>
        /// Initializes a new instance of the <see cref="Decoder"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="pointerBase">The base address in the stream.</param>
        internal Decoder(byte[] stream, long pointerBase)
        {
            this.pointerBase = pointerBase;
            fs = stream;
        }

        /// <summary>
        /// Decodes the object at the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>An object containing the data read from the stream</returns>
        internal Result Decode(int offset)
        {
            if (offset >= fs.Length)
                throw new InvalidDatabaseException("The MaxMind DB file's data section contains bad data: "
                                                    + "pointer larger than the database.");

            byte ctrlByte = fs[offset++];

            ObjectType type = FromControlByte(ctrlByte);

            if (type == ObjectType.Pointer) {
                Result pointer = DecodePointer(ctrlByte, offset);
                Result result = Decode(pointer.Node.Value<int>());
                result.Offset = pointer.Offset;
                return result;
            }

            if (type == ObjectType.Extended) {
                int nextByte = fs[offset++];
                int typeNum = nextByte + 7;
                if (typeNum < 8)
                    throw new InvalidDatabaseException(
                            "Something went horribly wrong in the decoder. An extended type "
                                    + "resolved to a type number < 8 (" + typeNum
                                    + ")");
                type = (ObjectType)typeNum;
            }

            int[] sizeArray = SizeFromCtrlByte(ctrlByte, offset);
            int size = sizeArray[0];
            offset = sizeArray[1];

            return DecodeByType(type, offset, size);
        }

        private byte[] ReadMany(int position, int size)
        {
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(fs, position, buffer, 0, size);
            return buffer;
        }

        private Result DecodeByType(ObjectType type, int offset, int size)
        {
            int newOffset = offset + size;

            switch (type) {
                case ObjectType.Map:
                    return DecodeMap(size, offset);
                case ObjectType.Array:
                    return DecodeArray(size, offset);
                case ObjectType.Boolean:
                    return new Result(DecodeBoolean(size), offset);
                case ObjectType.Utf8String:
                    return new Result(new JValue(Encoding.UTF8.GetString(fs, offset, size)), newOffset);
                case ObjectType.Double:
                    return new Result(DecodeDouble(ReadMany(offset, size)), newOffset);
                case ObjectType.Float:
                    return new Result(DecodeFloat(ReadMany(offset, size)), newOffset);
                case ObjectType.Bytes:
                    return new Result(new JValue(ReadMany(offset, size)), newOffset);
                case ObjectType.Uint16:
                    return new Result(DecodeInteger(ReadMany(offset, size)), newOffset);
                case ObjectType.Uint32:
                    return new Result(DecodeLong(ReadMany(offset, size)), newOffset);
                case ObjectType.Int32:
                    return new Result(DecodeInteger(ReadMany(offset, size)), newOffset);
                case ObjectType.Uint64:
                    return new Result(DecodeUint64(ReadMany(offset, size)), newOffset);
                case ObjectType.Uint128:
                    return new Result(JToken.FromObject(new BigInteger(fs, offset, size)), newOffset);
                default:
                    throw new InvalidDatabaseException("Unable to handle type:" + type);
            }
        }

        private ObjectType FromControlByte(byte b)
        {
            int p = b >> 5;
            return (ObjectType)p;
        }
        private int[] SizeFromCtrlByte(byte ctrlByte, int offset)
        {
            int size = ctrlByte & 0x1f;
            int bytesToRead = size < 29 ? 0 : size - 28;

            if (size == 29) {
                byte[] buffer = ReadMany(offset, bytesToRead);
                int i = Decoder.DecodeInteger(buffer);
                size = 29 + i;
            } else if (size == 30) {
                byte[] buffer = ReadMany(offset, bytesToRead);
                int i = Decoder.DecodeInteger(buffer);
                size = 285 + i;
            } else if (size > 30) {
                byte[] buffer = ReadMany(offset, bytesToRead);
                int i = Decoder.DecodeInteger(buffer) & (0x0FFFFFFF >> (32 - (8 * bytesToRead)));
                size = 65821 + i;
            }

            return new int[] { size, offset + bytesToRead };
        }

        private JValue DecodeBoolean(int size)
        {
            switch (size) {
                case 0:
                    return new JValue(false);
                case 1:
                    return new JValue(true);
                default:
                    throw new InvalidDatabaseException("The MaxMind DB file's data section contains bad data: "
                                                        + "invalid size of boolean.");
            }
        }
        private JValue DecodeDouble(byte[] buffer)
        {
            if (buffer.Length != 8)
                throw new InvalidDatabaseException("The MaxMind DB file's data section contains bad data: "
                                                   + "invalid size of double.");

            Array.Reverse(buffer);
            return new JValue(BitConverter.ToDouble(buffer, 0));
        }
        private JValue DecodeFloat(byte[] buffer)
        {
            if (buffer.Length != 4)
                throw new InvalidDatabaseException("The MaxMind DB file's data section contains bad data: "
                                                    + "invalid size of float.");
            Array.Reverse(buffer);
            return new JValue(BitConverter.ToSingle(buffer, 0));
        }
        private Result DecodeMap(int size, int offset)
        {
            var obj = new JObject();

            for (int i = 0; i < size; i++) {
                Result left = Decode(offset);
                var key = left.Node;
                offset = left.Offset;
                Result right = Decode(offset);
                var value = right.Node;
                offset = right.Offset;
                obj.Add(key.Value<string>(), value);
            }

            return new Result(obj, offset);
        }
        private JValue DecodeLong(byte[] buffer)
        {
            long integer = 0;
            for (int i = 0; i < buffer.Length; i++) {
                integer = (integer << 8) | buffer[i];
            }
            return new JValue(integer);
        }
        private Result DecodeArray(int size, int offset)
        {
            var array = new JArray();

            for (int i = 0; i < size; i++) {
                Result r = Decode(offset);
                offset = r.Offset;
                array.Add(r.Node);
            }

            return new Result(array, offset);
        }
        private JValue DecodeUint64(byte[] buffer)
        {
            UInt64 integer = 0;
            for (int i = 0; i < buffer.Length; i++) {
                integer = (integer << 8) | buffer[i];
            }
            return new JValue(integer);
        }
        private Result DecodePointer(int ctrlByte, int offset)
        {
            int pointerSize = ((ctrlByte >> 3) & 0x3) + 1;
            int b = pointerSize == 4 ? 0 : (byte)(ctrlByte & 0x7);
            byte[] buffer = ReadMany(offset, pointerSize);
            int packed = Decoder.DecodeInteger(buffer, b);
            long pointer = packed + pointerBase + pointerValueOffset[pointerSize];
            return new Result(new JValue(pointer), offset + pointerSize);
        }
        
        internal static int DecodeInteger(byte[] buffer, int baseValue = 0)
        {
            int integer = baseValue;
            for (int i = 0; i < buffer.Length; i++) {
                integer = (integer << 8) | buffer[i];
            }
            return integer;
        }
    }
}