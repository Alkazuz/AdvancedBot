using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace AdvancedBot
{
    public class SrvResolver
    {
        public static bool ResolveIP(ref string ip, ref ushort port)
        {
            return GetSRVRecords("_minecraft._tcp." + ip, ref ip, ref port);
        }
        public static List<string> GetRecordList(string ip, ushort port)
        {
            List<string> r = new List<string>();
            GetSRVRecords("_minecraft._tcp." + ip, ref ip, ref port, r);
            return r;
        }

        [DllImport("dnsapi.dll", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int DnsQuery(string pszName, int wType, QueryOptions options, IntPtr pExtra, ref IntPtr ppQueryResults, IntPtr pReserved);

        [DllImport("dnsapi.dll")] private static extern void DnsRecordListFree(IntPtr pRecordList, int FreeType);
        private const int DNS_TYPE_SRV = 0x0021;

        private static bool GetSRVRecords(string needle, ref string ip, ref ushort port, List<string> records = null)
        {
            IntPtr queryResults = IntPtr.Zero;
            SRVRecord recSRV;
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new NotSupportedException();

            try {
                int code = DnsQuery(needle, DNS_TYPE_SRV, QueryOptions.DNS_QUERY_BYPASS_CACHE, IntPtr.Zero, ref queryResults, IntPtr.Zero);
                if (code != 0)
                    return false;

                for (IntPtr record = queryResults; record != IntPtr.Zero; record = recSRV.pNext) {
                    recSRV = (SRVRecord)Marshal.PtrToStructure(record, typeof(SRVRecord));
                    if (recSRV.wType == DNS_TYPE_SRV) {
                        if (records != null) {
                            records.Add(Marshal.PtrToStringAuto(recSRV.pNameTarget) + ":" + recSRV.wPort);
                        } else {
                            ip = Marshal.PtrToStringAuto(recSRV.pNameTarget);
                            port = recSRV.wPort;
                            return true;
                        }
                    }
                }
            } finally {
                DnsRecordListFree(queryResults, 0);
            }
            return false;
        }

        private enum QueryOptions
        {
            DNS_QUERY_ACCEPT_TRUNCATED_RESPONSE = 1,
            DNS_QUERY_BYPASS_CACHE = 8,
            DNS_QUERY_DONT_RESET_TTL_VALUES = 0x100000,
            DNS_QUERY_NO_HOSTS_FILE = 0x40,
            DNS_QUERY_NO_LOCAL_NAME = 0x20,
            DNS_QUERY_NO_NETBT = 0x80,
            DNS_QUERY_NO_RECURSION = 4,
            DNS_QUERY_NO_WIRE_QUERY = 0x10,
            DNS_QUERY_RESERVED = -16777216,
            DNS_QUERY_RETURN_MESSAGE = 0x200,
            DNS_QUERY_STANDARD = 0,
            DNS_QUERY_TREAT_AS_FQDN = 0x1000,
            DNS_QUERY_USE_TCP_ONLY = 2,
            DNS_QUERY_WIRE_ONLY = 0x100
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SRVRecord
        {
            public IntPtr pNext;
            public string pName;
            public short wType;
            public short wDataLength;
            public int flags;
            public int dwTtl;
            public int dwReserved;
            public IntPtr pNameTarget;
            public short wPriority;
            public short wWeight;
            public ushort wPort;
            public short Pad;
        }
    }
}
