using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Globalization;
using System.Diagnostics;
using System.Threading;

namespace AdvancedBot.client
{
    //taken from java.util.UUID
    public class UUID
    {
        private static MD5CryptoServiceProvider _md5 = new MD5CryptoServiceProvider();

        public readonly long HiBits;
        public readonly long LoBits;

        public UUID(long hiBits, long loBits)
        {
            this.HiBits = hiBits;
            this.LoBits = loBits;
        }
        public static unsafe UUID NameUUID(string s)
        {
            Monitor.Enter(_md5);
            byte[] hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(s));
            Monitor.Exit(_md5);

            hash[6] = (byte)((hash[6] & 0x0F) | 0x30); //set version to 3
            hash[8] = (byte)((hash[8] & 0x3F) | 0x80); //set variant to IETF

            long hi = 0, lo = 0;
            for (int i = 0; i < 8; i++) hi = (hi << 8) | (long)hash[i];
            for (int i = 8; i < 16; i++) lo = (lo << 8) | (long)hash[i];
            return new UUID(hi, lo);
        }
        public static UUID Parse(string s)
        {
            //8-4-4-4-12
            if (s[8] != '-') {
                return new UUID(parseHex(s.Substring(0, 16)), parseHex(s.Substring(16, 16)));
            } else {
                string[] comp = s.Split('-');
                long hi = parseHex(comp[0]) << 32 | parseHex(comp[1]) << 16 | parseHex(comp[2]);
                long lo = parseHex(comp[3]) << 48 | parseHex(comp[4]);
                return new UUID(hi, lo);
            }
        }
        private static long parseHex(string s)
        {
            return long.Parse(s, NumberStyles.HexNumber);
        }
        private static string digits(long val, int digits)
        {
            char[] ch = new char[digits];
            for (int i = digits - 1; i >= 0; i--, val >>= 4) {
                ch[i] = "0123456789abcdef"[(int)(val & 0xF)];
            }
            return new string(ch);
        }

        public override string ToString()
        {
            return digits(HiBits >> 32, 8) + "-" +
                   digits(HiBits >> 16, 4) + "-" +
                   digits(HiBits, 4) + "-" +
                   digits(LoBits >> 48, 4) + "-" +
                   digits(LoBits, 12);
        }
        public override int GetHashCode()
        {
            long hilo = HiBits ^ LoBits;
            return (int)(hilo >> 32) ^ (int)hilo;
        }
        public override bool Equals(object obj)
        {
            if (obj is UUID) {
                UUID u = (UUID)obj;
                return u.HiBits == HiBits && u.LoBits == LoBits;
            }
            return false;
        }
    }
}
