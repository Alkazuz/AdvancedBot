using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.client;
using AdvancedBot.client.Map;

namespace AdvancedBot.Viewer.Model
{
    public class ModelRenderer : IDisposable
    {
        private Dictionary<int, ProcessedModel> models = new Dictionary<int, ProcessedModel>();

        private ModelFactory modelFactory;
        private BlockStateModel missingNoModel;
        private BlockStateVariant missingNo;

        public ModelTextureManager TexManager { get; }
        public ZipArchive ResourcePack { get; }

        public ModelRenderer(ZipArchive resPack)
        {
            ResourcePack = resPack;
            modelFactory = new ModelFactory(resPack);
            TexManager = new ModelTextureManager(resPack);

            missingNo = modelFactory.LoadModel("stone").Variants.First().Value[0];
            missingNo.Textures["all"] = "missingno";

            missingNoModel = new BlockStateModel() {
                Name = "missingno",
                Variants = new Dictionary<string, List<BlockStateVariant>>() {
                    ["default"] = new List<BlockStateVariant> { missingNo }
                }
            };
        }
        private ProcessedModel PrecomputeModel(BlockStateVariant bm)
        {
            ProcessedModel p = new ProcessedModel {
                AmbientOcclusion = bm.AmbientOcclusion,
                Weight = bm.Weight
            };
            foreach (BlockElement el in bm.Elements) {
                for (int i = 0; i < 6; i++) {
                    BlockFace face = el.Faces[i];
                    if (face == null) {
                        continue;
                    }
                    Direction faceID = DirectionEx.Values[i];
                    ProcessedFace pFace = new ProcessedFace();
                    Direction cullFace = face.CullFace;
                    if (bm.X > 0) {
                        int o = (int)bm.X / 90;
                        cullFace = RotateDirection(cullFace, o, faceRotationX, Direction.East, Direction.West, Direction.Invalid);
                        faceID = RotateDirection(faceID, o, faceRotationX, Direction.East, Direction.West, Direction.Invalid);
                    }
                    if (bm.Y > 0) {
                        int o = (int)bm.Y / 90;
                        cullFace = RotateDirection(cullFace, o, faceRotation, Direction.Up, Direction.Down, Direction.Invalid);
                        faceID = RotateDirection(faceID, o, faceRotation, Direction.Up, Direction.Down, Direction.Invalid);
                    }
                    pFace.CullFace = cullFace;
                    pFace.Facing = faceID;
                    pFace.TintIndex = face.TintIndex;
                    pFace.Shade = el.Shade;

                    FaceDetails vert = CreateFaceDetails(faceID);

                    string resolved = bm.ResolveTextureName(face.Texture);
                    Rectangle rect = TexManager.GetTexture(resolved);

                    int ux1 = (int)(face.UV[0] * rect.Width);
                    int ux2 = (int)(face.UV[2] * rect.Width);
                    int uy1 = (int)(face.UV[1] * rect.Height);
                    int uy2 = (int)(face.UV[3] * rect.Height);

                    int tw = rect.Width;
                    int th = rect.Height;
                    if (face.Rotation > 0) {
                        int x = ux1;
                        int y = uy1;
                        int w = ux2 - ux1;
                        int h = uy2 - uy1;
                        switch (face.Rotation) {
                            case 90:
                                uy2 = x + w;
                                ux1 = tw * 16 - (y + h);
                                ux2 = tw * 16 - y;
                                uy1 = x;
                                break;
                            case 180:
                                uy1 = th * 16 - (y + h);
                                uy2 = th * 16 - y;
                                ux1 = x + w;
                                ux2 = x;
                                break;
                            case 270:
                                uy2 = x;
                                uy1 = x + w;
                                ux2 = y + h;
                                ux1 = y;
                                break;
                        }
                    }

                    float minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity;
                    float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity, maxZ = float.NegativeInfinity;

                    bm.ResolveTextureName(face.Texture);

                    for (int vi = 0; vi < vert.Verts.Length; vi++) {
                        var v = vert.Verts[vi];

                        if (v.X == 0) {
                            v.X = (float)(el.From[0] / 16.0);
                        } else {
                            v.X = (float)(el.To[0] / 16.0);
                        }
                        if (v.Y == 0) {
                            v.Y = (float)(el.From[1] / 16.0);
                        } else {
                            v.Y = (float)(el.To[1] / 16.0);
                        }
                        if (v.Z == 0) {
                            v.Z = (float)(el.From[2] / 16.0);
                        } else {
                            v.Z = (float)(el.To[2] / 16.0);
                        }

                        double c, s, x, y, z;
                        if (el.Rotation != null) {
                            ModelRotation r = el.Rotation;
                            switch (r.Axis) {
                                case "y":
                                    double rotY = -r.Angle * (Math.PI / 180);
                                    c = Math.Cos(rotY);
                                    s = Math.Sin(rotY);
                                    x = v.X - (r.Origin[0] / 16.0);
                                    z = v.Z - (r.Origin[2] / 16.0);
                                    v.X = (float)(r.Origin[0] / 16.0 + (x * c - z * s));
                                    v.Z = (float)(r.Origin[2] / 16.0 + (z * c + x * s));
                                    break;
                                case "x":
                                    double rotX = r.Angle * (Math.PI / 180);
                                    c = Math.Cos(-rotX);
                                    s = Math.Sin(-rotX);
                                    z = v.Z - (r.Origin[2] / 16.0);
                                    y = v.Y - (r.Origin[1] / 16.0);
                                    v.Z = (float)(r.Origin[2] / 16.0 + (z * c - y * s));
                                    v.Y = (float)(r.Origin[1] / 16.0 + (y * c + z * s));
                                    break;
                                case "z":
                                    double rotZ = -r.Angle * (Math.PI / 180);
                                    c = Math.Cos(-rotZ);
                                    s = Math.Sin(-rotZ);
                                    x = v.X - (r.Origin[0] / 16.0);
                                    y = v.Y - (r.Origin[1] / 16.0);
                                    v.X = (float)(r.Origin[0] / 16.0 + (x * c - y * s));
                                    v.Y = (float)(r.Origin[1] / 16.0 + (y * c + x * s));
                                    break;
                            }
                        }

                        if (bm.X > 0) {
                            double rotX = bm.X * (Math.PI / 180);
                            c = Math.Cos(rotX);
                            s = Math.Sin(rotX);
                            z = v.Z - 0.5;
                            y = v.Y - 0.5;
                            v.Z = (float)(0.5 + (z * c - y * s));
                            v.Y = (float)(0.5 + (y * c + z * s));
                        }

                        if (bm.Y > 0) {
                            double rotY = bm.Y * (Math.PI / 180);
                            c = Math.Cos(rotY);
                            s = Math.Sin(rotY);
                            x = v.X - 0.5;
                            z = v.Z - 0.5;
                            v.X = (float)(0.5 + (x * c - z * s));
                            v.Z = (float)(0.5 + (z * c + x * s));
                        }

                        if (v.TOffsetX == 0) {
                            v.TOffsetX = (short)ux1;
                        } else {
                            v.TOffsetX = (short)ux2;
                        }
                        if (v.TOffsetY == 0) {
                            v.TOffsetY = (short)uy1;
                        } else {
                            v.TOffsetY = (short)uy2;
                        }

                        if (face.Rotation > 0) {
                            double rotY = -face.Rotation * (Math.PI / 180);
                            c = (short)(Math.Cos(rotY));
                            s = (short)(Math.Sin(rotY));
                            x = v.TOffsetX - 8 * tw;
                            y = v.TOffsetY - 8 * th;
                            v.TOffsetX = (short)(8 * tw + (x * c - y * s));
                            v.TOffsetY = (short)(8 * th + (y * c + x * s));
                        }

                        if (bm.UVLock && bm.Y > 0 && (pFace.Facing == Direction.Up || pFace.Facing == Direction.Down)) {
                            double rotY = -bm.Y * (Math.PI / 180);
                            c = (short)(Math.Cos(rotY));
                            s = (short)(Math.Sin(rotY));
                            x = v.TOffsetX - 8 * 16;
                            y = v.TOffsetY - 8 * 16;
                            v.TOffsetX = (short)(8 * 16 + (x * c + y * s));
                            v.TOffsetY = (short)(8 * 16 + (y * c - x * s));
                        }

                        if (bm.UVLock && bm.X > 0 && (pFace.Facing != Direction.Up && pFace.Facing != Direction.Down)) {
                            double rotY = bm.X * (Math.PI / 180);
                            c = (short)(Math.Cos(rotY));
                            s = (short)(Math.Sin(rotY));
                            x = v.TOffsetX - 8 * 16;
                            y = v.TOffsetY - 8 * 16;
                            v.TOffsetX = (short)(8 * 16 + (x * c + y * s));
                            v.TOffsetY = (short)(8 * 16 + (y * c - x * s));
                        }

                        v.U = (v.TOffsetX / 16.0f + rect.X) / TexManager.Width;
                        v.V = (v.TOffsetY / 16.0f + rect.Y) / TexManager.Height;


                        if ((el.Rotation != null && el.Rotation.Rescale)) {
                            if ((v.X < minX)) {
                                minX = v.X;
                            } else if ((v.X > maxX)) {
                                maxX = v.X;
                            }
                            if (v.Y < minY) {
                                minY = v.Y;
                            } else if (v.Y > maxY) {
                                maxY = v.Y;
                            }
                            if (v.Z < minZ) {
                                minZ = v.Z;
                            } else if (v.Z > maxZ) {
                                maxZ = v.Z;
                            }
                        }
                    }

                    if (el.Rotation != null && el.Rotation.Rescale) {
                        float diffX = maxX - minX;
                        float diffY = maxY - minY;
                        float diffZ = maxZ - minZ;
                        for (int vi = 0; vi < vert.Verts.Length; vi++) {
                            var v = vert.Verts[vi];
                            v.X = (v.X - minX) / diffX;
                            v.Y = (v.Y - minY) / diffY;
                            v.Z = (v.Z - minZ) / diffZ;
                        }
                    }

                    pFace.Vertices = vert.Verts;

                    p.Faces.Add(pFace);
                }
            }
            return p;
        }

