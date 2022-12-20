using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer.Character
{
    public class Polygon
    {
        public Vertex[] Vertices;

        public Polygon(Vertex[] vertices)
        {
            Vertices = vertices;
        }

        public Polygon(Vertex[] vertices, float u0, float v0, float u1, float v1)
             : this(vertices)
        {
            vertices[0] = vertices[0].Remap(u1, v0);
            vertices[1] = vertices[1].Remap(u0, v0);
            vertices[2] = vertices[2].Remap(u0, v1);
            vertices[3] = vertices[3].Remap(u1, v1);
        }

        public void Render()
        {
            for (int i = 3; i >= 0; i--) {
                Vertex v = Vertices[i];
                GL.glTexCoord2f(v.U, v.V);
                GL.glVertex3f(v.X, v.Y, v.Z);
            }
        }
    }
}
