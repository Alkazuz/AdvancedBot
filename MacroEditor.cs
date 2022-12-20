using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AdvancedBot.client;
using AdvancedBot.client.Map;
using AdvancedBot.Script;
using Esprima;
using Esprima.Ast;
using FastColoredTextBoxNS;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using JsProgram = Esprima.Ast.Program;

namespace AdvancedBot
{
    #pragma warning disable IDE1006 // Naming Styles
    public partial class MacroEditor : Form
    {
        private Style BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
        private Style GreenStyle = new TextStyle(Brushes.Green, null, FontStyle.Italic);
        private Style BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Regular);
        private Style ErrorStyle = new ErrorMarker(Pens.Red);
        private Style PossibleErrorStyle = new ErrorMarker(Pens.Green);

        private static HashSet<string> KeywordsAndLiterals = new HashSet<string>() {
            "if", "in", "do", "var", "for", "new", "try", "let",
            "this", "else", "case", "void", "with", "enum",
            "while", "break", "catch", "throw", "const", "yield",
            "class", "super", "return", "typeof", "delete",
            "switch", "export", "import", "default", "finally", "extends",
            "function", "continue", "debugger", "instanceof",
            //Literals
            "true", "false", "null"
        };
        private static HashSet<char> Separators = new HashSet<char>() {
            ' ', '\r', '\n', '.', ';', ':', '=', '!',
            '(', ')', '{', '}', '[', ']',
            '<', '>', '/', '+', '-', '*', '%', '&', '|', '^', '~'
        };

        private string filename;
        private bool isFileChanged = false; //used on the TextChangedDelayed event when a file was loaded
        private bool _isModified = true;
        private bool IsModified
        {
            get { return _isModified; }
            set {
                if (_isModified != value) {
                    Text = $"Editor de Macros - {filename}{(value ? "*" : "")}";
                }
                _isModified = value;
            }
        }

        private AutocompleteMenu completeMenu;
        public MacroEditor()
        {
            InitializeComponent();
            Translation.setup(this);
            Icon = Program.FrmMain.Icon;
            
            if (!Directory.Exists("macros\\")) {
                Directory.CreateDirectory("macros\\");
            }

            completeMenu = new AutocompleteMenu(fctb) {
                MinFragmentLength = 1
            };
            completeMenu.Items.SetAutocompleteItems(new DynamicCompleteItems(this, completeMenu, fctb));
            completeMenu.SearchPattern = @"[^=;\r\n]";
            //completeMenu.SearchPattern = @"";
            completeMenu.AllowTabKey = true;
            completeMenu.AlwaysShowTooltip = true;
            completeMenu.ToolTipDuration = 30000;

            string lastFile;
            if (Program.Config.Contains("MacroEditorLastFile") && File.Exists((lastFile = Program.Config.GetString("MacroEditorLastFile")))) {
                OpenFile(lastFile);
            } else {
                OpenDefaultFile();
            }

            fctb.Language = Language.Custom;

            fctb.AutoCompleteBrackets = true;
            fctb.AutoIndentNeeded += FCTB_AutoIndentNeeded;
            fctb.LeftBrackets = "({[".ToCharArray();
            fctb.RightBrackets = ")}]".ToCharArray();

            fctb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;
            fctb.FindEndOfFoldingBlockStrategy = FindEndOfFoldingBlockStrategy.Strategy1;

            fctb.AutoIndentExistingLines = false;
            fctb.AutoCompleteBrackets = true;
            fctb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;

            fctb.KeyDown += (sender, e) => {
                if (e.KeyData == (Keys.Space | Keys.Control)) {
                    completeMenu.Show(true);
                    e.Handled = true;
                }
            };
            fctb.TextChangedDelayed += FCTB_TextChangedDelayed;
        }

        private void FCTB_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string src = fctb.Text;
            
            if (!IsModified && !isFileChanged) {
                IsModified = true;
            }
            isFileChanged = false;

            fctb.ClearStyle(StyleIndex.All);

            Range allText = fctb.GetRange(0, src.Length - 1);

            allText.SetFoldingMarkers("{", "}");
            allText.SetFoldingMarkers("\\[", "\\]");
            allText.SetFoldingMarkers(@"/\*", @"\*/");

