using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer.Character
{
    public class Cube
    {
        private Vertex[] vertices;
        private Polygon[] polygons;
        public int TexX;
        public int TexY;
        public int TexW;
        public int TexH;
        public float X;
        public float Y;
        public float Z;
        public float RotX;
        public float RotY;
        public float RotZ;

        public Cube(int xTexOffs, int yTexOffs) : this(xTexOffs, yTexOffs, 64, 32)
        {
        }
        public Cube(int xTexOffs, int yTexOffs, int texWidth, int texHeight)
        {
            TexX = xTexOffs;
            TexY = yTexOffs;
            TexW = texWidth;
            TexH = texHeight;
        }

        public Cube AddBox(float x0, float y0, float z0, int w, int h, int d)
        {
            vertices = new Vertex[8];
            polygons = new Polygon[6];
            float x = x0 + w;
            float y = y0 + h;
            float z = z0 + d;
            Vertex u0 = new Vertex(x0, y0, z0, 0.0f, 0.0f);
            Vertex u2 = new Vertex(x, y0, z0, 0.0f, 8.0f);
            Vertex u3 = new Vertex(x, y, z0, 8.0f, 8.0f);
            Vertex u4 = new Vertex(x0, y, z0, 8.0f, 0.0f);
            Vertex l0 = new Vertex(x0, y0, z, 0.0f, 0.0f);
            Vertex l2 = new Vertex(x, y0, z, 0.0f, 8.0f);
            Vertex l3 = new Vertex(x, y, z, 8.0f, 8.0f);
            Vertex l4 = new Vertex(x0, y, z, 8.0f, 0.0f);
            vertices[0] = u0;
            vertices[1] = u2;
            vertices[2] = u3;
            vertices[3] = u4;
            vertices[4] = l0;
            vertices[5] = l2;
            vertices[6] = l3;
            vertices[7] = l4;

            int tx = TexX;
            int ty = TexY;
            float tw = TexW;
            float th = TexH;

            polygons[0] = new Polygon(new Vertex[] { l2, u2, u3, l3 }, u0: (tx + d + w) / tw,      v0: (ty + d) / th,  u1: (tx + d + w + d) / tw,     v1: (ty + d + h) / th);
            polygons[1] = new Polygon(new Vertex[] { u0, l0, l4, u4 }, u0: (tx + 0) / tw,          v0: (ty + d) / th,  u1: (tx + d) / tw,             v1: (ty + d + h) / th);
            polygons[2] = new Polygon(new Vertex[] { l2, l0, u0, u2 }, u0: (tx + d) / tw,          v0: (ty + 0) / th,  u1: (tx + d + w) / tw,         v1: (ty + d) / th);
            polygons[3] = new Polygon(new Vertex[] { u3, u4, l4, l3 }, u0: (tx + d + w) / tw,      v0: (ty + 0) / th,  u1: (tx + d + w + w) / tw,     v1: (ty + d) / th);
            polygons[4] = new Polygon(new Vertex[] { u2, u0, u4, u3 }, u0: (tx + d) / tw,          v0: (ty + d) / th,  u1: (tx + d + w) / tw,         v1: (ty + d + h) / th);
            polygons[5] = new Polygon(new Vertex[] { l0, l2, l3, l4 }, u0: (tx + d + w + d) / tw,  v0: (ty + d) / th,  u1: (tx + d + w + d + w) / tw, v1: (ty + d + h) / th);

            return this;
        }
        public Cube SetPos(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;

            return this;
        }

        public void Render()
        {
            const float c = 180.0f / (float)Math.PI;
            GL.glPushMatrix();
            GL.glTranslated(X, Y, Z);
            GL.glRotatef(RotZ * c, 0.0f, 0.0f, 1.0f);
            GL.glRotatef(RotY * c, 0.0f, 1.0f, 0.0f);
            GL.glRotatef(RotX * c, 1.0f, 0.0f, 0.0f);
            RenderNoTransform();
            GL.glPopMatrix();
        }
        public void RenderNoTransform()
        {
            GL.glBegin(GL.GL_QUADS);
            for (int i = 0; i < polygons.Length; i++) {
                float br = 1.0f - (i / 2 * 0.2f);
                GL.glColor3f(br, br, br);
                polygons[i].Render();
            }
            GL.glEnd();
        }
    }
}
