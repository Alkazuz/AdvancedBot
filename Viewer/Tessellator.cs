using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AdvancedBot.Viewer
{
    public class Tessellator
    {
        public static readonly Tessellator Instance = new Tessellator();

        //5242880
        protected int[] buffer = new int[1310720];//5MiB or ~218K vertices
        protected int vertices = 0;
        private int bufferPos;

        protected float u, v;
        protected int color;
        protected bool hasColor = false;
        protected bool hasTexture = false;
        protected double xo, yo, zo;
        protected int drawMode;

        public int VertCount = 0;

        protected Tessellator() { }
        public unsafe void End()
        {
            if (vertices <= 0) return;

            fixed (int* buf = buffer) {
                IntPtr ptr = (IntPtr)buf;

                GL.glEnableClientState(GL.GL_VERTEX_ARRAY);
                GL.glVertexPointer(3, GL.GL_FLOAT, 24, ptr);
                if (hasTexture) {
                    GL.glEnableClientState(GL.GL_TEXTURE_COORD_ARRAY);
                    GL.glTexCoordPointer(2, GL.GL_FLOAT, 24, ptr + 12);
                }
                if (hasColor) {
                    GL.glEnableClientState(GL.GL_COLOR_ARRAY);
                    //GL.glColorPointer(4, GL.GL_FLOAT, 0, ptr);
                    GL.glColorPointer(4, GL.GL_UNSIGNED_BYTE, 24, ptr + 20);
                }

                GL.glDrawArrays(drawMode, 0, vertices);

                GL.glDisableClientState(GL.GL_VERTEX_ARRAY);
                if (hasTexture) GL.glDisableClientState(GL.GL_TEXTURE_COORD_ARRAY);
                if (hasColor) GL.glDisableClientState(GL.GL_COLOR_ARRAY);
            }

            Clear();
        }
        public void EndVBO(ref VBO vbo)
        {
            if (vbo == null) {
                vbo = new VBO(drawMode);
            }

            vbo.Update(buffer, bufferPos, vertices, hasTexture, hasColor);

            Clear();
        }

        protected void Clear()
        {
            VertCount = vertices;
            vertices = 0;
            bufferPos = 0;

            Color(1, 1, 1, 1);
            hasColor = false;
            hasTexture = false;
        }
        public void Begin()
        {
            Begin(GL.GL_QUADS);
        }
        public void Begin(int mode)
        {
            drawMode = mode;
            Clear();
            Color(1, 1, 1, 1);
            hasColor = false;
            hasTexture = false;
            VertCount = 0;
        }

        public void TexCoord(float u, float v)
        {
            hasTexture = true;
            this.u = u;
            this.v = v;
        }
        public void Color(float r, float g, float b)
        {
            Color(r, g, b, 1f);
        }
        public void Color(float r, float g, float b, float a)
        {
            hasColor = true;

            int ai = a < 0.0f ? 0 : a > 1.0f ? 255 : (int)(a * 255.0f);
            int ri = r < 0.0f ? 0 : r > 1.0f ? 255 : (int)(r * 255.0f);
            int gi = g < 0.0f ? 0 : g > 1.0f ? 255 : (int)(g * 255.0f);
            int bi = b < 0.0f ? 0 : b > 1.0f ? 255 : (int)(b * 255.0f);

            color = ai << 24 | bi << 16 | gi << 8 | ri;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int FloatBits(float f)
        {
            return *(int*)&f;
        }

        public void Vertex(double x, double y, double z)
        {
            buffer[bufferPos + 0] = FloatBits((float)(xo + x));
            buffer[bufferPos + 1] = FloatBits((float)(yo + y));
            buffer[bufferPos + 2] = FloatBits((float)(zo + z));
            if (hasTexture) {
                buffer[bufferPos + 3] = FloatBits(u);
                buffer[bufferPos + 4] = FloatBits(v);
            }
            if (hasColor) {
                buffer[bufferPos + 5] = color;
            }
            bufferPos += 6;

            if (vertices % 4 == 0 && bufferPos >= buffer.Length - 32) {
                Debug.WriteLine("Expanding tessellator buffer");
                ExpandBuffer();
            }
            vertices++;
        }
        private void ExpandBuffer()
        {
            int[] nbuf = new int[buffer.Length + 524288]; //expand to +512KiB
            Buffer.BlockCopy(buffer, 0, nbuf, 0, buffer.Length * 4);
            buffer = nbuf;
        }

        public void Translate(double x, double y, double z)
        {
            xo = x;
            yo = y;
            zo = z;
        }
    }
}
