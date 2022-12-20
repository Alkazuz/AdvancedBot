using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net.Security;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using IOBuffer = System.Buffer;

namespace AdvancedBot.client
{
    //used for Mojang auth SOCKS4/5 with proxy support.
    public class HttpConnection
    {
        public const string JAVA_USER_AGENT = "Java/1.8.0_66";

        private const string HTTPS_GET_FORMAT = "GET https://{0}{1} HTTP/1.1";
        private const string HTTP_GET_FORMAT = "GET {1} HTTP/1.1";

        private const string HTTPS_POST_FORMAT = "POST https://{0}{1} HTTP/1.1";
        private const string HTTP_POST_FORMAT = "POST {1} HTTP/1.1";

        public readonly Uri URL;
        public Proxy Proxy;
        public Dictionary<string, string> Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public HttpConnection(string url)
        {
            URL = new Uri(url);
            Debug.WriteLine("Creating HttpConnection --> " + url);

            //GET /pathAndQuery HTTP/1.1
            //User-Agent: Java/1.8.0_111
            //Host: localhost:8888
            //Accept: text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2
            //Connection: keep-alive

            Headers["User-Agent"] = JAVA_USER_AGENT;
            Headers["Accept"] = "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2";
            Headers["Connection"] = "keep-alive";
        }

        #region Blocking methods
        private HttpResponse DoGet()
        {
            TcpClient c = CreateClient();

            bool https = URL.Port == 443;
            Stream s = c.GetStream();

            if (https) {
                SslStream ssl = new SslStream(c.GetStream());
                ssl.AuthenticateAsClient(URL.Host);
                s = ssl;
            }
            StringBuilder req = new StringBuilder();
            req.Append(string.Format(https ? HTTPS_GET_FORMAT : HTTP_GET_FORMAT, URL.Host, URL.PathAndQuery) + "\r\n");
            req.Append("Host: " + URL.Host + "\r\n");
            foreach (KeyValuePair<string, string> header in Headers)
                req.Append(header.Key + ": " + header.Value + "\r\n");
            req.Append("\r\n");

            byte[] rb = Encoding.ASCII.GetBytes(req.ToString());
            s.Write(rb, 0, rb.Length);

            return HttpResponse.Read(s);
        }
        private HttpResponse DoPost(byte[] data)
        {
            TcpClient c = CreateClient();
            bool https = URL.Port == 443;
            Stream s = c.GetStream();

            if (https) {
                SslStream ssl = new SslStream(c.GetStream());
                ssl.AuthenticateAsClient(URL.DnsSafeHost);
                s = ssl;
            }

            StringBuilder req = new StringBuilder();
            req.Append(string.Format(https ? HTTPS_POST_FORMAT : HTTP_POST_FORMAT, URL.DnsSafeHost, URL.PathAndQuery) + "\r\n");
            req.Append("Host: " + URL.Host + "\r\n");
            foreach (KeyValuePair<string, string> header in Headers)
                req.Append(header.Key + ": " + header.Value + "\r\n");
            if (!Headers.ContainsKey("Content-Length"))
                req.Append("Content-Length: " + data.Length + "\r\n");
            req.Append("\r\n");

            byte[] rb = new byte[req.Length + data.Length];
            byte[] reqb = Encoding.ASCII.GetBytes(req.ToString());
            Array.Copy(reqb, rb, reqb.Length);
            Array.Copy(data, 0, rb, req.Length, data.Length);

            s.Write(rb, 0, rb.Length);

            return HttpResponse.Read(s);
        }

        private TcpClient CreateClient()
        {
            //string ip = Dns.GetHostEntry(URL.DnsSafeHost).AddressList[0].ToString();
            return Proxy != null ? Proxy.Connect(URL.DnsSafeHost, (ushort)URL.Port) : new TcpClient(URL.DnsSafeHost, URL.Port);
        }

