using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using AdvancedBot.client;
using System.Diagnostics;
using System.Drawing;

namespace AdvancedBot
{
    public partial class Spammer : Form
    {
        private static Regex RAND_TOKEN_REGEX = new Regex(@"{rand,([^,]+),(\d+)}", RegexOptions.Compiled);
        private static Regex FOREACH_TOKEN_REGEX = new Regex(@"{foreach,([^,]+),(.+)}", RegexOptions.Compiled);

        private static Random RNG = new Random();

        private Thread msgLoopThread;
        private int msgDelay;
        bool continueLoop = true;
        bool spamMsgs = false;

        public Spammer()
        {
            InitializeComponent();

            tbMsg.PreviewKeyDown += (s, e) => e.IsInputKey = e.KeyCode == Keys.Tab;
            tbMsg.MouseDown += (s, e) => HideSuggestions();
            tbMsg.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
                    tbMsg.SelectAll();
                else
                    HandleSuggestKeys(e);
            };

            tbMsg.KeyPress += (s, e) => {
                if (e.KeyChar == '\t') { e.Handled = true; return; }
                if (e.KeyChar < 0x20) return;

                string txt = tbMsg.Text;
                if (txt.Length > 0) {
                    int currentLine = tbMsg.GetFirstCharIndexOfCurrentLine();

                    int lnEnd = currentLine;
                    for (int l = txt.Length; lnEnd < l; lnEnd++) {
                        char c = txt[lnEnd];
                        if (c == '\r' || c == '\n') break;
                    }
                    int lnLen = lnEnd - currentLine;
                    if (lnLen >= 100) e.Handled = true;
                }
            };

            msgLoopThread = new Thread(() => {
                while (continueLoop) {
                    try {
                        if (spamMsgs) {
                            bool delayLn = cbDelayForEachLines.Checked;
                            if (msgDelay == 0) msgDelay = (int)numericUpDown1.Value;

                            foreach (string txt in tbMsg.Lines) {
                                if (!ParseForeachToken(txt)) {
                                    List<MinecraftClient> clients = Program.FrmMain.Clients;
                                    for (int i = 0; i < clients.Count; i++) {
                                        clients[i].SendMessage(ParseRandToken(txt));
                                    }
                                }

                                if (delayLn) Thread.Sleep(msgDelay);
                            }

                            if (!delayLn) Thread.Sleep(msgDelay);
                        } else {
                            Thread.Sleep(20);
                        }
                    } catch { }
                }
            });
            msgLoopThread.Start();

            FormClosing += (s, e) => {
                continueLoop = false;
                try {
                    msgLoopThread.Join(200);
                    msgLoopThread.Abort();
                } catch { }
            };
        }

        private void tbMsg_TextChanged(object sender, EventArgs e)
        {
            string txt = tbMsg.Text;
            int lnLen = txt.Length;
            if (txt.Length > 0) {
                int currentLine = tbMsg.GetFirstCharIndexOfCurrentLine();

                int lnEnd = currentLine;
                for (int l = txt.Length; lnEnd < l; lnEnd++) {
                    char c = txt[lnEnd];
                    if (c == '\r' || c == '\n') break;
                }
                lnLen = lnEnd - currentLine;
            }
            SuggestCompletions(txt);
            lblRestChars.Text = (100 - lnLen) + " caracteres restantes";
        }

        #region auto completion
        private ListBox lbSuggestions;
        private bool suggestionsForCurrent = true;
        private int currentBlockSize = 0;

