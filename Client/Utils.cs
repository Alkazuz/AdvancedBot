using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ionic.Zlib;
using System.Threading;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using ZipArchiveEntry = System.IO.Compression.ZipArchiveEntry;

namespace AdvancedBot.client
{
    public static class Utils
    {
        public static bool EqualsIgnoreCase(this string t, string other)
        {
            return t.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
        public static bool ContainsIgnoreCase(this string t, string val)
        {
            return t.IndexOf(val, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static float WrapAngleTo180(float a)
        {
            a %= 360.0f;

            if (a >= 180.0f) a -= 360.0f;
            if (a < -180.0f) a += 360.0f;

            return a;
        }
        public static float WrapAngleTo360(float a)
        {
            a = WrapAngleTo180(a);
            if (a < 0.0f) a += 360.0f;
            return a;
        }
        /*public static string StripColorCodes(string chat)
        {
            char[] chars = new char[chat.Length];
            int n = 0;

            for (int i = 0; i < chars.Length; i++) {
                char ch = chat[i];
                if (ch == '§') {
                    ++i;
                } else {
                    chars[n++] = ch;
                }
            }

            return new string(chars, 0, n);
        }*/
        public static unsafe string StripColorCodes(string chat)
        {
            char[] dst = new char[chat.Length];
            int n = 0;
            fixed(char* s = chat, d = dst) {
                for (int i = 0; i < dst.Length; i++) {
                    char ch = s[i];
                    if (ch == '§') {
                        ++i;
                    } else {
                        d[n] = ch;
                        ++n;
                    }
                }
                return new string(d, 0, n);
            }
        }

        public static double DistTo(double x1, double y1, double z1,
                                    double x2, double y2, double z2)
        {
            double x = x1 - x2;
            double y = y1 - y2;
            double z = z1 - z2;
            return Math.Sqrt(x * x + y * y + z * z);
        }
        public static double DistToSq(double x1, double y1, double z1,
                                   double x2, double y2, double z2)
        {
            double x = x1 - x2;
            double y = y1 - y2;
            double z = z1 - z2;
            return x * x + y * y + z * z;
        }

        public static int Floor(double x)
        {
            int i = (int)x;
            return x < i ? i - 1 : i;
        }

        public static double Clamp(double x, double min, double max)
        {
            return x < min ? min : x > max ? max : x;
        }
        public static int Clamp(int x, int min, int max)
        {
            return x < min ? min : x > max ? max : x;
        }

        public static byte[] Deflate(byte[] b, int len, CompressionLevel lvl)
        {
            using (MemoryStream ms = new MemoryStream()) {
                using (ZlibStream zlib = new ZlibStream(ms, CompressionMode.Compress, lvl)) {
                    zlib.Write(b, 0, len);
                }
                return ms.ToArray();
            }
        }
        public static byte[] Inflate(byte[] b, int ofs, int inLen, int outLen)
        {
            using (var input = new MemoryStream(b, ofs, inLen)) {
                byte[] outp = new byte[outLen];
                using (var inf = new ZlibStream(input, CompressionMode.Decompress)) {
                    for (int read = 0; ;) {
                        int rem = outLen - read;
                        int bs = inf.Read(outp, read, rem);
                        if (bs == 0) break;
                        read += bs;
                    }
                    return outp;
                }
            }
        }
        public static int VarIntSize(int v)
        {
            uint val = (uint)v;
            return (val & 0xFFFFFF80) == 0 ? 1 :
                   (val & 0xFFFFC000) == 0 ? 2 :
                   (val & 0xFFE00000) == 0 ? 3 :
                   (val & 0xF0000000) == 0 ? 4 : 5;
        }
        public static void PutVarInt(byte[] buf, ref int pos, int val)
        {
            uint u = (uint)val;
            for (; u > 0x7F; u >>= 7) {
                buf[pos++] = (byte)((u & 0x7F) | 0x80);
            }
            buf[pos++] = (byte)u;
        }

        public static async Task<IPEndPoint> ResolveDnsAsync(string host, int port)
        {
            IPAddress[] addr = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
            IPAddress IPv6 = addr.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6);
            IPAddress IPv4 = addr.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            if (IPv4 != null) {
                return new IPEndPoint(IPv4, port);
            } else {
                return new IPEndPoint(IPv6, port);
            }
        }

        public static int HammingWeight32(int x)
        {
            x -= (x >> 1) & 0x55555555;
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            x = (x + (x >> 4)) & 0x0f0f0f0f;
            return (x * 0x01010101) >> 24;
        }

        public static int XorShift32(int x)
        {
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            return x;
        }

        public static Random NewRandom()
        {
            using (var rng = new RNGCryptoServiceProvider()) {
                byte[] s = new byte[4];
                rng.GetNonZeroBytes(s);
                return new Random(s[0] << 24 | s[1] << 16 | s[2] << 8 | s[3]);
            }
        }
        public static int GetBits(int n, int pos, int bitCount)
        {
            int mask = (1 << bitCount) - 1;
            return n >> pos & mask; 
        }
        public static void SetBits(ref int n, int val, int pos, int bitCount)
        {
            int mask = (1 << bitCount) - 1;
            n = (n & ~(mask << pos)) | val << pos;
        }

        public static long GetTimestamp()
        {
            return DateTime.UtcNow.Ticks / 10000;
        }

        public static async Task<string> GetStringAsync(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Proxy = null;
            req.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            req.KeepAlive = true;
            req.Timeout = 5000;
            try {
                using (var response = await req.GetResponseAsync().ConfigureAwait(false)) {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
                        return await sr.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            } catch {
                return "";
            }
        }

        public static string AsStr(this JToken j)
        {
            return j.Value<string>();
        }
        public static int AsInt(this JToken j)
        {
            return j.Value<int>();
        }
        public static float AsFloat(this JToken j)
        {
            return j.Value<float>();
        }
        public static double AsDouble(this JToken j)
        {
            return j.Value<double>();
        }
        public static JObject AsJObj(this JToken j)
        {
            return j.Value<JObject>();
        }
        public static JArray AsJArr(this JToken j)
        {
            return j.Value<JArray>();
        }
        public static bool AsBool(this JToken j)
        {
            return j != null && j.Value<bool>();
        }
        public static string AsStrOr(this JToken j, string _default)
        {
            return j == null ? _default : j.Value<string>();
        }
        public static int AsIntOr(this JToken j, int _default)
        {
            return j == null ? _default : j.Value<int>();
        }
        public static double AsDoubleOr(this JToken j, double _default)
        {
            return j == null ? _default : j.Value<double>();
        }
        public static bool AsBoolOrTrue(this JToken j)
        {
            return j == null ? true : j.Value<bool>();
        }
        public static T ToObjectOr<T>(this JToken j, T _default)
        {
            return j == null ? _default : j.ToObject<T>();
        }

        public static int RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dict, Func<TValue, bool> predicate)
        {
            List<TKey> keys = new List<TKey>();
            foreach (KeyValuePair<TKey, TValue> entry in dict) {
                if (predicate(entry.Value)) {
                    keys.Add(entry.Key);
                }
            }
            foreach (TKey key in keys) {
                dict.Remove(key);
            }
            return keys.Count;
        }

        public static async Task<TResult> SetTimeout<TResult>(this Task<TResult> task, int timeout)
        {
            //https://stackoverflow.com/a/22078975
            using (var cts = new CancellationTokenSource()) {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false);
                if (completedTask == task) {
                    cts.Cancel();
                    return await task.ConfigureAwait(false);
                } else {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }

        public static string ReadString(this ZipArchiveEntry entry)
        {
            using (StreamReader sr = new StreamReader(entry.Open(), Encoding.UTF8)) {
                return sr.ReadToEnd();
            }
        }
        public static JObject ReadJson(this ZipArchiveEntry entry)
        {
            using (JsonTextReader jr = new JsonTextReader(new StreamReader(entry.Open()))) {
                return JObject.Load(jr);
            }
        }

        public static long TotalMilliseconds(this DateTime dt)
        {
            return dt.Ticks / 10000;
        }
        public static byte[] UTF8Bytes(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }
    }
}