        public BlockStateVariant LoadModelVariant(BlockState bs)
        {
            BlockStateModel model = modelFactory.LoadModel(bs.Variant);

            if(model == null) {
                return missingNo;
            }

            BlockStateVariant variant = null;
            if (model.Multiparts.Count != 0) {
                List<BlockStateVariant> varDefault = null;
                foreach (var mp in model.Multiparts) {
                    if (mp.When == null) {
                        varDefault = mp.Apply;
                    } else if (mp.When.Evaluate(bs)) {
                        variant = mp.Apply.FirstOrDefault();
                        break;
                    }
                }
                if (variant == null && varDefault != null) {
                    variant = varDefault.FirstOrDefault();
                }
            } else {
                var varList = model.Variants.FirstOrDefault(a => a.Key.Split(',').All((p) => {
                    int n = p.IndexOf('=');
                    if (n == -1) {
                        return true;
                    }
                    string key = p.Substring(0, n);
                    return bs.Properties.TryGetValue(key, out string val) && 
                           val.EqualsIgnoreCase(p.Substring(n + 1));
                }));
                variant = varList.Value == null ? model.Variants.FirstOrDefault().Value.FirstOrDefault() : varList.Value.FirstOrDefault();
            }
            return variant ?? missingNo;
        }
        public ProcessedModel GetModel(BlockState b)
        {
            if (!models.TryGetValue(b.StateID, out ProcessedModel m)) {
                m = PrecomputeModel(LoadModelVariant(b));
                models[b.StateID] = m;
            }

            return m;
        }

