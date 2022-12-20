using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Drawing;

namespace AdvancedBot.Viewer.Gui
{
    public class GuiOptions : GuiBase
    {
        private GuiCheckBox cbUseTextures;
        private GuiTrackBar tbChunkRenderDist;
        private GuiCheckBox cbRenderSignText;
        private GuiCheckBox cbUseVBO;
        private GuiCheckBox cbUseMipMap;
        private GuiTrackBar tbMaxFps;
        private GuiTrackBar tbFlySpeed;

        public GuiOptions(WorldRenderer worldRenderer)
        {
            int cy = 76;

            cbUseTextures     = new GuiCheckBox(8, cy, "Usar texturas");
            cy += 24;
            cbRenderSignText  = new GuiCheckBox(8, cy, "Renderizar texto das placas");
            cy += 24;
            cbUseVBO          = new GuiCheckBox(8, cy, "Usar VBOs");
            cy += 24;
            cbUseMipMap       = new GuiCheckBox(8, cy, "Usar Mipmaps");
            cy += 24;
            tbMaxFps          = new GuiTrackBar(8, cy, 130, "Limite de FPS: {0:0}", 120);
            cy += 20;
            tbChunkRenderDist = new GuiTrackBar(8, cy, 130, "Área de renderização: {0:0}", WorldRenderer.MAX_CHUNK_DIST);


            tbFlySpeed = new GuiTrackBar(0, 0, 100, "Velocidade do fly: {0:0.00}", 10f);

            cbUseTextures.IsChecked = ViewerConfig.UseTexture;
            cbUseTextures.OnCheckChanged += (s, e) => {
                ViewerConfig.UseTexture = cbUseTextures.IsChecked;
                if (cbUseTextures.IsChecked) {
                    TextureManager.LoadTextureMap();
                }

                ViewForm.OpenForm.InvokeOnGLThread(() => worldRenderer.SetAllDirty());
            };
            Child.Add(cbUseTextures);

            cbRenderSignText.IsChecked = ViewerConfig.RenderSigns;
            cbRenderSignText.OnCheckChanged += (s, e) => ViewerConfig.RenderSigns = cbRenderSignText.IsChecked;
            Child.Add(cbRenderSignText);

            tbChunkRenderDist.Value = ViewerConfig.RenderDist / (float)WorldRenderer.MAX_CHUNK_DIST;
            tbChunkRenderDist.OnValueChanged += (s, e) =>
            {
                int d = (int)Math.Round(tbChunkRenderDist.Value * WorldRenderer.MAX_CHUNK_DIST);
                ViewerConfig.RenderDist = d;
            };
            Child.Add(tbChunkRenderDist);

            cbUseVBO.IsChecked = ViewerConfig.UseVBO;
            cbUseVBO.OnCheckChanged += (s, e) => {
                System.Diagnostics.Debug.WriteLine("Check VBO: " + cbUseVBO.IsChecked + " available: " + GL.IsVBOAvailable);
                if (GL.IsVBOAvailable) {
                    ViewForm.OpenForm.InvokeOnGLThread(() => {
                        ViewerConfig.UseVBO = cbUseVBO.IsChecked;
                        worldRenderer.SetAllDirty(true);
                    });
                } else {
                    cbUseVBO.IsChecked = false;
                }
            };
            Child.Add(cbUseVBO);

            cbUseMipMap.IsChecked = ViewerConfig.UseMipMap;
            cbUseMipMap.OnCheckChanged += (s, e) => {
                ViewerConfig.UseMipMap = cbUseMipMap.IsChecked;
                var viewer = ViewForm.OpenForm;
                viewer.InvokeOnGLThread(() => {
                    var texManager = viewer.TexManager;
                    texManager.DeleteTexture("debug_textures_blocks");
                });
            };
            Child.Add(cbUseMipMap);

            tbMaxFps.Value = ViewerConfig.MaxFps / 120.0f;
            tbMaxFps.OnValueChanged += (s, e) => {
                ViewerConfig.MaxFps = (int)Math.Round(tbMaxFps.Value * 120.0f);
            };
            Child.Add(tbMaxFps);

            tbFlySpeed.Value = (float)(ViewerConfig.FlySpeed / tbFlySpeed.Maximum);
            tbFlySpeed.OnValueChanged += (s, e) => {
                ViewerConfig.FlySpeed = tbFlySpeed.Value * tbFlySpeed.Maximum;
            };
            Child.Add(tbFlySpeed);
        }

        public override void Draw(Font fnt, int cx, int cy, int w, int h)
        {
            tbFlySpeed.X = w - tbFlySpeed.GetBounds(fnt).Width - 16;
            tbFlySpeed.Y = h - 24;
            tbFlySpeed.Visible = ViewForm.OpenForm.Fly;

            base.Draw(fnt, cx, cy, w, h);
            
            GL.glEnable(GL.GL_TEXTURE_2D);
            fnt.Draw("Teclas:\nE: Mostra o inventário - F4: Mostra informações do renderizador - F5: Atualiza todas as chunks - F6: Freecam", 2, h - 19, false);
            GL.glDisable(GL.GL_TEXTURE_2D);
        }
    }
}
