using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.client;
using Newtonsoft.Json.Linq;

namespace AdvancedBot.Viewer.Model
{
    //Port of StevenGO
    public class ModelFactory
    {
        private ZipArchive zip;

        public ModelFactory(ZipArchive za)
        {
            zip = za;
        }
        public BlockStateModel LoadModel(string name)
        {
            var entry = zip.GetEntry($"assets/minecraft/blockstates/{name}.json");
            if (entry == null) {
                Debug.WriteLine("Missing block state for " + name);
                return null;
            }
            JObject blockstate = entry.ReadJson();

            BlockStateModel bsm = new BlockStateModel {
                Name = name
            };
            JToken variants = blockstate["variants"];
            if (variants != null) {
                foreach (JProperty vk in variants) {
                    bsm.Variants[vk.Name] = ParseModelList(vk.Value);
                }
            } else {
                foreach (JToken part in blockstate["multipart"]) {
                    BlockPart p = new BlockPart();

                    JToken when = part["when"];
                    if (when != null && when["OR"] != null) {
                        p.When = new MultipartConditionsEvaluator(when["OR"]);
                    } else if (when != null) {
                        p.When = new MultipartConditionEvaluator((JProperty)when.First);
                    } else {
                        //default
                    }
                    p.Apply = ParseModelList(part["apply"]);
                    bsm.Multiparts.Add(p);
                }
            }

            return bsm;
        }
        private List<BlockStateVariant> ParseModelList(JToken data)
        {
            List<BlockStateVariant> models = new List<BlockStateVariant>();
            if (data is JArray) {
                foreach (JToken k in data) {
                    models.Add(ParseBlockStateVariant(k));
                }
            } else {
                models.Add(ParseBlockStateVariant(data));
            }
            return models;
        }
        private BlockStateVariant ParseBlockStateVariant(JToken js)
        {
            JObject data = zip.GetEntry($"assets/minecraft/models/block/{js["model"].AsStr()}.json").ReadJson();
            BlockStateVariant bm = ParseModel(data);

            bm.Y = data["y"].AsDoubleOr(0);
            bm.X = data["x"].AsDoubleOr(0);
            bm.UVLock = data["uvlock"].AsBool();
            bm.Weight = data["weight"].AsIntOr(1);
            return bm;
        }
        private BlockStateVariant ParseModel(JToken data)
        {
            BlockStateVariant bm;

            JToken tmp;
            if ((tmp = data["parent"]) != null && !tmp.AsStr().StartsWith("builtin/")) {
                bm = ParseModel(zip.GetEntry($"assets/minecraft/models/{data["parent"].AsStr()}.json").ReadJson());
            } else {
                bm = new BlockStateVariant();
            }

            if ((tmp = data["textures"]) != null) {
                foreach (JProperty p in tmp) {
                    bm.Textures[p.Name] = p.Value.AsStr();
                }
            }
            if ((tmp = data["elements"]) != null) {
                foreach (JToken e in tmp) {
                    bm.Elements.Add(ParseBlockElement(e));
                }
            }

            bm.AmbientOcclusion = data["ambientocclusion"].AsBoolOrTrue();
            if ((tmp = data["display"]) != null) {
                foreach (JProperty kv in tmp) {
                    bm.Display[kv.Name] = kv.Value.ToObject<ModelDisplay>();
                }
            }

            return bm;
        }
        private BlockElement ParseBlockElement(JToken data)
        {
            BlockElement be = new BlockElement {
                From = data["from"].ToObject<float[]>(),
                To = data["to"].ToObject<float[]>()
            };

            JToken tmp;
            be.Shade = (tmp = data["shade"]) == null || tmp.AsBool();

            JToken faces;
            if ((faces = data["faces"]) != null) {
                foreach (Direction d in DirectionEx.Values) {
                    if ((tmp = faces[d.Name()]) != null) {
                        int i = (int)d;
                        be.Faces[i] = new BlockFace(tmp);
                        if (double.IsNaN(be.Faces[i].UV[0])) {
                            be.Faces[i].UV = new double[] { 0, 0, 16, 16 };
                            switch (d) {
                                case Direction.North:
                                case Direction.South:
                                    be.Faces[i].UV[0] = be.From[0];
                                    be.Faces[i].UV[2] = be.To[0];
                                    be.Faces[i].UV[1] = 16 - be.To[1];
                                    be.Faces[i].UV[3] = 16 - be.From[1];
                                    break;
                                case Direction.West:
                                case Direction.East:
                                    be.Faces[i].UV[0] = be.From[2];
                                    be.Faces[i].UV[2] = be.To[2];
                                    be.Faces[i].UV[1] = 16 - be.To[1];
                                    be.Faces[i].UV[3] = 16 - be.From[1];
                                    break;
                                case Direction.Down:
                                case Direction.Up:
                                    be.Faces[i].UV[0] = be.From[0];
                                    be.Faces[i].UV[2] = be.To[0];
                                    be.Faces[i].UV[1] = 16 - be.To[2];
                                    be.Faces[i].UV[3] = 16 - be.From[2];
                                    break;
                            }
                        }
                    }
                }
            }

            if ((tmp = data["rotation"]) != null) {
                ModelRotation r = be.Rotation = new ModelRotation();

                r.Origin = tmp["origin"].ToObjectOr<double[]>(new double[] { 8, 8, 8 });
                r.Axis = tmp["axis"].AsStr();
                r.Angle = tmp["angle"].AsDouble();
                r.Rescale = tmp["rescale"].AsBool();
            }

            return be;
        }
    }
    public interface IMultipartEvaluator
    {
        bool Evaluate(BlockState bs);
    }
    public class MultipartConditionEvaluator : IMultipartEvaluator
    {
        public KeyValuePair<string, string[]> Condition { get; }

