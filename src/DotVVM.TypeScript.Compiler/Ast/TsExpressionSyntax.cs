﻿using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast
{
    public abstract class TsExpressionSyntax : TsSyntaxNode
    {
        protected TsExpressionSyntax(TsSyntaxNode parent) : base(parent)
        {
        }
    }

    public class TsIdentifierReferenceSyntax : TsExpressionSyntax
    {
        public TsIdentifierSyntax Identifier { get; }
        

        public TsIdentifierReferenceSyntax(TsSyntaxNode parent, TsIdentifierSyntax identifier) : base(parent)
        {
            Identifier = identifier;
        }

        public override string ToDisplayString()
        {
            return $"{Identifier.ToDisplayString()}";
        }

        public override IEnumerable<TsSyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }
    }
}