        public byte[] Download()
        {
            return Read(DoGet());
        }
        public byte[] Upload(byte[] data)
        {
            return Read(DoPost(data));
        }

        private static byte[] Read(HttpResponse r)
        {
            int length = r.GetHeaderInt("Content-Length");
            string ttenc = r.GetHeaderStr("Transfer-Encoding");
            string[] tenc = ttenc == null ? new string[0] : ttenc.ToLower().Split(new char[] { ',', ' ' }, StringSplitOptions.None);

            byte[] data = new byte[0];
            if (tenc.Contains("chunked"))
                data = ReadChunked(r.Stream);
            else
                data = ReadByteArray(r.Stream, length);

            r.Stream.Close();

            string tmp;
            if (r.Headers.TryGetValue("Content-Encoding", out tmp)) {
                using (MemoryStream s = new MemoryStream(data)) {
                    Stream dec;
                    if (tmp == "deflate") { //zlib
                        s.Position = 2;
                        dec = new DeflateStream(s, CompressionMode.Decompress);
                    } else if (tmp == "gzip") {
                        dec = new GZipStream(s, CompressionMode.Decompress);
                    } else throw new InvalidOperationException("Content encoding '" + tmp + "' is not supported.");
                    using (MemoryStream result = new MemoryStream()) {
                        dec.CopyTo(result);
                        data = result.ToArray();
                    }
                }
            }

            return data;
        }

        private static byte[] ReadChunked(Stream s)
        {
            using (MemoryStream mem = new MemoryStream()) {
                int size;

                do {
                    size = ReadChunkSize(s);
                    if (size != 0)
                        mem.Write(ReadByteArray(s, size), 0, size);
                    s.ReadByte();//\r
                    s.ReadByte();//\n
                } while (size != 0);
                return mem.ToArray();
            }
        }
        private static int ReadChunkSize(Stream s)
        {
            const string HEX_CHARS = "0123456789abcdef";

            int r = 0;
            for (int c; (c = s.ReadByte()) != '\r'; )
                r = (r << 4) | HEX_CHARS.IndexOf((char)(c | 0x20));
            s.ReadByte();//read '\n'

            return r;
        }
        private static byte[] ReadByteArray(Stream s, int length)
        {
            if (length <= 0) return new byte[0];
            byte[] buffer = new byte[length];

            for (int remain = length; remain > 0; ) {
                int r = s.Read(buffer, length - remain, remain);
                if (r == 0) throw new EndOfStreamException("EOF when reading byte array.");
                remain -= r;
            }
            return buffer;
        }
        #endregion