        private BlockState GetBlockState(World w, int x, int y, int z)
        {
            w.GetBlockAndData(x, y, z, out byte id, out byte data);
            return BlockState.FromID(id << 4 | data);
        }

        private static readonly int[] QuadIndices = { 0, 1, 2, 3, 2, 1 };
        public void Render(Tessellator t, int x, int y, int z, World w)
        {
            int cx = x & 0xF;
            int cz = z & 0xF;

            var b = GetBlockState(w, x, y, z);
            ProcessedModel p = GetModel(b);
            foreach (var f in p.Faces) {
                if (f.CullFace != Direction.Invalid && ShouldCullAgainst(w, b, x, y, z, f.CullFace)) {
                    continue;
                }

                Colorf c = new Colorf(1f, 1f, 1f);
                if (f.TintIndex == 0) {
                    c = GetTint(b);
                }

                float br = 1.0f - ((int)f.Facing / 2) * 0.2f;
                t.Color(c.R * br, c.G * br, c.B * br);

                for (int i = 5; i >= 0; i--) {
                    ChunkVertex cv = f.Vertices[QuadIndices[i]];
                    t.TexCoord(cv.U, cv.V);
                    t.Vertex(cv.X + cx, cv.Y + y, cv.Z + cz);
                }
            }
        }

