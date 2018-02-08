﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Core.Common;
using DotVVM.Framework.Api.Swashbuckle.Attributes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotVVM.Framework.Api.Swashbuckle.AspNetCore.Filters
{
    public class AddAsObjectOperationFilter : IOperationFilter
    {
        private readonly DotvvmApiKnownTypesOptions knownTypesOptions;

        public AddAsObjectOperationFilter(IOptions<DotvvmApiKnownTypesOptions> knownTypesOptions)
        {
            this.knownTypesOptions = knownTypesOptions.Value;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor)
            {
                ApplyControllerAction(operation, context.ApiDescription);
            }
        }

        private void ApplyControllerAction(Operation operation, ApiDescription apiDescription)
        {
            // all properties of objects with FromQuery parameters have the same ParameterDescriptor
            var groups = apiDescription.ParameterDescriptions.GroupBy(p => p.ParameterDescriptor);
            foreach (var group in groups.Where(p => p.Count() > 1))
            {
                var parameterDescriptor = (ControllerParameterDescriptor)group.First().ParameterDescriptor;

                // determine group name
                var attribute = parameterDescriptor
                    .ParameterInfo
                    .GetCustomAttribute<AsObjectAttribute>();

                if (attribute == null)
                {
                    continue;
                }

                // add group name in the metadata
                foreach (var param in group)
                {
                    var jsonParam = operation.Parameters.SingleOrDefault(p => p.Name == param.Name);
                    if (jsonParam != null)
                    {
                        var parameterType = attribute.ClientType ?? param.ParameterDescriptor.ParameterType;

                        jsonParam.Name = parameterDescriptor.Name + '.' + jsonParam.Name;
                        jsonParam.Extensions.Add(ApiConstants.DotvvmWrapperTypeKey, parameterType.FullName + ", " + parameterType.Assembly.GetName().Name);
                    }
                }
            }
        }
    }
}
