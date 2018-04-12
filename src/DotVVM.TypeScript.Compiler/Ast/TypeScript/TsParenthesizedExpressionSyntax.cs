﻿using System.Collections.Generic;
using System.Linq;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public class TsParenthesizedExpressionSyntax : TsExpressionSyntax, IParenthesizedExpressionSyntax
    {
        public IExpressionSyntax Expression { get; }

        public TsParenthesizedExpressionSyntax(ISyntaxNode parent, IExpressionSyntax expression) : base(parent)
        {
            Expression = expression;
        }

        public override string ToDisplayString()
        {
            return $"({Expression.ToDisplayString()})";
        }

        public override IEnumerable<ISyntaxNode> DescendantNodes()
        {
            return Enumerable.Empty<TsSyntaxNode>();
        }

        public override void AcceptVisitor(INodeVisitor visitor)
        {
            visitor.VisitParenthesizedExpression(this);
        }
    }
}