        public MultipartConditionEvaluator(JProperty condition)
        {
            Condition = CompileCase(condition);
        }
        public bool Evaluate(BlockState bs)
        {
            return EvaluateWhenCase(bs, Condition.Key, Condition.Value);
        }

        public static bool EvaluateWhenCase(BlockState bs, string property, string[] cases)
        {
            if (bs.Properties.TryGetValue(property, out string prop)) {
                switch (cases.Length) {
                    case 1: return prop.EqualsIgnoreCase(cases[0]);
                    case 2: return prop.EqualsIgnoreCase(cases[0]) || prop.EqualsIgnoreCase(cases[1]);
                    default:
                        for (int i = 0; i < cases.Length; i++) {
                            if (prop.EqualsIgnoreCase(cases[i])) {
                                return true;
                            }
                        }
                        return false;
                }
            }
            Debug.WriteLine("Property not found for " + bs.Block.Name);
            return false;
        }
        public static KeyValuePair<string, string[]>[] CompileCondition(JObject jt)
        {
            var kvs = new KeyValuePair<string, string[]>[jt.Count];
            int i = 0;
            foreach (var kv in jt.Properties()) {
                kvs[i++] = CompileCase(kv);
            }
            return kvs;
        }
        public static KeyValuePair<string, string[]> CompileCase(JProperty kv)
        {
            var k = kv.Value.ToObject<string>();
            if (kv.Value.Type == JTokenType.Boolean) {
                k = k.ToLower();
            }
            return new KeyValuePair<string, string[]>(kv.Name, k.Split('|'));
        }
    }
    public class MultipartConditionsEvaluator : IMultipartEvaluator
    {
        public KeyValuePair<string, string[]>[][] Conditions { get; }

        public MultipartConditionsEvaluator(JToken or)
        {
            Conditions = or.Select(a => MultipartConditionEvaluator.CompileCondition((JObject)a)).ToArray();
        }
        public bool Evaluate(BlockState bs)
        {
            for (int i = 0; i < Conditions.Length; i++) {
                if (EvaluateWhenConditions(bs, Conditions[i])) {
                    return true;
                }
            }
            return false;
        }
        private static bool EvaluateWhenConditions(BlockState bs, KeyValuePair<string, string[]>[] cases)
        {
            for (int i = 0; i < cases.Length; i++) {
                var kv = cases[i];
                if (!MultipartConditionEvaluator.EvaluateWhenCase(bs, kv.Key, kv.Value)) {
                    return false;
                }
            }
            return true;
        }
    }

    public class BlockStateModel
    {
        public string Name;
        public Dictionary<string, List<BlockStateVariant>> Variants = new Dictionary<string, List<BlockStateVariant>>();
        public List<BlockPart> Multiparts = new List<BlockPart>();

        public override string ToString()
        {
            return string.Format("{0}, Variants={1}", Name, Variants.Count);
        }
    }
    public class BlockStateVariant
    {
        public string Model;
        public double X, Y;
        public bool UVLock;
        public int Weight;

        public Dictionary<string, string> Textures = new Dictionary<string, string>();
        public List<BlockElement> Elements = new List<BlockElement>();
        public bool AmbientOcclusion;
        public Dictionary<string, ModelDisplay> Display = new Dictionary<string, ModelDisplay>();

        public string ResolveTextureName(string name)
        {
            if (name.StartsWith("#")) {
                return ResolveTextureName(Textures[name.Substring(1)]);
            }
            return name;
        }
    }
    public class BlockPart
    {
        public IMultipartEvaluator When;
        public List<BlockStateVariant> Apply;
    }
    public class BlockElement
    {
        public float[] From, To;
        public bool Shade;
        public ModelRotation Rotation;
        public BlockFace[] Faces = new BlockFace[6];
    }
    public class ModelRotation
    {
        public double[] Origin;
        public string Axis;
        public double Angle;
        public bool Rescale;
    }
    public class BlockFace
    {
        public double[] UV;
        public string Texture;
        //textureInfo *render.TextureInfo
        public Direction CullFace;
        public int Rotation;
        public int TintIndex;

        public Vec3d[] Vertices;

        public BlockFace() { }
        public BlockFace(JToken data)
        {
            UV = data["uv"].ToObjectOr(new[] { double.NaN, 0, 0, 0 });
            Texture = data["texture"].AsStr();
            if (!Texture.StartsWith("#")) {
                Texture = "#" + Texture;
            }
            CullFace = DirectionEx.FromString(data["cullface"].AsStrOr(null));
            Rotation = data["rotation"].AsIntOr(0);
            TintIndex = data["tintindex"].AsIntOr(-1);
        }
    }
    public class ModelDisplay
    {
        public double[] Rotation;
        public double[] Translation;
        public double[] Scale;
    }
}