        public Task<string> GetAsync()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(URL.Port == 443 ? HTTPS_GET_FORMAT : HTTP_GET_FORMAT, URL.Host, URL.PathAndQuery) + "\r\n");
            sb.Append("Host: " + URL.Host + "\r\n");
            foreach (KeyValuePair<string, string> header in Headers)
                sb.Append(header.Key + ": " + header.Value + "\r\n");
            sb.Append("\r\n");
            return RequestAsync(Encoding.ASCII.GetBytes(sb.ToString()));
        }
        public Task<string> PostAsync(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format(URL.Port == 443 ? HTTPS_POST_FORMAT : HTTP_POST_FORMAT, URL.DnsSafeHost, URL.PathAndQuery) + "\r\n");
            sb.Append("Host: " + URL.Host + "\r\n");
            foreach (KeyValuePair<string, string> header in Headers)
                sb.Append(header.Key + ": " + header.Value + "\r\n");
            if (!Headers.ContainsKey("Content-Length"))
                sb.Append("Content-Length: " + data.Length + "\r\n");
            sb.Append("\r\n");

            byte[] final = new byte[sb.Length + data.Length];
            byte[] req = Encoding.ASCII.GetBytes(sb.ToString());
            IOBuffer.BlockCopy(req, 0, final, 0, req.Length);
            IOBuffer.BlockCopy(data, 0, final, req.Length, data.Length);
            return RequestAsync(final);
        }
        private async Task<string> RequestAsync(byte[] request)
        {
            Socket socket;
            if (Proxy != null) {
                socket = await Proxy.ConnectAsync(URL.DnsSafeHost, (ushort)URL.Port).ConfigureAwait(false);
            } else {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await Task.Factory.FromAsync(socket.BeginConnect, socket.EndConnect, URL.DnsSafeHost, URL.Port, null).ConfigureAwait(false);
            }

            bool https = URL.Port == 443;

            Stream s = new NetworkStream(socket, true);

            try {
                if (https) {
                    SslStream ssl = new SslStream(s);
                    await ssl.AuthenticateAsClientAsync(URL.Host);
                    s = ssl;
                }
                await s.WriteAsync(request, 0, request.Length).ConfigureAwait(false);

                byte[] buf = new byte[16384];
                int read = await s.ReadAsync(buf, 0, buf.Length).ConfigureAwait(false);
                int p = 0;
                var resp = HttpResponse.Read(buf, ref p, read);

                var reader = new AsyncReader(s, buf, p, read);

                int length = resp.GetHeaderInt("Content-Length");
                string tenc = resp.GetHeaderStr("Transfer-Encoding");

                byte[] data;
                if (tenc != null && tenc.Contains("chunked")) {
                    using (MemoryStream mem = new MemoryStream()) {
                        for (int size; (size = await reader.ReadChunkSize().ConfigureAwait(false)) != 0;) {
                            await reader.ReadTo(mem, size).ConfigureAwait(false);
                            await reader.ReadBytes(2).ConfigureAwait(false);
                        }
                        //await reader.ReadBytes(2).ConfigureAwait(false)
                        data = mem.GetBuffer();
                        length = (int)mem.Length;
                    }
                } else {
                    data = await reader.ReadBytes(length).ConfigureAwait(false);
                }
                if (resp.Headers.TryGetValue("Content-Encoding", out string tmp)) {
                    Stream dec;
                    if (tmp == "deflate") {
                        //zlib header
                        dec = new DeflateStream(new MemoryStream(data) { Position = 2 }, CompressionMode.Decompress);
                    } else if (tmp == "gzip") {
                        dec = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
                    } else {
                        throw new NotSupportedException($"Content encoding '{tmp}'.");
                    }
                    using (MemoryStream result = new MemoryStream()) {
                        dec.CopyTo(result, 4096);
                        data = result.GetBuffer();
                        length = (int)result.Length;
                    }
                }
                return Encoding.UTF8.GetString(data, 0, length);
            } finally {
                s.Dispose();
            }
        }

        private class AsyncReader
        {
            public Stream Stream;
            public byte[] Buffer;
            public int Offset;
            public int Length;
            public int Remaining { get { return Length - Offset; } }

            public AsyncReader(Stream s, byte[] buf, int ofs, int len)
            {
                Stream = s;
                Buffer = buf;
                Offset = ofs;
                Length = len;
            }
            public AsyncReader(Stream s)
            {
                Stream = s;
                Buffer = new byte[16384];
            }

            private async Task EnsureReadable()
            {
                Length = await Stream.ReadAsync(Buffer, 0, Buffer.Length).ConfigureAwait(false);
                Offset = 0;
                if (Length == 0) {
                    throw new EndOfStreamException();
                }
            }

            public async Task<byte> ReadByte()
            {
                if (Remaining <= 0) {
                    await EnsureReadable().ConfigureAwait(false);
                }
                return Buffer[Offset++];
            }
            public async Task ReadTo(Stream mem, int count)
            {
                int remain = count;
                
                while (remain > 0) {
                    if (Remaining <= 0) {
                        await EnsureReadable().ConfigureAwait(false);
                    }
                    int l = Math.Min(Remaining, remain);
                    mem.Write(Buffer, Offset, l);
                    Offset += l;
                    remain -= l;
                }
            }
            public async Task<byte[]> ReadBytes(int count)
            {
                byte[] buf = new byte[count];
                int remain = count;

                if(Remaining > 0) {
                    int l = Math.Min(Remaining, count);
                    IOBuffer.BlockCopy(Buffer, Offset, buf, 0, l);
                    Offset += l;
                    remain -= l;
                }
                while(remain > 0) {
                    int r = await Stream.ReadAsync(buf, count - remain, remain).ConfigureAwait(false);
                    if (r == 0) throw new EndOfStreamException();
                    remain -= r;
                }
                return buf;
            }
            public async Task<int> ReadChunkSize()
            {
                const string HEX_CHARS = "0123456789abcdef";

                int r = 0;

                //ReadByte() = (Remaining > 0 ? Buffer[Offset++] : await ReadByte().ConfigureAwait(false))
                while (true) {
                    byte c = Remaining > 0 ? Buffer[Offset++] : await ReadByte().ConfigureAwait(false);
                    if (c == '\r') {
                        if (Remaining > 0) {//advance 1 byte on stream
                            Offset++;
                        } else {
                            await ReadByte().ConfigureAwait(false);
                        }
                        return r;
                    }
                    r = (r << 4) | HEX_CHARS.IndexOf((char)(c | 0x20));
                }
            }
        }
    }
    public class HttpResponse
    {
        private static readonly Regex HTTP_STATUS_REGEX = new Regex(@"HTTP\/\d.\d (\d+) ([a-zA-Z ]+)");
        public Dictionary<string, string> Headers;
        public Stream Stream;
        public int Code;
        public string CodeStr;
        public string RawStatus;

        private HttpResponse(int c, string cs, string rawStats, Stream s)
        {
            Code = c;
            CodeStr = cs;
            Stream = s;
            RawStatus = rawStats;
            Headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public static HttpResponse Read(Stream s)
        {
            string raw = ReadHeader(s);
            Match m = HTTP_STATUS_REGEX.Match(raw);

            HttpResponse rsp = new HttpResponse(int.Parse(m.Groups[1].Value), m.Groups[2].Value, raw, s);
            for (string header; !string.IsNullOrWhiteSpace((header = ReadHeader(s))); ) {
                int idx = header.IndexOf(": ");
                rsp.Headers[header.Substring(0, idx)] = header.Substring(idx + 2);
            }

            return rsp;
        }
        public static HttpResponse Read(byte[] buf, ref int pos, int len)
        {
            string raw = ReadHeader(buf, ref pos, len);
            Match m = HTTP_STATUS_REGEX.Match(raw);

            HttpResponse rsp = new HttpResponse(int.Parse(m.Groups[1].Value), m.Groups[2].Value, raw, null);
            for (string header; (header = ReadHeader(buf, ref pos, len)).Length != 0; ) {
                int idx = header.IndexOf(": ");
                rsp.Headers[header.Substring(0, idx)] = header.Substring(idx + 2);
            }

            return rsp;
        }

        public int GetHeaderInt(string name)
        {
            return Headers.TryGetValue(name, out string r) ? int.Parse(r) : -1;
        }
        public string GetHeaderStr(string name)
        {
            return Headers.TryGetValue(name, out string r) ? r : "";
        }
        private static string ReadHeader(Stream s)
        {
            StringBuilder sb = new StringBuilder(32);

            while (true) {
                char c = (char)s.ReadByte();
                if (c == '\r') {
                    s.ReadByte();//\n
                    break;
                }
                sb.Append(c);
            }

            return sb.ToString();
        }

        private static string ReadHeader(byte[] buf, ref int pos, int len)
        {
            int start = pos;
            for (; pos < len; pos++) {
                if (buf[pos] == '\r') {
                    if (buf[pos + 1] != '\n') break;
                    pos += 2;
                    return Encoding.ASCII.GetString(buf, start, (pos - 2) - start);
                }
            }
            throw new EndOfStreamException();
        }
    }
}