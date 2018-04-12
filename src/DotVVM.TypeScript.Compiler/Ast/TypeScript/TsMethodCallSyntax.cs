﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Utils;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsMethodCallSyntax : TsExpressionSyntax, IMethodCallSyntax
    {
        public IIdentifierSyntax Name { get; }
        public ImmutableList<IExpressionSyntax> Parameters { get;  }

        public TsMethodCallSyntax(ISyntaxNode parent, IIdentifierSyntax name, ImmutableList<IExpressionSyntax> parameters) : base(parent)
        {
            Name = name;
            Parameters = parameters;
        }

        public override string ToDisplayString()
        {
            return $"{Name.ToDisplayString()}({Parameters.Select(p => p.ToDisplayString()).StringJoin(",")})";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitMethodCall(this);
        }
    }
}