        private void HideSuggestions()
        {
            suggestionsForCurrent = false;
            if (lbSuggestions != null)
                lbSuggestions.Visible = false;
        }
        private void SuggestCompletions(string txt)
        {
            int cur = tbMsg.SelectionStart;
            int tmp;
            string block = ExtractTokenBlock(txt, cur, out tmp);
            if (block != null) {
                string[] p = block.Substring(1).Split(',');
                if (!suggestionsForCurrent && p.Length == currentBlockSize) return;

                suggestionsForCurrent = true;
                currentBlockSize = p.Length;

                if (lbSuggestions == null) {
                    lbSuggestions = new ListBox();
                    lbSuggestions.ScrollAlwaysVisible = true;
                    lbSuggestions.Size = new Size(150, 80);
                    lbSuggestions.IntegralHeight = false;
                    Controls.Add(lbSuggestions);
                }
                lbSuggestions.BeginUpdate();
                lbSuggestions.Items.Clear();
                switch (p.Length) {
                    case 1:
                        foreach (string token in "rand,foreach".Split(','))
                            if (token.StartsWith(p[0]))
                                lbSuggestions.Items.Add(token);
                        break;
                    case 2: {
                            string first = p[0];
                            if (first == "rand")
                                lbSuggestions.Items.AddRange("[a-z],[A-Z],[0-9]".Split(','));
                            else if (first == "foreach")
                                lbSuggestions.Items.AddRange("player,playerd,playerdi".Split(','));
                        }
                        break;
                    case 3: {
                            string first = p[0];
                            if (first == "rand")
                                lbSuggestions.Items.AddRange("5,10,20,50,100".Split(','));
                            else if (first == "foreach")
                                lbSuggestions.Items.AddRange("%nick%|{rand,@#$!ABCD,100}".Split('|'));
                        }
                        break;
                }

                Point pos = tbMsg.GetPositionFromCharIndex(cur - 1);
                lbSuggestions.Location = new Point(tbMsg.Location.X + pos.X + 12, tbMsg.Location.Y + pos.Y + 28);
                lbSuggestions.EndUpdate();
                lbSuggestions.BringToFront();

                if (lbSuggestions.Items.Count == 0)
                    lbSuggestions.Visible = false;
                else {
                    lbSuggestions.SelectedIndex = 0;
                    if (!lbSuggestions.Visible) lbSuggestions.Visible = true;
                }
            } else if (lbSuggestions != null && lbSuggestions.Visible) lbSuggestions.Visible = false;
        }
        private void ApplySuggestion()
        {
            if (lbSuggestions.SelectedItem != null) {
                string suggest = lbSuggestions.SelectedItem.ToString();
                if (suggest[0] == '[' && suggest[suggest.Length - 1] == ']') {
                    char basis = suggest[1];
                    int l = (basis | 0x20) == 'a' ? 26 : 10;
                    char[] c = new char[l];
                    for (int i = 0; i < l; i++)
                        c[i] = (char)(basis + i);
                    suggest = new string(c);
                }
                int st;

                int curOld = tbMsg.SelectionStart;
                string txt = tbMsg.Text;
                string block = ExtractTokenBlock(txt, curOld, out st);
                if (block != null) {
                    int pos = -1;
                    int commaCount = 0;
                    for (int i = curOld - 1; i >= st; i--) {
                        char ch = txt[i];
                        if (ch == ',' || ch == '{') {
                            if (pos == -1) pos = i + 1;
                            commaCount += ch != '{' ? 1 : 0;
                        }
                    }
                    if (commaCount < 2) {
                        if (!block.Contains("{rand"))
                            suggest += ",";
                        else
                            pos = st + block.Length;
                        tbMsg.SelectionStart = st + pos;
                        tbMsg.SelectionLength = curOld - pos;
                    }
                    tbMsg.SelectedText = suggest;
                } else tbMsg.AppendText(suggest);
            }
        }
        private void HandleSuggestKeys(KeyEventArgs e)
        {
            if (lbSuggestions == null || !lbSuggestions.Visible) return;

            switch (e.KeyCode) {
                case Keys.Down:
                    lbSuggestions.SelectedIndex = (lbSuggestions.SelectedIndex + 1) % lbSuggestions.Items.Count;
                    break;
                case Keys.Up:
                    int n = lbSuggestions.SelectedIndex - 1;
                    if (n < 0) n = lbSuggestions.Items.Count - 1;
                    lbSuggestions.SelectedIndex = n;
                    break;
                case Keys.Tab: ApplySuggestion(); break;
                case Keys.Escape: HideSuggestions(); break;
                case Keys.OemPipe: if (lbSuggestions.Visible) lbSuggestions.Visible = false; break;
            }
        }
        private string ExtractTokenBlock(string txt, int pos, out int start)
        {
            start = -1;
            for (int i = pos - 1; i >= 0; i--) {
                char ch = txt[i];
                if (ch == '{') {
                    start = i;
                    break;
                } else if (ch == '}') return null;
            }
            if (start == -1) return null;

            for (int i = start, b = 0; i < txt.Length + 1; i++) {
                if (i >= pos) return txt.Substring(start);
                char ch = txt[i];
                if (ch == '{')
                    b++;
                else if (ch == '}' && --b == 0)
                    return txt.Substring(start, i - start + 1);
            }
            return null;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (spamMsgs) {
                spamMsgs = false;
                btn.Text = "Iniciar";
            } else {
                spamMsgs = true;
                btn.Text = "Parar";
            }
        }