        public void RenderLiquid(Tessellator t, int x, int y, int z, World bs)
        {
            int b1, b2;

            BlockState b = GetBlockState(bs, x, y, z);
            if (b.BlockID == Blocks.lava || b.BlockID == Blocks.flowing_lava) {
                b1 = Blocks.lava;
                b2 = Blocks.flowing_lava;
            } else {
                b1 = Blocks.water;
                b2 = Blocks.flowing_water;
            }

            int tl, tr, bl, br;

            BlockState bUp = GetBlockState(bs, x, y + 1, z);
            if (bUp.BlockID == b1 || bUp.BlockID == b2) {
                tl = 8;
                tr = 8;
                bl = 8;
                br = 8;
            } else {
                tl = AverageLiquidLevel(bs, x, y, z, b.BlockID);
                tr = AverageLiquidLevel(bs, x + 1, y, z, b.BlockID);
                bl = AverageLiquidLevel(bs, x, y, z + 1, b.BlockID);
                br = AverageLiquidLevel(bs, x + 1, y, z + 1, b.BlockID);
            }

            var rect = TexManager.GetTexture(b1 == Blocks.water ? "blocks/water_still" :
                                                                  "blocks/lava_still");
            t.Color(1.0f, 1.0f, 1.0f);

            int tw = TexManager.Width;
            int th = TexManager.Height;

            int cx = x & 0xF;
            int cz = z & 0xF;

            for (int f = 0; f < 6; f++) {
                Direction d = (Direction)f;
                Vec3i o = d.Offset();

                bool special = d == Direction.Up && (tl < 8 || tr < 8 || bl < 8 || br < 8);

                int bo = bs.GetBlock(x + o.X, y + o.Y, z + o.Z);
                if (special || (bo != b1 && bo != b2 && !b.Block.ShouldCullAgainst())) {
                    var vert = CreateFaceDetails(d);

                    short ux1 = 0;
                    short ux2 = (short)(16 * rect.Width);
                    short uy1 = 0;
                    short uy2 = (short)(16 * rect.Height);

                    for (int vi = 0; vi < vert.Verts.Length; vi++) {
                        var v = vert.Verts[vi];
                        if (v.Y != 0) {
                            int height;
                            if (v.X == 0 && v.Z == 0) {
                                height = (int)((16.0f / 8.0f) * tl);
                            } else if (v.X != 0 && v.Z == 0) {
                                height = (int)((16.0f / 8.0f) * tr);
                            } else if (v.X == 0 && v.Z != 0) {
                                height = (int)((16.0f / 8.0f) * bl);
                            } else {
                                height = (int)((16.0f / 8.0f) * br);
                            }
                            v.Y = height / 16.0f;
                        }

                        if (v.TOffsetX == 0) {
                            v.TOffsetX = ux1;
                        } else {
                            v.TOffsetX = ux2;
                        }
                        if (v.TOffsetY == 0) {
                            v.TOffsetY = uy1;
                        } else {
                            v.TOffsetY = uy2;
                        }
                    }
                    for (int i = 5; i >= 0; i--) {
                        ChunkVertex cv = vert.Verts[QuadIndices[i]];
                        t.TexCoord((cv.TOffsetX / 16.0f + rect.X) / tw, (cv.TOffsetY / 16.0f + rect.Y) / th);
                        t.Vertex(cv.X + cx, cv.Y + y, cv.Z + cz);
                    }
                }
            }
        }
        private int AverageLiquidLevel(World w, int x, int y, int z, int lId)
        {
            int level = 0;
            for (int xx = -1; xx < 1; xx++) {
                for (int zz = -1; zz < 1; zz++) {
                    byte b = w.GetBlock(x + xx, y + 1, z + zz);

                    if (Blocks.IsLiquid(b) && IsLava(b) == IsLava(lId)) {
                        return 8;
                    }
                    w.GetBlockAndData(x + xx, y, z + zz, out b, out byte data);
                    if (Blocks.IsLiquid(b) && IsLava(b) == IsLava(lId)) {
                        int nl = 7 - (data & 0x7);
                        if (nl > level) {
                            level = nl;
                        }
                    }
                }
            }
            return level;
        }

