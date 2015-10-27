using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace DotVVM.Framework.ViewModel
{
    public class ViewModelPropertyValidationRule
    {

        [JsonProperty("ruleName")]
        public string ClientRuleName { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("parameters")]
        public object[] Parameters { get; set; }

        [JsonIgnore]
        public ValidationAttribute SourceValidationAttribute { get; set; }

        public ViewModelPropertyValidationRule(string clientRule, ValidationAttribute sourceValidationAttribute, string errorMessage, params object[] parameters)
        {
            ClientRuleName = clientRule;
            SourceValidationAttribute = sourceValidationAttribute;
            ErrorMessage = errorMessage;
            Parameters = parameters;
        }

        public ViewModelPropertyValidationRule() { }
    }
}