            int start, end;
            int idStart = 0;
            for (int i = 0, l = src.Length - 1; i < l; i++) {
                char ch = src[i];
                if (ch == '"' || ch == '\'' || (ch == '/' && i + 1 < src.Length && src[i + 1] != '/' && src[i + 1] != '*')) { //string | regex
                    start = i;
                    end = i + 1;
                    for (; end < l; end++) {
                        if (src[end] == ch && src[end - 1] != '\\') break;
                    }
                    i = ++end;
                    fctb.GetRange(start, end).SetStyle(BrownStyle);
                } else if (Separators.Contains(ch)) { //keywords
                    int idLen = i - idStart;
                    if (idLen > 1 && idLen < 16 && KeywordsAndLiterals.Contains(src.Substring(idStart, idLen).Trim())) {
                        fctb.GetRange(idStart, i).SetStyle(BlueStyle);
                    }
                    idStart = i + 1;
                }
                if (i + 1 < src.Length && ch == '/' && src[i + 1] == '/') { //comment
                    start = i;
                    if ((i = src.IndexOf('\n', i)) == -1) {
                        i = src.Length;
                    }
                    fctb.GetRange(start, i--).SetStyle(GreenStyle);
                } else if (i + 1 < src.Length && ch == '/' && src[i + 1] == '*') { /* multiline comment */
                    start = i;
                    if ((i = src.IndexOf("*/", i) + 2) == 1) {
                        i = src.Length;      //-1 + 2   = 1
                    }
                    idStart = i;
                    fctb.GetRange(start, i).SetStyle(GreenStyle);
                }
            }
            var errorHandler = new ErrorHandler() { Tolerant = true };
            JavaScriptParser jsp = new JavaScriptParser(src, new ParserOptions() {
                Tolerant = true,
                Comment = true,
                Range = true,
                ErrorHandler = errorHandler
            });
            try {
                var prog = jsp.ParseProgram();

                JsResolver sug = new JsResolver();
                sug.SetProgram(src, prog);
                fctb.Tag = sug;
                foreach(var err in sug.ErrorRanges) {
                    fctb.GetRange(err[0], err[1]).SetStyle(PossibleErrorStyle);
                    Debug.WriteLine("Slice> " + src.Substring(err[0], err[1] - err[0]));
                }
            } catch (ParserException jsex) {
                if(jsex.EndIndex == jsex.Index) {
                    jsex.Index--;
                }
                fctb.GetRange(jsex.Index, jsex.EndIndex).SetStyle(ErrorStyle);
            } catch (Exception) {
                fctb.GetRange(0, src.Length).SetStyle(ErrorStyle);
            }
            foreach (ParserException pex in errorHandler.Errors) {
                fctb.GetRange(pex.Index, pex.EndIndex).SetStyle(ErrorStyle);
            }
            sw.Stop();
            Debug.WriteLine($"Text changed took {sw.ElapsedMilliseconds}ms");
        }
        private void FCTB_AutoIndentNeeded(object sender, AutoIndentEventArgs e)
        {
            //block {}
            if (Regex.IsMatch(e.LineText, @"^[^""']*\{.*\}[^""']*$"))
                return;
            //start of block {}
            if (Regex.IsMatch(e.LineText, @"^[^""']*\{")) {
                e.ShiftNextLines = e.TabLength;
                return;
            }
            //end of block {}
            if (Regex.IsMatch(e.LineText, @"}[^""']*$")) {
                e.Shift = -e.TabLength;
                e.ShiftNextLines = -e.TabLength;
                return;
            }
            //label
            if (Regex.IsMatch(e.LineText, @"^\s*\w+\s*:\s*($|//)") &&
                !Regex.IsMatch(e.LineText, @"^\s*default\s*:")) {
                e.Shift = -e.TabLength;
                return;
            }
            //some statements: case, default
            if (Regex.IsMatch(e.LineText, @"^\s*(case|default)\b.*:\s*($|//)")) {
                e.Shift = -e.TabLength / 2;
                return;
            }
            //is unclosed operator in previous line ?
            if (Regex.IsMatch(e.PrevLineText, @"^\s*(if|for|foreach|while|[\}\s]*else)\b[^{]*$")) {
                if (!Regex.IsMatch(e.PrevLineText, @"(;\s*$)|(;\s*//)")) { //operator is unclosed
                    e.Shift = e.TabLength;
                    return;
                }
            }
        }

        private void NewMacro(string name, bool force)
        {
            if (!force) {
                PromptKeepChanges();
            }
            filename = name;
            fctb.Text = $"/* {DateTime.Now.ToString()} */\n\n";
            fctb.ClearUndo();
            IsModified = true;
            IsModified = false;
        }
        private void Save()
        {
            if (!Directory.Exists("macros\\")) {
                Directory.CreateDirectory("macros\\");
            }
            fctb.SaveToFile("macros\\" + filename, Encoding.UTF8);
            IsModified = false;
        }
        private void PromptKeepChanges()
        {
            if (IsModified && MessageBox.Show("Deseja salvar as alterações?", "Salvar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                Save();
            }
        }
        private void OpenDefaultFile()
        {
            string lastFile = null;
            if (!Program.Config.Contains("MacroEditorLastFile") || !File.Exists((lastFile = Program.Config.GetString("MacroEditorLastFile")))) {
                lastFile = Directory.GetFiles("macros\\").FirstOrDefault();
            }
            if (lastFile != null) {
                OpenFile(lastFile);
            } else {
                tsmiNew.PerformClick();
            }
        }
        private void OpenFile(string filename)
        {
            isFileChanged = true;
            fctb.OpenFile(filename, Encoding.UTF8);
            this.filename = Path.GetFileName(filename);
            IsModified = true;
            IsModified = false;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            PromptKeepChanges();
            Program.Config.AddString("MacroEditorLastFile", "macros\\" + filename);
            BlueStyle.Dispose();
            GreenStyle.Dispose();
            BrownStyle.Dispose();
            ErrorStyle.Dispose();
            base.OnFormClosing(e);
        }
        
        private void tsmiSave_Click(object sender, EventArgs e)
        {
            Save();
        }
        private void tsmiMacros_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            OpenFile("macros\\" + e.ClickedItem.Text);
        }
        private void tsmiDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja continuar?", "?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes) {
                File.Delete("macros\\" + filename);
                OpenDefaultFile();
            }
        }
        private void tsmiMacros_DropDownOpening(object sender, EventArgs e)
        {
            if (Directory.Exists("macros\\")) {
                tsmiMacros.DropDownItems.Clear();
                tsmiMacros.DropDownItems.AddRange(Directory.GetFiles("macros\\").Select(a => new ToolStripMenuItem(Path.GetFileName(a))).ToArray());
            }
        }

        private Form createNewFrm;
        private void tsmiNew_Click(object sender, EventArgs e)
        {
            if (createNewFrm != null) {
                createNewFrm.Focus();
                return;
            }
            Form frm = new Form {
                StartPosition = FormStartPosition.CenterScreen,
                Size = new Size(335, 106),
                MaximizeBox = false,
                Text = "Novo macro",
                Icon = Program.FrmMain.Icon
            };
            Size cs = frm.ClientSize;

            Button btn = new Button {
                Text = "Criar"
            };
            btn.Location = new Point(cs.Width - btn.Width - 11, cs.Height - btn.Height - 7);

            TextBox tb = new TextBox {
                Location = new Point(12, 12),
                Name = "tbFilename",
                Text = "novomacro"
            };
            btn.Focus();
            tb.Size = new Size(cs.Width - 24, cs.Height - 18 - btn.Height);
            tb.KeyPress += (s, ev) => {
                if (ev.KeyChar == '\r') {
                    btn.PerformClick();
                }
            };

            frm.Controls.Add(tb);
            frm.Controls.Add(btn);

            btn.Click += (s, ev) => {
                string filename = tb.Text;
                if (Path.GetExtension(filename) == "") {
                    filename += ".txt";
                }
                NewMacro(filename, false);
                createNewFrm = null;

                frm.Close();
            };
            frm.Owner = this;
            (createNewFrm = frm).Show();
        }

        private static string GetTypeName(Type t)
        {
            if (t.IsGenericType) {
                var gargs = t.GetGenericArguments();
                var name = t.Name;
                int qi = name.IndexOf('`');
                return string.Format("{0}<{1}>", name.Substring(0, qi == -1 ? name.Length : qi),
                                                 string.Join(", ", gargs.Select(a => GetTypeName(a))));
            } else {
                switch (Type.GetTypeCode(t)) {
                    case TypeCode.Boolean: return "bool";
                    case TypeCode.SByte: return "sbyte";
                    case TypeCode.Byte: return "byte";
                    case TypeCode.Char: return "char";
                    case TypeCode.Int16: return "short";
                    case TypeCode.UInt16: return "ushort";
                    case TypeCode.Int32: return "int";
                    case TypeCode.UInt32: return "uint";
                    case TypeCode.Int64: return "long";
                    case TypeCode.UInt64: return "ulong";
                    case TypeCode.Single: return "float";
                    case TypeCode.Double: return "double";
                    case TypeCode.String: return "string";
                    default: return t == typeof(void) ? "void" : t.Name;
                }
            }
        }

        private class ErrorMarker : MarkerStyle
        {
            private Pen pen;
            public ErrorMarker(Pen pen)
                : base(null)
            {
                this.pen = pen;
            }
            public override void Draw(Graphics g, Point pos, Range range)
            {
                int width = range.Length * range.tb.CharWidth;
                int chHeight = range.tb.CharHeight - 1;
                Point[] pts = new Point[Math.Max(2, width / 2)];
                for (int i = 0; i < pts.Length; i++) {
                    pts[i] = new Point(pos.X + i * 2, (i % 2 * 2) + pos.Y + chHeight);
                }
                g.DrawLines(pen, pts);
            }
        }

        private class MemberAutocompleteItem : AutocompleteItem
        {
            public AutocompleteMenu Menu { get; private set; }
            public string MemberName { get; protected set; }

            public MemberAutocompleteItem(AutocompleteMenu menu)
            {
                Menu = menu;
            }
            public override CompareResult Compare(string fragmentText)
            {
                var member = DynamicCompleteItems.GetFragment(Menu);
                int n = member.LastIndexOf('.');
                if (n >= 0) {
                    member = member.Substring(n + 1);
                }
                if (MemberName.StartsWith(member, StringComparison.OrdinalIgnoreCase)) {
                    return CompareResult.VisibleAndSelected;
                } else if (MemberName.IndexOf(member, StringComparison.OrdinalIgnoreCase) >= 0) {
                    return CompareResult.Visible;
                }

                return CompareResult.Hidden;
            }
            public override string GetTextForReplace()
            {
                var frag = Menu.Fragment.Text;
                int n = frag.LastIndexOf('.');
                if (n >= 0) {
                    return frag.Substring(0, n + 1) + MemberName;
                } else {
                    return MemberName;
                }
            }
        }
        private class FuncAutocompleteItem : MemberAutocompleteItem
        {
            public MethodInfo Method;

            public FuncAutocompleteItem(string methodDesc, string desc, AutocompleteMenu menu) : base(menu)
            {
                int start = methodDesc.IndexOf(' ') + 1;
                int end = methodDesc.IndexOf('(');
                MemberName = methodDesc.Substring(start, end - start);
                Text = MemberName + "()";
                ToolTipText = desc;
                ToolTipTitle = methodDesc;
            }
            public FuncAutocompleteItem(MethodInfo mi, AutocompleteMenu menu) : base(menu)
            {
                Method = mi;
                MemberName = mi.Name;
                Text = MemberName + "()";
                //Description = GetTypeName(mi.DeclaringType) + "::" + mi.Name;
                ToolTipTitle = "";//long titles will not appear

                StringBuilder sb = new StringBuilder();
                var attribs = mi.Attributes;
                if ((attribs & MethodAttributes.Private) != 0) sb.Append("private ");
                if ((attribs & MethodAttributes.Public)  != 0) sb.Append("public ");
                if ((attribs & MethodAttributes.Static)  != 0) sb.Append("static ");

                sb.AppendFormat("{0} {1}.{2}(", GetTypeName(mi.ReturnType), GetTypeName(mi.DeclaringType), mi.Name);
                
                var pars = mi.GetParameters();
                int wordWrap = 0;
                for(int i = 0; i < pars.Length; i++) {
                    if (i != 0) sb.Append(", ");
                    if (sb.Length - wordWrap > 96) {
                        wordWrap = sb.Length;
                        sb.Append('\n');
                    }
                    var par = pars[i];
                    if(par.GetCustomAttribute<ParamArrayAttribute>() != null) {
                        sb.Append("params ");
                    }
                    sb.Append(GetTypeName(par.ParameterType));
                    sb.Append(' ');
                    sb.Append(par.Name);
                }

                sb.Append(')');
                ToolTipText = sb.ToString();
            }
        }
        private class PropAutocompleteItem : MemberAutocompleteItem
        {
            public PropAutocompleteItem(string propName, JsValue val, AutocompleteMenu menu) : base(menu)
            {
                Text = propName;
                MemberName = propName;
                ToolTipTitle = val == null ? "(unknown) " : val.Type.ToString();
            }
            public PropAutocompleteItem(PropertyInfo prop, AutocompleteMenu menu) : base(menu)
            {
                MemberName = prop.Name;
                Text = prop.Name;
                ToolTipText = GetTypeName(prop.DeclaringType) + "." + prop.Name;

                StringBuilder sb = new StringBuilder();

                var attribs = prop.GetMethod != null ? prop.GetMethod.Attributes : 0;

                if ((attribs & MethodAttributes.Public) != 0) sb.Append("public ");
                if ((attribs & MethodAttributes.Static) != 0) sb.Append("static ");

                sb.AppendFormat("{0} {1} {{", GetTypeName(prop.PropertyType), prop.Name);
                
                if(prop.CanRead) sb.Append(" get;");
                if(prop.CanWrite) sb.Append(" set;");
                sb.Append(" }");

                ToolTipTitle = sb.ToString();
            }
            public PropAutocompleteItem(FieldInfo field, AutocompleteMenu menu) : base(menu)
            {
                MemberName = field.Name;
                Text = field.Name;
                ToolTipText = GetTypeName(field.DeclaringType) + "." + field.Name;

                StringBuilder sb = new StringBuilder();
                var attribs = field.Attributes;
                if ((attribs & FieldAttributes.Private) != 0) sb.Append("private ");
                if ((attribs & FieldAttributes.Public) != 0) sb.Append("public ");
                if ((attribs & FieldAttributes.Static) != 0) sb.Append("static ");

                sb.AppendFormat("{0} {1}", GetTypeName(field.FieldType), field.Name);

                ToolTipTitle = sb.ToString();
            }
        }

        private class DynamicCompleteItems : IEnumerable<AutocompleteItem>
        {
            private AutocompleteMenu menu;
            private FastColoredTextBox tb;
            private MacroEditor editor;
            private Engine jsEngine = new Engine();

            public DynamicCompleteItems(MacroEditor me, AutocompleteMenu acMenu, FastColoredTextBox fctb)
            {
                menu = acMenu;
                tb = fctb;
                editor = me;
            }

            public IEnumerator<AutocompleteItem> GetEnumerator()
            {
                if (editor.filename.EndsWith(".txt")) {
                    foreach (string s in new[] { "void sendmsg(obj... parts)$Envia uma mensagem para o servidor.",
                                                 "void log(obj... parts)$Adiciona uma mensagem ao chat do bot.",
                                                 "string randstr(string chars, int len)$Gera uma string aleatória.",
                                                 "int randnum(int min, int max)$Gera um número aleatório.",
                                                 "void wait(int delay)$Para o script pelo tempo especificado (em ms)." }) {
                        int n = s.IndexOf('$');
                        yield return new FuncAutocompleteItem(s.Substring(0, n), s.Substring(n + 1, s.Length - n - 1), menu);
                    }
                    yield break;
                }
                using (var de = DynamicEnumerate()) {
                    while(TryMoveNext(de)) {
                        yield return de.Current;
                    }
                }
            }
            private static bool TryMoveNext<T>(IEnumerator<T> enumerator)
            {
                try {
                    return enumerator.MoveNext();
                } catch {
                    return false;
                }
            }

            private IEnumerator<AutocompleteItem> DynamicEnumerate()
            {
                var text = GetFragment(menu);
                int i = tb.SelectionStart;
                Debug.WriteLine($"Fragment: '{text}'");
                if (tb.Tag is JsResolver sug) {
                    string[] p = text.Split(new[] { '.' });

                    ResolvedObject resolved = sug.ResolveVariable(p[0]);

                    if (resolved != null) {
                        for (int j = 1; j < p.Length - 1; j++) {
                            var member = p[j];
                            if (Regex.IsMatch(member, @"^\w+\(.*?\)")) {
                                member = member.Substring(0, member.IndexOf('('));
                                resolved = resolved.AccessMember(member);
                            } else if (Regex.IsMatch(member, @"^\w+\[.+?\]")) {
                                member = member.Substring(0, member.IndexOf('['));
                                resolved = resolved.AccessMember(member);
                            } else {
                                resolved = resolved.AccessMember(member);
                            }
                            Debug.WriteLine(member + "=" + resolved.Type);
                            if (resolved.Type == null) yield break;
                        }
                        Type varType = resolved.Type;
                        foreach (var ai in EnumerateMembers(varType).Concat(varType.GetInterfaces().SelectMany(EnumerateMembers))) {
                            yield return ai;
                        }
                        IEnumerable<AutocompleteItem> EnumerateMembers(Type type)
                        {
                            foreach (var field in type.GetFields()) {
                                yield return new PropAutocompleteItem(field, menu);
                            }
                            foreach (var prop in type.GetProperties()) {
                                yield return new PropAutocompleteItem(prop, menu);
                            }
                            foreach (var method in type.GetMethods().Where(a => !a.IsSpecialName)) {
                                yield return new FuncAutocompleteItem(method, menu);
                            }
                        }
                    } else {
                        JsValue jVal;
                        if (p[0].Length > 0 && (jVal = jsEngine.GetValue(p[0])) != null && jVal.IsObject()) {
                            var obj = jVal.AsObject();
                            for (int j = 1; j < p.Length - 1 && obj != null; j++) {
                                obj = obj.GetProperty(p[j])?.Value.AsObject();
                            }
                            if (obj != null) {
                                foreach (var item in EnumerateNative(obj)) {
                                    yield return item;
                                }
                            }
                        } else {
                            var v = jsEngine.Global.GetOwnProperties();
                            foreach (var item in v) {
                                var val = item.Value.Value as ObjectInstance;
                                if (val != null && val.Class == "Function") {
                                    bool ctor = val is ObjectConstructor;
                                    yield return new FuncAutocompleteItem(item.Key + "()", "", menu);
                                } else {
                                    yield return new PropAutocompleteItem(item.Key, item.Value.Value, menu);
                                }
                            }
                        }
                    }
                }
            }
            private IEnumerable<AutocompleteItem> EnumerateNative(JsValue jVal)
            {
                var obj = jVal.AsObject();

                foreach (var prop in obj.GetOwnProperties()) {
                    var val = prop.Value.Value;
                    if (val.IsObject() && val.AsObject().Class == "Function") {
                        yield return new FuncAutocompleteItem(prop.Key + "()", "", menu);
                    } else {
                        yield return new PropAutocompleteItem(prop.Key, val, menu);
                    }
                }
            }

            public static string GetFragment(AutocompleteMenu menu)
            {
                /*
                 * i < item.GetList(1, 2).Coun|  --> item.GetList(1, 2).Coun
                 * if(!item.GetList().IsEmpt|) { --> Item.GetList().IsEmpt
                 * if(cli.|) //test              --> cli.
                 */
                var tb = menu.TextBox;
                int sel = tb.SelectionStart;
                var frag = menu.Fragment;
                frag = tb.GetRange(tb.PlaceToPosition(frag.Start), sel);
                var text = frag.Text.Trim();
                
                HashSet<char> OPs = new HashSet<char>() {
                    '<', '>', '=', '!', '+', '-', '*', '/', '%', '&', '|', '^'
                };
                for (int i = text.Length - 1, brackets = 0; i >= 0; i--) {
                    char ch = text[i];
                    if (ch == ')' || ch == '}' || ch == ']') {
                        ++brackets;
                    } else if (ch == '(' || ch == '{' || ch == '[') {
                        --brackets;
                    }

                    if(brackets < 0 || (brackets == 0 && (ch == ' ' || OPs.Contains(ch)))) {
                        return text.Substring(i + 1, text.Length - i - 1);
                    }
                }
                return text;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private void MacroEditor_Load(object sender, EventArgs e)
        {

        }
    }
}