        private bool IsLava(int id) => id == Blocks.lava || id == Blocks.flowing_lava;

        private bool ShouldCullAgainst(World w, BlockState bs, int x, int y, int z, Direction cullFace)
        {
            var o = cullFace.Offset();
            var bc = GetBlockState(w, x + o.X, y + o.Y, z + o.Z);
            if ((bc.Block.ShouldCullAgainst() || bc.Block.ID == bs.Block.ID) && 
                !(bc.Block.ID == Blocks.leaves || bc.Block.ID == Blocks.leaves2)) 
            {
                return true;
            }
            return false;
        }

        private static Colorf GetTint(BlockState bs)
        {
            switch(bs.Block.ID) {
                case Blocks.leaves: {
                    switch(bs.Data & 0x3) {
                        case 1: return new Colorf(0.38f, 0.60f, 0.38f);  //Pine
                        case 2: return new Colorf(0.50f, 0.65f, 0.33f);  //Birch
                        default: return new Colorf(0.28f, 0.71f, 0.09f); //Oak
                    }
                }
                case Blocks.leaves2: return new Colorf(0.28f, 0.71f, 0.09f); //Oak
                case Blocks.melon_stem:
                case Blocks.pumpkin_stem:
                case Blocks.double_plant:
                case Blocks.tallgrass:
                case Blocks.grass: return new Colorf(0.51f, 0.76f, 0.27f);
                case Blocks.redstone_wire: {
                    float power = bs.Data / 15f;
                    float r = bs.Data == 0 ? 0.3f : power * 0.6f + 0.4f;
                    float g = power * power * 0.7f - 0.5f;
                    float b = power * power * 0.6f - 0.7f;

                    return new Colorf(r, g, b);
                }
                default: return new Colorf(1f, 1f, 1f);
            }
        }
        private struct Colorf
        {
            public float R, G, B;

            public Colorf(float r, float g, float b)
            {
                R = r > 1.0f ? 1.0f : r < 0.0f ? 0.0f : r;
                G = g > 1.0f ? 1.0f : g < 0.0f ? 0.0f : g;
                B = b > 1.0f ? 1.0f : b < 0.0f ? 0.0f : b;
            }
        }

        public void Dispose()
        {
            TexManager.Dispose();
            ResourcePack.Dispose();
        }

