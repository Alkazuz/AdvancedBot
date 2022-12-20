using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdvancedBot.Client;

namespace AdvancedBot.Viewer.Model
{
    public class ModelManager : IDisposable
    {
        private ZipArchive zip;
        private Dictionary<int, BlockStateModel> models = new Dictionary<int, BlockStateModel>();
        private ModelFactory factory;
        
        public ModelManager(ZipArchive za)
        {
            zip = za;
            factory = new ModelFactory(za);
        }
        public BlockStateVariant GetModelVariant(BlockState bs)
        {
            if(!models.TryGetValue(bs.Block.ID, out BlockStateModel model)) {
                model = factory.LoadModel(bs.Variant);
                models[bs.Block.ID] = model;
            }
            if(model.Multiparts.Count != 0) {
                return model.Multiparts.FirstOrDefault(a => a.When(bs)).Apply.FirstOrDefault();
            } else {
                return model.Variants.FirstOrDefault(a => a.Key.Split(',').All((p) => {
                    int n = p.IndexOf('=');
                    if(n == -1) {
                        return true;
                    }
                    string key = p.Substring(0, n);
                    return bs.Properties.TryGetValue(key, out string val) && val.EqualsIgnoreCase(p.Substring(n + 1));
                })).Value.FirstOrDefault();
            }
        }

        public void Dispose()
        {
            zip.Dispose();
        }
    }
}
