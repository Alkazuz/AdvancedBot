using AdvancedBot.client;
using AdvancedBot.Viewer.Gui.ItemRenderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.Viewer.Gui
{
    public class GuiInventory : GuiBase
    {
        private ViewForm form;
        private TextureManager texManager;
        private MinecraftClient client;
        public GuiInventory(ViewForm form)
        {
            this.form = form;
            texManager = form.TexManager;
            client = form.Client;
        }

        //TODO: cleanup
        public override void Draw(Font font, int cx, int cy, int w, int h)
        {
            base.Draw(font, cx, cy, w, h);

            int x = (w / 2) - 176;
            Inventory inv = client.Inventory;

            GL.glEnable(GL.GL_TEXTURE_2D);
            if (client.OpenWindow != null) {
                int y = (h / 2) - (((client.OpenWindow.NumSlots / 9) * 18) + 104);
                DrawChestAndInv(font, x, y, w, h, cx, cy);
            } else {
                int y = (h / 2) - 100;
                DrawInventory(font, x, y, w, h, cx, cy);
            }
            if (Inventory.ClickedItem != null) {
                GL.glEnable(GL.GL_TEXTURE_2D);
                DrawItem(texManager, font, Inventory.ClickedItem, cx - 16, cy - 16, 2);
            }
        }
        public override void MouseUp(int cx, int cy, MouseButtons btn)
        {
            base.MouseUp(cx, cy, btn);

            int w = form.ClientSize.Width;
            int h = form.ClientSize.Height;
            ClickInv(cx, cy, w, h, btn == MouseButtons.Left);
        }

        private void DrawInventory(Font font, int x, int y, int w, int h, int cx, int cy)
        {
            Inventory inv = client.Inventory;
            if (inv.WindowID == 0) {
                GL.glBindTexture(GL.GL_TEXTURE_2D, texManager.Get("inv_slots"));
                GL.glBegin(GL.GL_QUADS);

                GL.glTexCoord2f(0, 1); GL.glVertex2f(x, y + 200);
                GL.glTexCoord2f(1, 1); GL.glVertex2f(x + 352, y + 200);
                GL.glTexCoord2f(1, 0); GL.glVertex2f(x + 352, y);
                GL.glTexCoord2f(0, 0); GL.glVertex2f(x, y);
                GL.glEnd();
            }
            bool playerInv = inv.WindowID == 0;

            string tooltip = null;
            for (int i = playerInv ? 9 : 0; i < inv.NumSlots; i++) {
                int itemX = (i % 9) * 18;
                int itemY = (i / 9) * 18;
                Rectangle rect = new Rectangle(x + 16 + (itemX * 2), y + (playerInv && i > 35 ? 8 : 0) + (itemY * 2), 32, 32);
                if (rect.Contains(cx, cy)) {
                    GL.glDisable(GL.GL_TEXTURE_2D);
                    GL.glBegin(GL.GL_QUADS);
                    GL.glColor4f(1, 1, 1, 0.5f);
                    GL.glVertex2f(rect.X, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y);
                    GL.glVertex2f(rect.X, rect.Y);
                    GL.glEnd();
                    GL.glColor4f(1, 1, 1, 1);

                    ItemStack it = inv.Slots[i];
                    if (it != null) {
                        string lore = it.GetLore();
                        if (lore != null) {
                            tooltip = string.Format("{0} §7{2}:{3}§f\n{1}", it.GetDisplayName(), lore, it.ID, it.Metadata);
                        } else {
                            tooltip = string.Format("{0} §7{1}:{2}§f", it.GetDisplayName(), it.ID, it.Metadata);
                        }
                        string ench = it.GetEnchantments();
                        if (ench != null) {
                            tooltip += "\n\n" + ench;
                        }
                    }
                    GL.glEnable(GL.GL_TEXTURE_2D);
                    if (InputHelper.IsKeyDown(Keys.Q)) {
                        client.Inventory.DropItem(client, i);
                    }
                }
                if (i - 36 == client.HotbarSlot) {
                    GL.glDisable(GL.GL_TEXTURE_2D);
                    GL.glBegin(GL.GL_LINE_LOOP);
                    GL.glLineWidth(2);
                    GL.glColor4f(0.2f, 0.2f, 0.2f, 1f);
                    GL.glVertex2f(rect.X, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y);
                    GL.glVertex2f(rect.X, rect.Y);
                    GL.glEnd();
                    GL.glLineWidth(1);
                    GL.glColor4f(1, 1, 1, 1);
                    GL.glEnable(GL.GL_TEXTURE_2D);
                }
                DrawItem(texManager, font, inv.Slots[i], rect.X, rect.Y, 2);
            }
            if (tooltip != null) {
                GuiUtils.DrawTooltip(tooltip, cx, cy, font, w, h);
            }
        }
        private void DrawChestAndInv(Font font, int x, int y, int w, int h, int cx, int cy)
        {
            GL.glBindTexture(GL.GL_TEXTURE_2D, texManager.Get("inv_slots"));

            GL.glBegin(GL.GL_QUADS);
            GL.glTexCoord2f(0.0f, 0.16f); GL.glVertex2f(x, y + 36);
            GL.glTexCoord2f(1.0f, 0.16f); GL.glVertex2f(x + 352, y + 36);
            GL.glTexCoord2f(1.0f, 0.0f); GL.glVertex2f(x + 352, y);
            GL.glTexCoord2f(0.0f, 0.0f); GL.glVertex2f(x, y);
            GL.glEnd();

            Inventory inv = client.OpenWindow;
            if (inv.Title != null) {
                GL.glPushMatrix();
                GL.glScalef(2, 2, 0);
                font.Draw(inv.Title, (x + 12) / 2, (y + 12) / 2, false);
                GL.glPopMatrix();
                GL.glColor4f(1, 1, 1, 1);

                GL.glBindTexture(GL.GL_TEXTURE_2D, texManager.Get("inv_slots"));
            }
            y += 16 * 2;

            //draw chest rows
            int rows = Math.Max(1, client.OpenWindow.NumSlots / 9);
            for (int i = 0; i < rows; i++)
                DrawSlotRow(x, y + (i * (18 * 2)));
            //draw splitter

            y += (rows * 36) - 2;
            GL.glBegin(GL.GL_QUADS);
            GL.glTexCoord2f(0.0f, 0.75f); GL.glVertex2f(x, y + 12);
            GL.glTexCoord2f(1.0f, 0.75f); GL.glVertex2f(x + 352, y + 12);
            GL.glTexCoord2f(1.0f, 0.7f); GL.glVertex2f(x + 352, y);
            GL.glTexCoord2f(0.0f, 0.7f); GL.glVertex2f(x, y);
            GL.glEnd();

            y -= (rows * 36) - 4;

            string tooltip = null;

            for (int i = 0; i < inv.NumSlots; i++) {
                int itemX = (i % 9) * 18;
                int itemY = (i / 9) * 18;
                Rectangle rect = new Rectangle(x + 16 + (itemX * 2), y + (itemY * 2), 32, 32);

                if (rect.Contains(cx, cy)) {
                    GL.glDisable(GL.GL_TEXTURE_2D);
                    GL.glBegin(GL.GL_QUADS);
                    GL.glColor4f(1, 1, 1, 0.5f);
                    GL.glVertex2f(rect.X, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y);
                    GL.glVertex2f(rect.X, rect.Y);
                    GL.glEnd();
                    GL.glColor4f(1, 1, 1, 1);

                    ItemStack it = inv.Slots[i];
                    if (it != null) {
                        string lore = it.GetLore();
                        if (lore != null) {
                            tooltip = string.Format("{0} §7{2}:{3}§f\n{1}", it.GetDisplayName(), lore, it.ID, it.Metadata);
                        } else {
                            tooltip = string.Format("{0}\n§7{1}:{2}§f", it.GetDisplayName(), it.ID, it.Metadata);
                        }
                        string ench = it.GetEnchantments();
                        if (ench != null) {
                            tooltip += "\n\n" + ench;
                        }
                    }
                    GL.glEnable(GL.GL_TEXTURE_2D);
                }
                DrawItem(texManager, font, inv.Slots[i], rect.X, rect.Y, 2);

                GL.glPushMatrix();
                GL.glScalef(1, 1, 0);
                font.Draw(Convert.ToString(i + 1), rect.X, rect.Y, false);
                GL.glPopMatrix();
                GL.glColor4f(1, 1, 1, 1);
            }
            y += (rows * 36) + 8;

            GL.glBindTexture(GL.GL_TEXTURE_2D, texManager.Get("inv_slots"));
            GL.glBegin(GL.GL_QUADS);
            GL.glTexCoord2f(0.0f, 1.0f); GL.glVertex2f(x, y + 166);
            GL.glTexCoord2f(1.0f, 1.0f); GL.glVertex2f(x + 352, y + 166);
            GL.glTexCoord2f(1.0f, 0.17f); GL.glVertex2f(x + 352, y);
            GL.glTexCoord2f(0.0f, 0.17f); GL.glVertex2f(x, y);
            GL.glEnd();

            y -= 34;
            inv = client.Inventory;

            for (int i = 9; i < inv.NumSlots; i++) {
                int itemX = (i % 9) * 18;
                int itemY = (i / 9) * 18;
                Rectangle rect = new Rectangle(x + 16 + (itemX * 2), y + (i > 35 ? 8 : 0) + (itemY * 2), 32, 32);

                if (rect.Contains(cx, cy)) {
                    GL.glDisable(GL.GL_TEXTURE_2D);
                    GL.glBegin(GL.GL_QUADS);
                    GL.glColor4f(1, 1, 1, 0.5f);
                    GL.glVertex2f(rect.X, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y);
                    GL.glVertex2f(rect.X, rect.Y);
                    GL.glEnd();
                    GL.glColor4f(1, 1, 1, 1);

                    ItemStack it = inv.Slots[i];
                    if (it != null)
                        tooltip = string.Format("{0}\n§7{1}:{2}§f", it.GetDisplayName(), it.ID, it.Metadata);
                    GL.glEnable(GL.GL_TEXTURE_2D);
                    if (InputHelper.IsKeyDown(Keys.Q)) {
                        client.Inventory.DropItem(client, i);
                    }
                }
                if (i - 36 == client.HotbarSlot) {
                    GL.glDisable(GL.GL_TEXTURE_2D);
                    GL.glBegin(GL.GL_LINE_LOOP);
                    GL.glLineWidth(2);
                    GL.glColor4f(0.2f, 0.2f, 0.2f, 1f);
                    GL.glVertex2f(rect.X, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y + rect.Height);
                    GL.glVertex2f(rect.X + rect.Width, rect.Y);
                    GL.glVertex2f(rect.X, rect.Y);
                    GL.glEnd();
                    GL.glLineWidth(1);
                    GL.glColor4f(1, 1, 1, 1);
                    GL.glEnable(GL.GL_TEXTURE_2D);
                }
                DrawItem(texManager, font, inv.Slots[i], rect.X, rect.Y, 2);
            }

            if (tooltip != null) {
                GuiUtils.DrawTooltip(tooltip, cx, cy, font, w, h);
            }
        }
        private void DrawSlotRow(int x, int y)
        {
            GL.glBegin(GL.GL_QUADS);

            const float tminx = 0.0f;
            const float tminy = 0.17f;
            const float tmaxx = 1.0f;
            const float tmaxy = 0.35f;

            GL.glTexCoord2f(tminx, tmaxy); GL.glVertex2f(x, y + 36);
            GL.glTexCoord2f(tmaxx, tmaxy); GL.glVertex2f(x + 352, y + 36);
            GL.glTexCoord2f(tmaxx, tminy); GL.glVertex2f(x + 352, y);
            GL.glTexCoord2f(tminx, tminy); GL.glVertex2f(x, y);
            GL.glEnd();
        }

        private void ClickInv(int cx, int cy, int w, int h, bool left)
        {
            if (client.OpenWindow != null) {
                int x = w / 2 - 176;
                int y = (h / 2 - (((client.OpenWindow.NumSlots / 9) * 18) + 104));
                Inventory inv = client.OpenWindow;

                y += 16 * 2 + 2;
                int rows = Math.Max(1, client.OpenWindow.NumSlots / 9);
                for (int i = 0; i < inv.NumSlots; i++) {
                    int itemX = (i % 9) * 18;
                    int itemY = (i / 9) * 18;
                    Rectangle rect = new Rectangle(x + 16 + (itemX * 2), y + (itemY * 2), 32, 32);
                    if (rect.Contains(cx, cy)) {
                        inv.Click(client, (short)i, false, left);
                        break;
                    }
                }
                y += (rows * 36) - 22;
                inv = client.Inventory;
                for (int i = 9; i < inv.NumSlots; i++) {
                    int itemX = (i % 9) * 18;
                    int itemY = (i / 9) * 18;
                    Rectangle rect = new Rectangle(x + 16 + (itemX * 2), y + (i > 35 ? 8 : 0) + (itemY * 2), 32, 32);
                    if (rect.Contains(cx, cy)) {
                        if (!left && inv.Slots[i] != null && i >= 36 && i < 45) {
                            client.HotbarSlot = i - 36;
                            client.LeftClickItem();
                        } else {
                            inv.Click(client, (short)i, true, left);
                        }
                        break;
                    }
                }
            } else {
                Inventory inv = client.Inventory;
                int x = (w / 2) - 176;
                int y = (h / 2) - 100;
                for (int i = 9; i < inv.NumSlots; i++) {
                    int itemX = (i % 9) * 18;
                    int itemY = (i / 9) * 18;
                    Rectangle rect = new Rectangle(x + 16 + (itemX * 2), y + (i > 35 ? 8 : 0) + (itemY * 2), 32, 32);
                    if (rect.Contains(cx, cy)) {
                        if (!left && inv.Slots[i] != null && i >= 36 && i < 45) {
                            client.HotbarSlot = i - 36;
                            client.LeftClickItem();
                        } else {
                            inv.Click(client, (short)i, false, left);
                        }
                        break;
                    }
                }
            }
        }
        public static void DrawItem(TextureManager texMgr, Font fnt, ItemStack item, int x, int y, int scale)
        {
            if (item == null) return;

            var renderer = ItemRendererBase.GetRenderer(item);
            if (renderer != null) {
                renderer.Render(item, x, y);
            } else {
                bool isItem = item.ID > 255;

                GL.glBindTexture(GL.GL_TEXTURE_2D, texMgr.Get(isItem ? "debug_textures_items" : "item_blocks"));

                Rectangle itemInfo = TextureManager.GetItemTexture(item.ID, item.Metadata);

                int width = itemInfo.Width;
                int height = itemInfo.Height;

                float texWidth = isItem ? 256 : 1024;
                float texHeight = isItem ? 256 : 320;

                float u0 = itemInfo.X / texWidth;
                float u1 = (itemInfo.X + width) / texWidth;
                float v0 = itemInfo.Y / texHeight;
                float v1 = (itemInfo.Y + height) / texHeight;

                if (isItem) {
                    width *= scale;
                    height *= scale;
                }
                GL.glBegin(GL.GL_QUADS);
                GL.glTexCoord2f(u0, v1); GL.glVertex2f(x, y + height);
                GL.glTexCoord2f(u1, v1); GL.glVertex2f(x + width, y + height);
                GL.glTexCoord2f(u1, v0); GL.glVertex2f(x + width, y);
                GL.glTexCoord2f(u0, v0); GL.glVertex2f(x, y);
                GL.glEnd();
            }

            if (item.Count != 1) {
                int w = fnt.Measure(item.Count.ToString()) / 2;
                fnt.Draw(item.Count.ToString(), x + ((15 - w) * scale), y + (12 * scale), false);
            }
        }
    }
}
