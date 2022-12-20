using AdvancedBot.client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer.Gui.ItemRenderer
{
    public abstract class ItemRendererBase : IDisposable
    {
        public abstract void Render(ItemStack stack, int x, int y);
        public virtual void Tick() { }
        public abstract void Dispose();

        private static SkullRenderer skullRenderer;
        private static BannerRenderer bannerRenderer;

        public static ItemRendererBase GetRenderer(ItemStack stack)
        {
            if (stack.ID == Items.skull && stack.Metadata == 3) {
                return skullRenderer;
            } else if (stack.ID == Items.banner) {
                return bannerRenderer;
            }
            return null;
        }

        public static void Initialize()
        {
            skullRenderer = new SkullRenderer();
            bannerRenderer = new BannerRenderer();
        }
        public static void NotifyTick()
        {
            skullRenderer.Tick();
            bannerRenderer.Tick();
        }
        public static void CleanUp()
        {
            skullRenderer.Dispose();
            bannerRenderer.Dispose();
        }
    }
}