        #region stuff
        private static Direction[] faceRotation = {
            Direction.North,
            Direction.East,
            Direction.South,
            Direction.West,
        };
        private static Direction[] faceRotationX = {
            Direction.North,
            Direction.Down,
            Direction.South,
            Direction.Up,
        };
        private static Direction RotateDirection(Direction val, int offset, Direction[] rots, params Direction[] invalid)
        {
            foreach (var d in invalid) {
                if (d == val) {
                    return val;
                }
            }
            int pos = 0;
            for (int di = 0; di < rots.Length; di++) {
                if (rots[di] == val) {
                    pos = di;
                    break;
                }
            }
            return rots[(pos + offset) % rots.Length];
        }
        private class FaceDetails
        {
            public ChunkVertex[] Verts;
        }
        private static FaceDetails CreateFaceDetails(Direction dir)
        {
            switch (dir) {
                case Direction.Down:
                    return new FaceDetails() {
                        Verts = new ChunkVertex[] {
                            new ChunkVertex() {X= 0, Y= 0, Z= 0, TOffsetX= 0, TOffsetY= 1},
                            new ChunkVertex() {X= 0, Y= 0, Z= 1, TOffsetX= 0, TOffsetY= 0},
                            new ChunkVertex() {X= 1, Y= 0, Z= 0, TOffsetX= 1, TOffsetY= 1},
                            new ChunkVertex() {X= 1, Y= 0, Z= 1, TOffsetX= 1, TOffsetY= 0},
                        }
                    };
                case Direction.Up:
                    return new FaceDetails() {
                        Verts = new ChunkVertex[] {
                            new ChunkVertex() {X= 0, Y= 1, Z= 0, TOffsetX= 0, TOffsetY= 0},
                            new ChunkVertex() {X= 1, Y= 1, Z= 0, TOffsetX= 1, TOffsetY= 0},
                            new ChunkVertex() {X= 0, Y= 1, Z= 1, TOffsetX= 0, TOffsetY= 1},
                            new ChunkVertex() {X= 1, Y= 1, Z= 1, TOffsetX= 1, TOffsetY= 1}
                        }
                    };
                case Direction.North:
                    return new FaceDetails() {
                        Verts = new ChunkVertex[] {
                            new ChunkVertex() {X= 0, Y= 0, Z= 0, TOffsetX= 1, TOffsetY= 1},
                            new ChunkVertex() {X= 1, Y= 0, Z= 0, TOffsetX= 0, TOffsetY= 1},
                            new ChunkVertex() {X= 0, Y= 1, Z= 0, TOffsetX= 1, TOffsetY= 0},
                            new ChunkVertex() {X= 1, Y= 1, Z= 0, TOffsetX= 0, TOffsetY= 0},
                        }
                    };
                case Direction.South:
                    return new FaceDetails() {
                        Verts = new ChunkVertex[] {
                            new ChunkVertex() {X= 0, Y= 0, Z= 1, TOffsetX= 0, TOffsetY= 1},
                            new ChunkVertex() {X= 0, Y= 1, Z= 1, TOffsetX= 0, TOffsetY= 0},
                            new ChunkVertex() {X= 1, Y= 0, Z= 1, TOffsetX= 1, TOffsetY= 1},
                            new ChunkVertex() {X= 1, Y= 1, Z= 1, TOffsetX= 1, TOffsetY= 0},
                        }
                    };
                case Direction.West:
                    return new FaceDetails() {
                        Verts = new ChunkVertex[] {
                            new ChunkVertex() {X= 0, Y= 0, Z= 0, TOffsetX= 0, TOffsetY= 1},
                            new ChunkVertex() {X= 0, Y= 1, Z= 0, TOffsetX= 0, TOffsetY= 0},
                            new ChunkVertex() {X= 0, Y= 0, Z= 1, TOffsetX= 1, TOffsetY= 1},
                            new ChunkVertex() {X= 0, Y= 1, Z= 1, TOffsetX= 1, TOffsetY= 0},
                        }
                    };
                case Direction.East:
                    return new FaceDetails() {
                        Verts = new ChunkVertex[] {
                            new ChunkVertex() {X= 1, Y= 0, Z= 0, TOffsetX= 1, TOffsetY= 1},
                            new ChunkVertex() {X= 1, Y= 0, Z= 1, TOffsetX= 0, TOffsetY= 1},
                            new ChunkVertex() {X= 1, Y= 1, Z= 0, TOffsetX= 1, TOffsetY= 0},
                            new ChunkVertex() {X= 1, Y= 1, Z= 1, TOffsetX= 0, TOffsetY= 0},
                        }
                    };
                default: return null;
            }
        }
        #endregion
    }

    public class ProcessedModel
    {
        public List<ProcessedFace> Faces = new List<ProcessedFace>();
        public bool AmbientOcclusion;
        public int Weight;
    }
    public class ProcessedFace
    {
        public Direction CullFace;
        public Direction Facing;
        public ChunkVertex[] Vertices;
        public string Texture;
        public bool Shade;
        public int TintIndex;
    }
    public class ChunkVertex
    {
        public float X, Y, Z;
        public float U, V;
        public short TOffsetX, TOffsetY;
    }
}
