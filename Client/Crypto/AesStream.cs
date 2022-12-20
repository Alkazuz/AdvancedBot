using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace AdvancedBot.client.Crypto
{
    public class AesStream : Stream
    {
        private ICryptoTransform encTransform, decTransform;
        private CryptoStream encStream, decStream;

        public Stream BaseStream { get; set; }
        public AesStream(Stream stream, byte[] key)
        {
            BaseStream = stream;
            RijndaelManaged aes = GenerateAES(key);
            encTransform = aes.CreateEncryptor();
            decTransform = aes.CreateDecryptor();
            encStream = new CryptoStream(stream, encTransform, CryptoStreamMode.Write);
            decStream = new CryptoStream(stream, decTransform, CryptoStreamMode.Read);
        }
        private static RijndaelManaged GenerateAES(byte[] key)
        {
            RijndaelManaged cipher = new RijndaelManaged();
            cipher.Mode = CipherMode.CFB;
            cipher.Padding = PaddingMode.None;
            cipher.KeySize = 128;
            cipher.FeedbackSize = 8;
            cipher.Key = key;
            cipher.IV = key;
            return cipher;
        }
        
        public override int ReadByte() { return decStream.ReadByte(); }
        public override void WriteByte(byte b) { encStream.WriteByte(b); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return decStream.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            encStream.Write(buffer, offset, count);
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return BaseStream.BeginRead(buffer, offset, count, CallbackHook, new AsyncResultHook(buffer, offset, count, callback, state));
        }
        private void CallbackHook(IAsyncResult asyncResult)
        {
            AsyncResultHook rh = (AsyncResultHook)asyncResult.AsyncState;
            rh.baseResult = asyncResult;
            rh.callback(rh);
        }
        public override int EndRead(IAsyncResult asyncResult)
        {
            AsyncResultHook bar = (AsyncResultHook)asyncResult;
            int bytesRead = BaseStream.EndRead(bar.baseResult);
            
            byte[] outputBuf = new byte[bytesRead];
            int transformed = decTransform.TransformBlock(bar.buffer, bar.offset, bytesRead, outputBuf, 0);
            if (transformed != bytesRead) {
                throw new Exception("Unexpected: AesDecryptor.TransformBlock() != BytesRead");
            }
            Buffer.BlockCopy(outputBuf, 0, bar.buffer, bar.offset, bytesRead);
            return bytesRead;
        }
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            byte[] outputBuf = new byte[count];
            int transformed = encTransform.TransformBlock(buffer, offset, count, outputBuf, 0);
            if (transformed != count) {
                throw new Exception("Unexpected: AesEncryptor.TransformBlock() != Count");
            }
            return BaseStream.BeginWrite(outputBuf, 0, count, callback, state);
        }
        public override void EndWrite(IAsyncResult asyncResult)
        {
            BaseStream.EndWrite(asyncResult);
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return true; } }
        public override void Flush() { BaseStream.Flush(); }
        public override long Length { get { throw new NotSupportedException(); } }
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public class AsyncResultHook : IAsyncResult
        {
            public int offset, length;
            public byte[] buffer;
            public object state;
            public AsyncCallback callback;
            public IAsyncResult baseResult;

            public AsyncResultHook(byte[] buf, int ofs, int len, AsyncCallback cb, object stt)
            {
                buffer = buf;
                offset = ofs;
                length = len;
                callback = cb;
                state = stt;
            }

            public object AsyncState { get { return state; } }
            public WaitHandle AsyncWaitHandle { get { return baseResult.AsyncWaitHandle; } }
            public bool CompletedSynchronously { get { return baseResult.CompletedSynchronously; } }
            public bool IsCompleted { get { return baseResult.IsCompleted; } }
        }
    }
}