        public static string ParseRandToken(string txt)
        {
            //foreach(Match m in RAND_TOKEN_REGEX.Matches(txt))
            for (Match m; (m = RAND_TOKEN_REGEX.Match(txt)).Success;) {
                string chars = m.Groups[1].Value;
                int count;
                if (int.TryParse(m.Groups[2].Value, out count)) {
                    if (count > 99) count = 99;

                    char[] rndStr = new char[count];
                    for (int i = 0; i < count; i++)
                        rndStr[i] = chars[RNG.Next(chars.Length)];

                    if (m.Index != 0) {
                        string init = txt.Substring(0, m.Index);
                        string end = txt.Substring(m.Index + m.Length);
                        txt = init + new string(rndStr) + end;
                    } else
                        txt = new string(rndStr) + txt.Substring(m.Length);
                }
            }

            return txt;
        }

        private bool ParseForeachToken(string txt)
        {
            Match m = FOREACH_TOKEN_REGEX.Match(txt);
            if (m.Success) {
                string property = m.Groups[1].Value.ToLower();
                string text = m.Groups[2].Value;
                switch (property) {
                    case "player":
                        foreach (MinecraftClient cli in Program.FrmMain.Clients) {
                            if (cli.IsBeingTicked()) {
                                PlayerManager pm = cli.PlayerManager;
                                foreach (string nick in pm.UUID2Nick.Values)
                                    cli.SendMessage(ParseRandToken(text).Replace("%nick%", nick));
                            }
                        }
                        return true;
                    case "playerd": return ProcessForeachPlayerDelay(text);
                    case "playerdi": return ProcessForeachPlayerDelayI(text);
                }
            }
            return false;
        }
        private bool ProcessForeachPlayerDelay(string text)
        {
            HashSet<string> nicks = new HashSet<string>();
            foreach (MinecraftClient cli in Program.FrmMain.Clients) {
                if (cli.IsBeingTicked()) {
                    PlayerManager pm = cli.PlayerManager;
                    foreach (string nick in pm.UUID2Nick.Values)
                        if (nick != cli.Username)
                            nicks.Add(nick);
                }
            }
            int sepIdx = text.IndexOf('|');
            int delay;
            if (sepIdx != -1 && int.TryParse(text.Substring(0, sepIdx), out delay)) {
                text = text.Substring(sepIdx + 1);
                if (text.Length > 0) {
                    foreach (string nick in nicks) {
                        if (!spamMsgs) return true;

                        string ftxt = ParseRandToken(text).Replace("%nick%", nick);
                        foreach (MinecraftClient cli in Program.FrmMain.Clients)
                            cli.SendMessage(ftxt);

                        Thread.Sleep(delay);
                    }
                }
            }
            return true;
        }
        private bool ProcessForeachPlayerDelayI(string text)
        {
            List<string> nicks = new List<string>();
            foreach (MinecraftClient cli in Program.FrmMain.Clients) {
                if (cli.IsBeingTicked()) {
                    PlayerManager pm = cli.PlayerManager;
                    foreach (string nick in pm.UUID2Nick.Values)
                        if (nick != cli.Username && !nicks.Contains(nick))
                            nicks.Add(nick);
                }
            }
            int sepIdx = text.IndexOf('|');
            int delay;
            int count = 2;
            if (sepIdx != -1 && nicks.Count > 0) {
                string dparams = text.Substring(0, sepIdx);
                int sepIdxn = dparams.IndexOf('-');
                if (!int.TryParse(sepIdxn == -1 ? dparams : dparams.Substring(0, sepIdxn), out delay)) return false;
                if (sepIdxn != -1 ? !int.TryParse(dparams.Substring(sepIdxn + 1), out count) : false) return false;

                text = text.Substring(sepIdx + 1);
                if (text.Length > 0) {
                    int nickIdx = 0;
                    for (int i = 0; i < count; i++) {
                        foreach (MinecraftClient cli in Program.FrmMain.Clients) {
                            if (!spamMsgs) return true;

                            string nick = nicks[nickIdx++ % nicks.Count];
                            cli.SendMessage(ParseRandToken(text).Replace("%nick%", nick));
                        }
                        Thread.Sleep(delay);
                    }
                }
            }
            return true;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            msgDelay = (int)numericUpDown1.Value;
        }

        private void Spammer_Load(object sender, EventArgs e)
        {
            Translation.setup(this);
        }
    }
}