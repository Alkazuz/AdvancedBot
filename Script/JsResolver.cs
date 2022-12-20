using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Esprima;
using Esprima.Ast;
using Esprima.Utils;
using JsProgram = Esprima.Ast.Program;

namespace AdvancedBot.Script
{
    public class JsResolver : EsprimaVisitor
    {
        public Dictionary<string, INode> Variables = new Dictionary<string, INode>();
        public List<int[]> ErrorRanges = new List<int[]>();
        public string Source;

        public void SetProgram(string src, JsProgram prog)
        {
            Source = src;
            Variables.Clear();
            ErrorRanges.Clear();
            VisitProgram(prog);
        }
        public ResolvedObject ResolveVariable(string varName)
        {
            if (Variables.TryGetValue(varName, out var node)) {
                if (node is Expression expr) {
                    return ResolveExpression(expr);
                }
            }
            return GetContextVariable(varName);
        }
        private ResolvedObject ResolveExpression(Expression expr)
        {
            switch(expr.Type) {
                case Nodes.Identifier: return ResolveVariable(expr.As<Identifier>().Name);
                case Nodes.MemberExpression: return ResolveMemberExpression(expr.As<MemberExpression>());
                case Nodes.CallExpression: return ResolveCallExpression(expr.As<CallExpression>());
                default:
                    Debug.WriteLine("Unknown expression type: " + expr.Type);
                    return null;
            }
        }

        private ResolvedObject ResolveMemberExpression(MemberExpression me, int funcArgs = -1)
        {
            string name = me.Property is Identifier pid ? pid.Name :
                          me.Property is Literal lit ? lit.Value.ToString() : 
                          me.Property is BinaryExpression bex ? "**" : null;
            int[] range = me.Property.Range;
            if (name == null) {
                return null;
            }
            switch (me.Object) {
                case Identifier id:        return ResolveVariable(id.Name)?.AccessMember(name, funcArgs, range);
                case MemberExpression sme: return ResolveMemberExpression(sme)?.AccessMember(name, funcArgs, range);
                case CallExpression ce:    return ResolveCallExpression(ce)?.AccessMember(name, ce.Arguments.Count, range);
                default:
                    Debug.WriteLine("Unknown member expression object type: " + me.Object.Type);
                    return null;
            }
        }
        private ResolvedObject ResolveCallExpression(CallExpression ce)
        {
            switch(ce.Callee) {
                case MemberExpression cme: return ResolveMemberExpression(cme, ce.Arguments.Count);
                case Identifier id:        return GetContextFunc(id.Name, ce.Arguments);
                default:
                    Debug.WriteLine("Unknown call expression callee type: " + ce.Callee.Type);
                    return null;
            }
        }

        public override void VisitVariableDeclarator(VariableDeclarator vd)
        {
            Variables[((Identifier)vd.Id).Name] = vd.Init;
        }

        public override void VisitMemberExpression(MemberExpression me)
        {
            var resolved = ResolveMemberExpression(me);
            if (resolved == null) {
                ErrorRanges.Add(me.Range);
            } else if (resolved != null && resolved.Type == null) {
                var r = resolved?.LastRange ?? me.Range;
                ErrorRanges.Add(r);
            }
        }
        public override void VisitCallExpression(CallExpression ce)
        {
            var resolved = ResolveCallExpression(ce);
            if(resolved == null && !(ce.Callee is Identifier)) {
                ErrorRanges.Add(ce.Range);
            } else if(resolved != null && !(resolved is NamespaceObject) && resolved.Type == null) {
                var r = resolved?.LastRange ?? ce.Range;
                ErrorRanges.Add(r);
            }
        }

        private ResolvedObject GetContextVariable(string name)
        {
            switch(name) {
                case "AdvancedBot": return new ResolvedObject() { Type = typeof(JsScriptContext.ABContext) };
                default: return null;
            }
        }
        private ResolvedObject GetContextFunc(string name, List<ArgumentListElement> args = null)
        {
            switch(name) {
                case "importNamespace":
                    if(args.Count == 1 && args[0] is Literal lit && lit.TokenType == TokenType.StringLiteral) {
                        return new NamespaceObject() { Namespace = lit.StringValue };
                    }
                    return null;
                default: return null;
            }
        }
    }
    public class ResolvedObject
    {
        public Type Type { get; set; }
        public int[] LastRange = null;
        private const BindingFlags DEFAULT_FLAGS = BindingFlags.Instance | BindingFlags.Public;

        public virtual ResolvedObject AccessMember(string name, int funcArgs = -1, int[] ranges = null)
        {
            if (Type == null) return this;
            if(ranges != null) LastRange = ranges;

            if(Type.IsArray) {
                Type = name == "length" ? typeof(int) : Type.GetElementType();
            } else if(funcArgs != -1) {
                var methods = Type.GetMethods(DEFAULT_FLAGS).Where(a => a.Name == name);
                if(Type.IsInterface || Type.IsAbstract) {
                    foreach(var impl in Type.GetInterfaces()) {
                        methods = methods.Concat(impl.GetMethods(DEFAULT_FLAGS).Where(a => a.Name == name));
                    }
                }
                Type = (methods.FirstOrDefault(a => a.GetParameters().Length == funcArgs) ??
                        methods.FirstOrDefault())?.ReturnType;
            } else {
                Type = Type.GetField(name, DEFAULT_FLAGS)?.FieldType ?? 
                       Type.GetProperty(name, DEFAULT_FLAGS)?.PropertyType;
            }
            return this;
        }
    }
    public class NamespaceObject : ResolvedObject
    {
        private string _namespace;
        public string Namespace
        {
            get { return _namespace; }
            set {
                _namespace = value;
                Type = Type.GetType(_namespace);
            }
        }

        public override ResolvedObject AccessMember(string name, int funcArgs = -1, int[] lastRanges = null)
        {
            Namespace += "." + name;
            return this;
        }
    }
}
