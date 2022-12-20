using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using AdvancedBot.client;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Esprima.Ast;
using Esprima;

namespace AdvancedBot.Script
{
    public class ScriptContext : IScriptContext
    {
        private Statement[] body;
        private int statementPos;

        private Dictionary<string, object> Variables = new Dictionary<string, object>();

        private static Regex RANDOM_SEQ_CHARS = new Regex(@"\[(.)-(.)\]", RegexOptions.Compiled);
        private Random rng = Utils.NewRandom();

        private int waitDelay = -1;
        
        public ScriptContext(MinecraftClient cli) : base(cli)
        {
            Variables["randstr"] = (Func<object[], object>)RandomString;
            Variables["randnum"] = new Func<object[], object>((pars) => rng.Next(Convert.ToInt32(pars[0]), Convert.ToInt32(pars[1])));
            Variables["sendmsg"] = new Action<object[]>((pars) => Client.SendMessage(string.Concat(pars)));
            Variables["log"]     = new Action<object[]>((pars) => Client.PrintToChat(string.Concat(pars)));
            Variables["wait"]    = new Func<object[], object>((pars) => waitDelay = Convert.ToInt32(pars[0]));
#if DEBUG
            Variables["_log"]    = new Action<object[]>((pars => Debug.WriteLine(pars[0])));
#endif
        }
        public override void Execute()
        {
            var parser = new JavaScriptParser(Source);
            var program = parser.ParseProgram();
            body = program.Body.Cast<Statement>().ToArray();
            Exec();
        }

        private async void Exec()
        {
            try {
                for (; statementPos < body.Length; statementPos++) {
                    var s = body[statementPos];
                    Expression expr = (s as ExpressionStatement)?.Expression;

                    switch (expr != null ? expr.Type : s.Type) {
                        case Nodes.AssignmentExpression:
                            AssignmentExpression ae = (AssignmentExpression)expr;
                            switch (ae.Operator) {
                                case AssignmentOperator.Assign:
                                    Variables[((Identifier)ae.Left).Name] = ResolveExpression(ae.Right);
                                    break;
                            }
                            break;
                        case Nodes.CallExpression:
                            ResolveExpression(expr);
                            if (waitDelay != -1) {
                                await Task.Delay(waitDelay);
                                waitDelay = -1;
                            }
                            break;
                        case Nodes.VariableDeclaration:
                            foreach (var varDec in ((VariableDeclaration)s).Declarations) {
                                Variables[((Identifier)varDec.Id).Name] = ResolveExpression(varDec.Init);
                            }
                            break;
                        default:
                            throw new Exception($"Declaração inválida {FormatLine(s)}");
                    }
                }
            } catch(Exception ex) {
                Client.PrintToChat(ex.Message);
            }
        }

        private object ResolveExpression(Expression expr)
        {
            switch(expr.Type) {
                case Nodes.Identifier: return Variables[((Identifier)expr).Name];
                case Nodes.Literal: return ((Literal)expr).Value;
                case Nodes.CallExpression:
                    CallExpression cs = (CallExpression)expr;
                    var func = ResolveExpression(cs.Callee) as Delegate;
                    if(func == null) {
                        throw new Exception($"Erro ao invocar a função: Identificador '{cs.Callee}' inexistente ou inválido {FormatLine(expr)}");
                    }
                    var ar = cs.Arguments.Select(a => ResolveExpression((Expression)a)).ToArray();
                    return func.DynamicInvoke(new object[] { ar });
                case Nodes.BinaryExpression:
                    BinaryExpression be = (BinaryExpression)expr;
                    object L = ResolveExpression(be.Left);
                    object R = ResolveExpression(be.Right);

                    double? Ld = L as double?;
                    double? Rd = R as double?;
                    switch(be.Operator) {
                        case BinaryOperator.Plus:   return Ld != null && Rd != null ? (object)(Ld + Rd) : L.ToString() + R.ToString();
                        case BinaryOperator.Minus:  return Ld - Rd;
                        case BinaryOperator.Times:  return Ld * Rd;
                        case BinaryOperator.Divide: return Ld / Rd;
                        case BinaryOperator.Modulo: return Ld % Rd;
                    }
                    break;
            }
            throw new Exception($"Não foi possível resolver a expressão do tipo {expr.Type} {FormatLine(expr)}");
        }
        private static string FormatLine(Expression s) => $"(linha {s.Location.Start.Line} coluna {s.Location.Start.Column})";
        private static string FormatLine(Statement s) => $"(linha {s.Location.Start.Line} coluna {s.Location.Start.Column})";

        private object RandomString(object[] pars)
        {
            string chars = (string)pars[0];
            int len = Math.Min(256, Convert.ToInt32(pars[1]));

            if (chars.IndexOf(']') != -1) {
                chars = RANDOM_SEQ_CHARS.Replace(chars, (match) => {
                    char a = match.Groups[1].Value[0];
                    char b = match.Groups[2].Value[0];
                    if (IsValidSeqChar(a, b)) {
                        int cnt = b - a + 1;
                        if (cnt <= 0) return a.ToString();
                        char[] chr = new char[cnt];
                        for (int i = 0; i < chr.Length; i++) {
                            chr[i] = (char)(a + i);
                        }
                        return new string(chr);
                    }
                    return match.Value;
                });
            }
            char[] buf = new char[len];
            for (int i = 0; i < len; i++) {
                buf[i] = chars[rng.Next(chars.Length)];
            }
            return new string(buf);
        }
        private static bool IsValidSeqChar(char a, char b)
        {
            if (a >= 'a' && b <= 'z') {
                return b >= 'a' && b <= 'z';
            } else if (a >= 'A' && a <= 'Z') {
                return b >= 'A' && b <= 'Z';
            } else if (a >= '0' && a <= '9') {
                return b >= '0' && b <= '9';
            }
            return false;
        }
    }
}
