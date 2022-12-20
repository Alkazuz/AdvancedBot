using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.Client;
using AdvancedBot.Client.Map;

namespace AdvancedBot.Viewer.Model
{
    public class ModelChunkRenderer
    {
        public VBO vbo;

        public World world;
        public int x, y, z;
        private bool built;

        private static ModelManager modelMgr = new ModelManager(new ZipArchive(new FileStream(@"C:\Users\Daniel\Desktop\Arquivos2\bot_src\Default_111.zip", FileMode.Open)));
        private static ModelRenderer renderer = new ModelRenderer(modelMgr);
        public ModelChunkRenderer(World w, int x, int y, int z)
        {
            world = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void Rebuild()
        {
            Tessellator t = Tessellator.Instance;
            t.Begin();
            Tessellate(t);
            vbo = t.EndVBO(vbo);

            built = true;
        }
        public void Render()
        {
            if (built) {
                vbo.Render();
            }
        }
        public void Delete()
        {
            if (vbo != null) {
                vbo.Dispose();
                vbo = null;
            }
            built = false;
        }

        public void Tessellate(Tessellator t)
        {
            int cx = x * 16;
            int cy = y * 16;
            int cz = z * 16;

            Chunk c = world.GetChunk(x, z);
            bool tex = ViewForm.UseTexture;

            ChunkSection sec = c?.Sections[y];
            if (sec != null) {
                for (int by = 0; by < 16; by++) {
                    for (int bz = 0; bz < 16; bz++) {
                        for (int bx = 0; bx < 16; bx++) {
                            int idx = by << 8 | bz << 4 | bx;
                            int id = sec.Blocks[idx];
                            if (id != Blocks.air && id != Blocks.barrier) {
                                byte b = sec.Metadata[idx / 2];
                                int data = (idx & 1) == 0 ? b & 0x0F : (b >> 4) & 0x0F;

                                //renderModel(t, cx + bx, cy + by, cz + bz, id, data);
                            }
                        }
                    }
                }
            }
        }
        
    }
}
