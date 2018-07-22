﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.HelperNamespace;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Compilation.ControlTree
{
    public abstract class BindingExtensionParameter
    {
        public string Identifier { get; }
        public bool Inherit { get; }
        public ITypeDescriptor ParameterType { get; }

        public BindingExtensionParameter(string identifier, ITypeDescriptor type, bool inherit)
        {
            this.Identifier = identifier;
            this.ParameterType = type;
            this.Inherit = inherit;
        }

        public abstract Expression GetServerEquivalent(Expression controlParameter);
        public abstract JsExpression GetJsTranslation(JsExpression dataContext);

        public override bool Equals(object obj) =>
            obj is BindingExtensionParameter other && Equals(other);

        public bool Equals(BindingExtensionParameter other) =>
            string.Equals(Identifier, other.Identifier) && Inherit == other.Inherit && ParameterType.IsEqualTo(other.ParameterType);

        public override int GetHashCode() =>
            unchecked(((Identifier?.GetHashCode() ?? 0) * 397) ^ (Inherit.GetHashCode() * 17) + ParameterType.FullName.GetHashCode());
    }

    public class CurrentMarkupControlExtensionParameter : BindingExtensionParameter
    {
        public CurrentMarkupControlExtensionParameter(ITypeDescriptor controlType) : base("_control", controlType, true)
        {
        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.Convert(ExpressionUtils.Replace((DotvvmBindableObject c) => c.GetClosestControlBindingTarget(), controlParameter), ResolvedTypeDescriptor.ToSystemType(ParameterType));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return dataContext.Member("$control").WithAnnotation(new ViewModelInfoAnnotation(ResolvedTypeDescriptor.ToSystemType(this.ParameterType), isControl: true));
        }

        public static CurrentMarkupControlExtensionParameter refserializer_create(ITypeDescriptor parameterType) => new CurrentMarkupControlExtensionParameter(parameterType);
    }

    public class CurrentCollectionIndexExtensionParameter : BindingExtensionParameter
    {
        public CurrentCollectionIndexExtensionParameter() : base("_index", new ResolvedTypeDescriptor(typeof(int)), true)
        {

        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return ExpressionUtils.Replace((DotvvmBindableObject c) => c.GetAllAncestors(true, false).OfType<DataItemContainer>().First().DataItemIndex.Value, controlParameter);
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsSymbolicParameter(JavascriptTranslator.CurrentIndexParameter);
        }
    }

    public class BindingPageInfoExtensionParameter : BindingExtensionParameter
    {
        public BindingPageInfoExtensionParameter() : base("_page", new ResolvedTypeDescriptor(typeof(BindingPageInfo)), true)
        {

        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.New(typeof(BindingPageInfo));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsObjectExpression(
                new JsObjectProperty(nameof(BindingPageInfo.EvaluatingOnClient), new JsLiteral(true)),
                new JsObjectProperty(nameof(BindingPageInfo.EvaluatingOnServer), new JsLiteral(false)),
                new JsObjectProperty(nameof(BindingPageInfo.IsPostbackRunning), new JsIdentifierExpression("dotvvm").Member("isPostbackRunning").Invoke())
            );
        }
    }

    public class BindingCollectionInfoExtensionParameter : BindingExtensionParameter
    {
        public BindingCollectionInfoExtensionParameter(string identifier) : base(identifier, new ResolvedTypeDescriptor(typeof(BindingCollectionInfo)), true)
        {
        }

        public override Expression GetServerEquivalent(Expression controlParameter) =>
            ExpressionUtils.Replace((DotvvmBindableObject c) => new BindingCollectionInfo(c.GetAllAncestors(true, false).OfType<DataItemContainer>().First().DataItemIndex.Value), controlParameter);

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsObjectExpression();
        }
    }

    public class InjectedServiceExtensionParameter : BindingExtensionParameter
    {
        public InjectedServiceExtensionParameter(string identifier, ITypeDescriptor type)
            : base(identifier, type, inherit: true) { }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            var type = ((ResolvedTypeDescriptor)this.ParameterType).Type;
            var expr = ExpressionUtils.Replace((DotvvmBindableObject c) => ResolveStaticCommandService(c, type), controlParameter);
            return Expression.Convert(expr, type);
        }

        private object ResolveStaticCommandService(DotvvmBindableObject c, Type type)
        {
            var context = (IDotvvmRequestContext)c.GetValue(Internal.RequestContextProperty, true);
            return context.Services.GetService<IStaticCommandServiceLoader>().GetStaticCommandService(type, context);
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            throw new InvalidOperationException($"Can't use injected services in javascript-translated bindings.");
        }
    }

    public class BindingApiExtensionParameter : BindingExtensionParameter
    {
        public BindingApiExtensionParameter() : base("_api", new ResolvedTypeDescriptor(typeof(BindingApi)), true)
        {

        }

        public override Expression GetServerEquivalent(Expression controlParameter)
        {
            return Expression.New(typeof(BindingApi));
        }

        public override JsExpression GetJsTranslation(JsExpression dataContext)
        {
            return new JsObjectExpression();
        }
    }
}
