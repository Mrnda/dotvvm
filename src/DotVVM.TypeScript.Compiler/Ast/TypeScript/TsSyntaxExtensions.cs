﻿using System;

namespace DotVVM.TypeScript.Compiler.Ast.TypeScript
{
    public static class TsSyntaxExtensions
    {
        public static string ToDisplayString(this TsModifier modifier)
        {
            switch (modifier)
            {
                case TsModifier.Public:
                    return "public";
                case TsModifier.Private:
                    return "private";
                case TsModifier.Protected:
                    return "protected";
                default:
                    return string.Empty;
            }
        }

        public static string ToDisplayString(this BinaryOperator binaryOperator)
        {
            switch (binaryOperator)
            {
                case BinaryOperator.Add:
                    return "+";
                case BinaryOperator.Subtract:
                    return "-";
                case BinaryOperator.Multiply:
                    return "*";
                case BinaryOperator.Divide:
                    return "/";
                case BinaryOperator.Remainder:
                    return "%";
                case BinaryOperator.And:
                    return "&";
                case BinaryOperator.Or:
                    return "|";
                case BinaryOperator.ExclusiveOr:
                    return "^";
                case BinaryOperator.ConditionalAnd:
                    return "&&";
                case BinaryOperator.ConditionalOr:
                    return "||";
                case BinaryOperator.Equals:
                case BinaryOperator.ObjectValueEquals:
                    return "==";
                case BinaryOperator.NotEquals:
                case BinaryOperator.ObjectValueNotEquals:
                    return "!=";
                case BinaryOperator.LessThan:
                    return "<";
                case BinaryOperator.LessThanOrEqual:
                    return "<=";
                case BinaryOperator.GreaterThan:
                    return ">";
                case BinaryOperator.GreaterThanOrEqual:
                    return ">=";
                default:
                    throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, null);
            }
        }

        public static string ToDisplayString(this UnaryOperator unaryOperator)
        {
            switch (unaryOperator)
            {
                case UnaryOperator.BitwiseNegation:
                    return "~";
                case UnaryOperator.Not:
                    return "!";
                case UnaryOperator.Plus:
                    return "+";
                case UnaryOperator.Minus:
                    return "-";
                case UnaryOperator.True:
                    return "true";
                case UnaryOperator.False:
                    return "false";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unaryOperator), unaryOperator, null);
            }
        }
    }
}