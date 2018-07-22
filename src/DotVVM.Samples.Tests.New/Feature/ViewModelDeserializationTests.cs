﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Riganti.Selenium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Xunit.Abstractions;
using Xunit;

namespace DotVVM.Samples.Tests.New.Feature
{
    public class ViewModelDeserializationTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_ViewModelDeserialization_DoesNotDropObject()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelDeserialization_DoesNotDropObject);

                AssertUI.InnerTextEquals(browser.First("span"), "0");
                //value++
                browser.ElementAt("input[type=button]", 2).Click();
                browser.ElementAt("input[type=button]", 2).Click();
                //check value
                AssertUI.InnerTextEquals(browser.First("span"), "2");
                //hide span
                browser.ElementAt("input[type=button]", 0).Click();
                //show span
                browser.ElementAt("input[type=button]", 1).Click();
                //value++
                browser.ElementAt("input[type=button]", 2).Click();
                //check value
                AssertUI.InnerTextEquals(browser.First("span"), "3");
            });
        }

        [Fact]
        public void Feature_ViewModelDeserialization_NegativeLongNumber()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_ViewModelDeserialization_NegativeLongNumber);

                var postback = browser.Single("[data-ui=decrement-postback]");
                var longNumber = browser.Single("[data-ui=long-number]");

                AssertUI.InnerTextEquals(longNumber, "0");

                // value--
                postback.Click();
                AssertUI.InnerTextEquals(longNumber, "-1");

                // value--
                postback.Click();
                AssertUI.InnerTextEquals(longNumber, "-2");
            });
        }

        public ViewModelDeserializationTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
