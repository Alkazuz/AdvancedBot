using AdvancedBot.client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvancedBot.Viewer.Gui
{
    public class GuiChatInput : GuiBase
    {
        public string Text = "";
        public int CaretPos = 0;
        private long LastInput;

        private ViewForm form;
        public GuiChatInput(ViewForm form)
        {
            this.form = form;
        }

        public override void Draw(Font fnt, int cx, int cy, int w, int h)
        {
            w = (w * 2) / 3;
            h = (h * 2) / 3;

            GL.glPushMatrix();
            GL.glScalef(1.5f, 1.5f, 1);

            GL.glColor4f(0.5f, 0.5f, 0.5f, 0.4f);
            GuiUtils.DrawRectangle(2, h - 14, w - 4, 12);

            GL.glEnable(GL.GL_TEXTURE_2D);
            fnt.Draw(Text, 3, h - 12, false);
            GL.glDisable(GL.GL_TEXTURE_2D);

            long now = Utils.GetTimestamp();
            if ((now - LastInput) < 500 || now / 500 % 2 == 0) {
                int tw = fnt.Measure(Text, 0, CaretPos) + 3;
                GL.glColor4f(0.2f, 0.2f, 0.2f, 0.8f);
                GuiUtils.DrawRectangle(tw, h - 13, 1, 10);
            }

            GL.glPopMatrix();
        }

        public override void KeyPress(char chr)
        {
            base.KeyPress(chr);
            switch (chr) {
                case '\b': { //Backspace
                    if (CaretPos > 0) {
                        Text = Text.Remove(CaretPos - 1, 1);
                        --CaretPos;
                    }
                    break;
                }
                case '\r': { //Return, Enter
                    form.Client.SendMessage(Text);
                    form.OpenGUI = null;
                    break;
                }
                default: {
                    if (chr >= 0x20 && Text.Length < 99) {
                        Text = Text.Insert(CaretPos, chr.ToString());
                        ++CaretPos;
                    }
                    break;
                }
            }
        }
        public override void KeyDown(KeyEventArgs e)
        {
            base.KeyDown(e);
            LastInput = Utils.GetTimestamp();

            if (e.KeyCode == Keys.Left && CaretPos > 0) {
                --CaretPos;
            } else if(e.KeyCode == Keys.Right && CaretPos < Text.Length) {
                ++CaretPos;
            } else if(e.KeyCode == Keys.V && e.Modifiers == Keys.Control) {
                string txt = Clipboard.GetText();
                if(txt != null) {
                    if (txt.Length + Text.Length > 99) {
                        int rem = 99 - Text.Length;
                        txt = txt.Substring(0, rem);
                        Debug.WriteLine(rem+" " +txt);
                    }
                    Text = Text.Insert(CaretPos, txt);
                    CaretPos += txt.Length;
                }
            } else if(e.KeyCode == Keys.C && e.Modifiers == Keys.Control) {
                Clipboard.SetDataObject(Text, true);
            }
        }
    }
}
