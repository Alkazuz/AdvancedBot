using AdvancedBot.client;
using AdvancedBot.client.NBT;
using AdvancedBot.Viewer.Character;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdvancedBot.Viewer.Gui.ItemRenderer
{
    public class SkullRenderer : ItemRendererBase
    {
        private Cube head;
        private Dictionary<string, int> textureIds;

        public SkullRenderer()
        {
            head = new Cube(0, 0).AddBox(-4.0f, -8.0f, -4.0f, 8, 8, 8);
            textureIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        public override void Render(ItemStack stack, int x, int y)
        {
            int texId = GetSkullTexture(ViewForm.OpenForm, stack);
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glBindTexture(GL.GL_TEXTURE_2D, texId);

            //GuiUtils.DrawTexturedRect(x, y, 32, 32, 8, 8, 8, 8, 64, 32);
            DrawHeadOnGUI(x, y, 2);
        }

        private void DrawHeadOnGUI(int x, int y, float scale)
        {
            GL.glPushMatrix();
            GL.glTranslated(x + 8 * scale, y + 12 * scale, 16);
            GL.glScalef(-scale, -scale, scale);
            GL.glRotatef(210f, 1, 0, 0);
            GL.glRotatef(45f, 0, 1, 0);

            head.RenderNoTransform();

            GL.glColor3f(1f, 1f, 1f);
            GL.glPopMatrix();
        }

        public override void Dispose()
        {
            foreach (var tex in textureIds) {
                GL.glDeleteTexture(tex.Value);
            }
            textureIds.Clear();
        }

        const string DEFAULT_SKIN = "!Default";
        private int GetDefaultTextureId(ViewForm form)
        {
            if (!textureIds.TryGetValue(DEFAULT_SKIN, out int texId)) {
                texId = form.TexManager.Get("steve", false);
                textureIds[DEFAULT_SKIN] = texId;
            }
            return texId;
        }
        private int GetSkullTexture(ViewForm vfrm, ItemStack stack)
        {
            int texId = 0;
            try
            {
                string url = GetSkullUrl(stack);

                
                if (url == null)
                {
                    texId = GetDefaultTextureId(vfrm);
                }
                else if (!textureIds.TryGetValue(url, out texId))
                {
                    texId = GetDefaultTextureId(vfrm);
                    textureIds[url] = texId;

                    FireAndForget(DownloadSkinAsync(vfrm, url, url));
                }
            }catch(Exception ex) { }
            return texId;
        }
        private string GetSkullUrl(ItemStack stack)
        {
            try
            {
                var tag = stack.NBTData;
                if (tag == null)
                {
                    return null;
                }

                //SkullOwner/Properties/textures/Value

                var skullOwner = tag.GetCompound("SkullOwner");

                var name = skullOwner.GetString("Name");
                string id = skullOwner.GetString("Id");

                var textures = skullOwner.GetCompound("Properties").GetList("textures");
                if (textures.Count != 0)
                {
                    string base64 = ((CompoundTag)textures[0]).GetString("Value");
                    byte[] data = Convert.FromBase64String(base64);

                    try
                    {
                        var obj = JObject.Parse(Encoding.UTF8.GetString(data));
                        return obj["textures"]["SKIN"]["url"].AsStr();
                    }
                    catch
                    {

                    }
                }
                else if (!string.IsNullOrEmpty(name))
                {
                    return $"@{name}|{id}";
                }
                
            }catch(Exception ex) { }
            return null;
        }

        private async Task DownloadSkinAsync(ViewForm vfrm, string key, string url)
        {
            if (url[0] == '@') {
                int separator = url.IndexOf('|');
                string name = url.Substring(1, separator - 1);
                string id = url.Substring(separator + 1);

                if (id.Length == 0) {
                    var json = await GetJsonAsync($"https://api.mojang.com/users/profiles/minecraft/{name}");
                    id = json["id"].AsStr();
                }

                var profile = await GetJsonAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{id}");

                var texProp = profile["properties"].First(tok => tok["name"].AsStr() == "textures");

                var texBytes = Convert.FromBase64String(texProp["value"].AsStr());
                var tex = JObject.Parse(Encoding.UTF8.GetString(texBytes));

                url = tex["textures"]["SKIN"]["url"].AsStr();
            }
            byte[] imgData = await GetAsync(url);
            var bmp = new Bitmap(new MemoryStream(imgData));

            if (bmp.Height > 32) {
                var cropped = bmp.Clone(new Rectangle(0, 0, 64, 32), PixelFormat.Format32bppArgb);
                bmp.Dispose();
                bmp = cropped;
            }
            
            vfrm.InvokeOnGLThread(() => {
                //this is thread safe, as the render method should only be called by this same thread
                textureIds[key] = TextureManager.CreateTexture(bmp, false); //CreateTexture() also disposes the bitmap
            });
        }

        private async Task<byte[]> GetAsync(string url)
        {
            var req = CreateRequest(url);
            using (var resp = await req.GetResponseAsync())
            using (var stream = resp.GetResponseStream()) {
                byte[] buf = new byte[(int)resp.ContentLength];
                for (int rem = buf.Length; rem > 0;) {
                    int read = await stream.ReadAsync(buf, buf.Length - rem, rem);
                    if (read == 0) throw new EndOfStreamException();
                    rem -= read;
                }
                return buf;
            }
        }
        private async Task<JToken> GetJsonAsync(string url)
        {
            var req = CreateRequest(url);
            using (var resp = await req.GetResponseAsync())
            using (var stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8)) {
                return JToken.Parse(await stream.ReadToEndAsync());
            }
        }
        private static HttpWebRequest CreateRequest(string url)
        {
            var req = WebRequest.CreateHttp(url);
            req.UserAgent = "Java/1.8.0_66";
            req.Proxy = null;
            return req;
        }

        private void FireAndForget(Task task)
        {
            task.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
