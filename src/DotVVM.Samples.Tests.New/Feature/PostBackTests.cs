﻿using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using System.Threading;
using DotVVM.Samples.Tests.New;
using DotVVM.Testing.Abstractions;
using Riganti.Selenium.Core;
using Riganti.Selenium.Core.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace DotVVM.Samples.Tests.Feature
{
    public class PostBackTests : AppSeleniumTest
    {
        [Fact]
        public void Feature_PostBack_PostbackUpdate()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostbackUpdate);

                // enter number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "15");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("br").ThrowIfDifferentCountThan(14);

                // change number of lines and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "5");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("br").ThrowIfDifferentCountThan(4);
            });
        }

        [Fact]
        public void Feature_PostBack_PostbackUpdateRepeater()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostbackUpdateRepeater);

                // enter the text and click the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "test");
                browser.Click("input[type=button]");
                browser.Wait();

                // check the inner text of generated items
                browser.FindElements("p.item")
                    .ThrowIfDifferentCountThan(5).ForEach(e => {
                        AssertUI.InnerTextEquals(e, "test");
                    });

                // change the text and client the button
                browser.ClearElementsContent("input[type=text]");
                browser.SendKeys("input[type=text]", "xxx");
                browser.Click("input[type=button]");
                browser.Wait();

                browser.FindElements("p.item").ThrowIfDifferentCountThan(5).ForEach(e => AssertUI.InnerTextEquals(e, "xxx"));
            });
        }

        [Fact]
        public void Feature_PostBack_PostBackHandlers_Localization()
        {
            RunInAllBrowsers(browser => {

                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostBackHandlers_Localization);
                ValidatePostbackHandlersComplexSection(".commandBinding", browser);

                browser.NavigateToUrl(SamplesRouteUrls.FeatureSamples_PostBack_PostBackHandlers_Localization);
                ValidatePostbackHandlersComplexSection(".staticCommandBinding", browser);
            });
        }

        private  void ValidatePostbackHandlersComplexSection(string sectionSelector, IBrowserWrapper browser)
        {
            IElementWrapper section = null;
            browser.WaitFor(() => {
                section = browser.First(sectionSelector);
            }, 1000, "Cannot find static commands section.");

            var index = browser.First("[data-ui=\"command-index\"]");

            // confirm first
            section.ElementAt("input[type=button]", 0).Click();
            AssertUI.AlertTextEquals(browser, "Confirmation 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "1");

            // cancel second
            section.ElementAt("input[type=button]", 1).Click();
            AssertUI.AlertTextEquals(browser, "Confirmation 1");
            browser.ConfirmAlert();
            browser.Wait();

            AssertUI.AlertTextEquals(browser, "Confirmation 2");
            browser.DismissAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "1");
            // confirm second
            section.ElementAt("input[type=button]", 1).Click();
            AssertUI.AlertTextEquals(browser, "Confirmation 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.AlertTextEquals(browser, "Confirmation 2");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "2");

            // confirm third
            section.ElementAt("input[type=button]", 2).Click();
            Assert.IsFalse(browser.HasAlert());
            browser.Wait();
            AssertUI.InnerTextEquals(index, "3");

            // confirm fourth
            section.ElementAt("input[type=button]", 3).Click();
            AssertUI.AlertTextEquals(browser, "Generated 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "4");

            // confirm fifth
            section.ElementAt("input[type=button]", 4).Click();
            AssertUI.AlertTextEquals(browser, "Generated 2");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "5");

            // confirm conditional
            section.ElementAt("input[type=button]", 5).Click();
            Assert.IsFalse(browser.HasAlert());
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            browser.First("input[type=checkbox]").Click();

            section.ElementAt("input[type=button]", 5).Click();
            AssertUI.AlertTextEquals(browser, "Conditional 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            browser.First("input[type=checkbox]").Click();

            section.ElementAt("input[type=button]", 5).Click();
            Assert.IsFalse(browser.HasAlert());
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            browser.First("input[type=checkbox]").Click();

            section.ElementAt("input[type=button]", 5).Click();
            AssertUI.AlertTextEquals(browser, "Conditional 1");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "6");

            //localization - resource binding in confirm postback handler message

            section.ElementAt("input[type=button]", 6).Click();
            AssertUI.AlertTextEquals(browser, "EnglishValue");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "7");

            browser.First("#ChangeLanguageCZ").Click();

            browser.WaitFor(() => {
                index = browser.First("[data-ui=\"command-index\"]");
                AssertUI.InnerTextEquals(index, "0");
            }, 1500, "Redirect to CZ localization failed.");

            section = browser.First(sectionSelector);

            //ChangeLanguageEN
            section.ElementAt("input[type=button]", 6).Click();
            AssertUI.AlertTextEquals(browser, "CzechValue");
            browser.DismissAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "0");

            section.ElementAt("input[type=button]", 6).Click();
            AssertUI.AlertTextEquals(browser, "CzechValue");
            browser.ConfirmAlert();
            browser.Wait();
            AssertUI.InnerTextEquals(index, "7");

        }

        public PostBackTests(ITestOutputHelper output) : base(output)
        {
        }
    }
}
