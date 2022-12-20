using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace AdvancedBot.Viewer
{
    public class VBO : IDisposable
    {
        public int Id;
        public int Vertices;
        private int drawMode;
        private bool hasTex, hasColor;

        public VBO(int mode) { drawMode = mode; }

        public unsafe void Update(int[] buffer, int bufferLen, int vertCount, bool hasTex, bool hasColor)
        {
            if (vertCount <= 0) return;
            Vertices = vertCount;
            this.hasTex = hasTex;
            this.hasColor = hasColor;

            if (Id == 0) {
                GL.glGenBuffersARB(1, out Id);
            }
            fixed (int* buf = buffer) {
                GL.glBindBufferARB(GL.GL_ARRAY_BUFFER_ARB, Id);
                GL.glBufferDataARB(GL.GL_ARRAY_BUFFER_ARB, (IntPtr)(bufferLen * 4), (IntPtr)buf, GL.GL_DYNAMIC_DRAW_ARB);
                GL.glBindBufferARB(GL.GL_ARRAY_BUFFER_ARB, 0);
            }
        }
        public void Render()
        {
            if (Id != 0) {
                GL.glBindBufferARB(GL.GL_ARRAY_BUFFER_ARB, Id);

                GL.glEnableClientState(GL.GL_VERTEX_ARRAY);
                GL.glVertexPointer(3, GL.GL_FLOAT, 24, IntPtr.Zero);
                if (hasTex) {
                    GL.glEnableClientState(GL.GL_TEXTURE_COORD_ARRAY);
                    GL.glTexCoordPointer(2, GL.GL_FLOAT, 24, (IntPtr)12);
                }
                if (hasColor) {
                    GL.glEnableClientState(GL.GL_COLOR_ARRAY);
                    GL.glColorPointer(4, GL.GL_UNSIGNED_BYTE, 24, (IntPtr)20);
                }

                GL.glDrawArrays(drawMode, 0, Vertices);

                GL.glDisableClientState(GL.GL_VERTEX_ARRAY);
                if (hasTex) GL.glDisableClientState(GL.GL_TEXTURE_COORD_ARRAY);
                if (hasColor) GL.glDisableClientState(GL.GL_COLOR_ARRAY);
            }
        }
        public void Delete()
        {
            if (Id != 0) {
                GL.glDeleteBuffersARB(1, ref Id);
                Id = 0;
            }
        }
        public void Dispose()
        {
            Delete();
        }
    }
}
