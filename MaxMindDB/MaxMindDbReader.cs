﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MaxMind.MaxMindDb
{
    /// <summary>
    /// Given a MaxMind DB file, this class will retrieve information about an IP address
    /// </summary>
    public class MaxMindDbReader
    {
        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        public Metadata Metadata { get; private set; }

        #region Private

        private int DATA_SECTION_SEPARATOR_SIZE = 16;

        private byte[] METADATA_START_MARKER = new byte[] { 0xAB, 0xCD, 0xEF, 77, 97, 120, 77, 105, 110, 100, 46, 99, 111, 109 };

        private int _ipV4Start;
        private int ipV4Start
        {
            get {
                if (_ipV4Start == 0 || Metadata.IpVersion == 4) {
                    int node = 0;
                    for (int i = 0; i < 96 && node < Metadata.NodeCount; i++) {
                        node = ReadNode(node, 0);
                    }
                    _ipV4Start = node;
                }
                return _ipV4Start;
            }
        }

        private byte[] dbData { get; set; }

        private Decoder Decoder { get; set; }

        private int MetadataStart { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxMindDbReader"/> class.
        /// </summary>
        /// <param name="file">The MaxMind DB file.</param>
        public MaxMindDbReader(string file)
        {
            dbData = File.ReadAllBytes(file);

            int start = MetadataStart = FindMetadataStart();
            Result result = new Decoder(dbData, start).Decode(start);
            Metadata = result.Node.ToObject<Metadata>();
            Decoder = new Decoder(dbData, Metadata.SearchTreeSize + DATA_SECTION_SEPARATOR_SIZE);
        }

        /// <summary>
        /// Finds the data related to the specified address.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <returns>An object containing the IP related data</returns>
        public JToken Find(string address)
        {
            return Find(IPAddress.Parse(address));
        }

        /// <summary>
        /// Finds the data related to the specified address.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <returns>An object containing the IP related data</returns>
        public JToken Find(IPAddress address)
        {
            int pointer = FindAddressInTree(address);
            if (pointer == 0)
                return null;

            return ResolveDataPointer(pointer);
        }
        public IEnumerable<JToken> EnumerateDataSection()
        {
            //https://maxmind.github.io/MaxMind-DB/
            int start = (int)Metadata.SearchTreeSize + 16;
            int end = MetadataStart - METADATA_START_MARKER.Length;

            for (int i = start; i < end;) {
                var result = Decoder.Decode(i);
                yield return result.Node;
                i = result.Offset;
            }
        }

        #region Private

        private JToken ResolveDataPointer(int pointer)
        {
            int resolved = (int)((pointer - Metadata.NodeCount) + Metadata.SearchTreeSize);

            if (resolved >= dbData.Length) {
                throw new InvalidDatabaseException(
                        "The MaxMind DB file's search tree is corrupt: "
                                + "contains pointer larger than the database.");
            }

            return Decoder.Decode(resolved).Node;
        }

        private int FindAddressInTree(IPAddress address)
        {
            byte[] rawAddress = address.GetAddressBytes();

            int bitLength = rawAddress.Length * 8;
            int record = StartNode(bitLength);

            for (int i = 0; i < bitLength; i++) {
                if (record >= Metadata.NodeCount) {
                    break;
                }
                byte b = rawAddress[i / 8];
                int bit = 1 & (b >> 7 - (i % 8));
                record = ReadNode(record, bit);
            }
            if (record == Metadata.NodeCount) {
                // record is empty
                return 0;
            } else if (record > Metadata.NodeCount) {
                // record is a data pointer
                return record;
            }
            throw new InvalidDatabaseException("Something bad happened");
        }

        private int StartNode(int bitLength)
        {
            // Check if we are looking up an IPv4 address in an IPv6 tree. If this
            // is the case, we can skip over the first 96 nodes.
            if (Metadata.IpVersion == 6 && bitLength == 32) {
                return ipV4Start;
            }
            // The first node of the tree is always node 0, at the beginning of the
            // value
            return 0;
        }
        private int FindMetadataStart()
        {
            var metaLen = METADATA_START_MARKER.Length;
            for (int i = (dbData.Length - metaLen); i > 0; i--) {
                bool found = true;
                for (int j = 0; j < metaLen; j++) {
                    if (METADATA_START_MARKER[j] != dbData[i + j]) {
                        found = false;
                        break;
                    }
                }
                if (found) {
                    return i + METADATA_START_MARKER.Length;
                }
            }

            throw new InvalidDatabaseException("Could not find a MaxMind DB metadata marker in this file. Is this a valid MaxMind DB file?");
        }

        private int ReadNode(int nodeNumber, int index)
        {
            int baseOffset = (int)(nodeNumber * Metadata.NodeByteSize);

            int size = (int)Metadata.RecordSize;

            if (size == 24) {
                byte[] buffer = ReadMany(baseOffset + index * 3, 3);
                return Decoder.DecodeInteger(buffer);
            } else if (size == 28) {
                byte middle = dbData[baseOffset + 3];
                middle = (index == 0) ? (byte)(middle >> 4) : (byte)(0x0F & middle);

                byte[] buffer = ReadMany(baseOffset + index * 4, 3);
                return Decoder.DecodeInteger(buffer, middle);
            } else if (size == 32) {
                byte[] buffer = ReadMany(baseOffset + index * 4, 4);
                return Decoder.DecodeInteger(buffer);
            }

            throw new InvalidDatabaseException("Unknown record size: " + size);
        }

        private byte[] ReadMany(int position, int size)
        {
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(dbData, position, buffer, 0, size);
            return buffer;
        }

        #endregion
    }
}