﻿using DotVVM.Testing.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riganti.Selenium.Core;
using Xunit;
using Xunit.Abstractions;
using Riganti.Selenium.Core.Abstractions;

namespace DotVVM.Samples.Tests.New
{
    public class RepeaterTests : AppSeleniumTest
    {
        public RepeaterTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Control_Repeater_DataSourceNull()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_DataSourceNull);
                browser.Wait();

                var clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                var serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.Equal(0, clientRepeater.Children.Count);
                Assert.Equal(0, serverRepeater.Children.Count);

                var button = browser.Single("set-collection-button", this.SelectByDataUi);
                button.Click().Wait();

                clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.Equal(3, clientRepeater.Children.Count);
                Assert.Equal(3, serverRepeater.Children.Count);
            });
        }

        [Fact]
        public void Control_Repeater_RepeaterAsSeparator()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterAsSeparator);
                browser.Wait();

                var repeater = browser.Single("root-repeater", this.SelectByDataUi);

                for (int i = 2; i < 5; i++)
                {
                    var separators = repeater.FindElements("separator", this.SelectByDataUi);

                    Assert.Equal(i, separators.Count);

                    foreach (var separator in separators)
                    {
                        var texts = separator.FindElements("p");
                        Assert.Equal(3, texts.Count);
                        AssertUI.InnerTextEquals(texts[0], "First separator");
                        AssertUI.InnerTextEquals(texts[1], "Second separator");
                        AssertUI.InnerTextEquals(texts[2], "Third separator");
                    }

                    browser.Single("add-item-button", SelectByDataUi).Click();
                }
            });
        }

        [Fact]
        [SampleReference(nameof(SamplesRouteUrls.ControlSamples_Repeater_RepeaterAsSeparator))]
        public void Control_Repeater_RepeaterAsSeparator_CorrectBindingContext()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterAsSeparator);
                browser.Wait();

                var repeater = browser.Single("root-repeater", this.SelectByDataUi);

                var incrementButtons = repeater.FindElements("increment-button", this.SelectByDataUi);

                var counterValue = browser.Single("counter-value", this.SelectByDataUi);

                AssertUI.InnerTextEquals(counterValue, "0");

                var counter = 1;
                foreach (var button in incrementButtons)
                {
                    button.Click();
                    AssertUI.InnerTextEquals(counterValue, counter.ToString());
                    counter++;
                }

                browser.Single("add-item-button", SelectByDataUi).Click();
                incrementButtons = repeater.FindElements("increment-button", this.SelectByDataUi);

                foreach (var button in incrementButtons)
                {
                    button.Click();
                    AssertUI.InnerTextEquals(counterValue, counter.ToString());
                    counter++;
                }
            });
        }

        [Fact]
        public void Control_Repeater_RepeaterWrapperTag()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RepeaterWrapperTag);

                browser.FindElements("#part1>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1>div>p").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1>div>p", 3), "Test 4");

                browser.FindElements("#part2>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2>ul>li").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2>ul>li", 3), "Test 4");

                browser.FindElements("#part3>p").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3>p", 3), "Test 4");

                browser.FindElements("#part1_server>div").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part1_server>div>p").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part1_server>div>p", 3), "Test 4");

                browser.FindElements("#part2_server>ul").ThrowIfDifferentCountThan(1);
                browser.FindElements("#part2_server>ul>li").ThrowIfDifferentCountThan(4);

                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part2_server>ul>li", 3), "Test 4");

                browser.FindElements("#part3_server>p").ThrowIfDifferentCountThan(4);
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 0), "Test 1");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 1), "Test 2");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 2), "Test 3");
                AssertUI.InnerTextEquals(browser.ElementAt("#part3_server>p", 3), "Test 4");
            });
        }

        [Fact]
        public void Control_Repeater_RouteLink()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLink);

                // verify link urls
                var url = browser.CurrentUrl;
                AssertUI.Attribute(browser.ElementAt("a", 0), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 1), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 2), "href", url + "/3");
                AssertUI.Attribute(browser.ElementAt("a", 3), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 4), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 5), "href", url + "/3");
                AssertUI.Attribute(browser.ElementAt("a", 6), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 7), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 8), "href", url + "/3");
                AssertUI.Attribute(browser.ElementAt("a", 9), "href", url + "/1");
                AssertUI.Attribute(browser.ElementAt("a", 10), "href", url + "/2");
                AssertUI.Attribute(browser.ElementAt("a", 11), "href", url + "/3");

                for (int i = 0; i < 12; i++)
                {
                    AssertUI.InnerText(browser.ElementAt("a", i), s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [Fact]
        public void Control_Repeater_RouteLinkUrlSuffix()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLinkUrlSuffix);

                // verify link urls
                var url = browser.CurrentUrl;
                AssertUI.Attribute(browser.ElementAt("a", 0), "href", url + "/1?test");
                AssertUI.Attribute(browser.ElementAt("a", 1), "href", url + "/2?test");
                AssertUI.Attribute(browser.ElementAt("a", 2), "href", url + "/3?test");
                AssertUI.Attribute(browser.ElementAt("a", 3), "href", url + "/1?test");
                AssertUI.Attribute(browser.ElementAt("a", 4), "href", url + "/2?test");
                AssertUI.Attribute(browser.ElementAt("a", 5), "href", url + "/3?test");
                AssertUI.Attribute(browser.ElementAt("a", 6), "href", url + "/1?id=1");
                AssertUI.Attribute(browser.ElementAt("a", 7), "href", url + "/2?id=2");
                AssertUI.Attribute(browser.ElementAt("a", 8), "href", url + "/3?id=3");
                AssertUI.Attribute(browser.ElementAt("a", 9), "href", url + "/1?id=1");
                AssertUI.Attribute(browser.ElementAt("a", 10), "href", url + "/2?id=2");
                AssertUI.Attribute(browser.ElementAt("a", 11), "href", url + "/3?id=3");

                for (int i = 0; i < 12; i++)
                {
                    AssertUI.InnerText(browser.ElementAt("a", i), s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [Fact]
        public void Control_Repeater_RouteLinkQuery()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RouteLinkQuery);

                // verify link urls
                var url = browser.CurrentUrl;
                AssertUI.Attribute(browser.ElementAt("a", 0), "href", url + "?Static=query&Id=1");
                AssertUI.Attribute(browser.ElementAt("a", 1), "href", url + "?Static=query&Id=2");
                AssertUI.Attribute(browser.ElementAt("a", 2), "href", url + "?Static=query&Id=3");
                AssertUI.Attribute(browser.ElementAt("a", 3), "href", url + "?Static=query&Id=1");
                AssertUI.Attribute(browser.ElementAt("a", 4), "href", url + "?Static=query&Id=2");
                AssertUI.Attribute(browser.ElementAt("a", 5), "href", url + "?Static=query&Id=3");
                AssertUI.Attribute(browser.ElementAt("a", 6), "href", url + "?first=param&Static=query&Id=1#test");
                AssertUI.Attribute(browser.ElementAt("a", 7), "href", url + "?first=param&Static=query&Id=2#test");
                AssertUI.Attribute(browser.ElementAt("a", 8), "href", url + "?first=param&Static=query&Id=3#test");
                AssertUI.Attribute(browser.ElementAt("a", 9), "href", url + "?first=param&Static=query&Id=1#test");
                AssertUI.Attribute(browser.ElementAt("a", 10), "href", url + "?first=param&Static=query&Id=2#test");
                AssertUI.Attribute(browser.ElementAt("a", 11), "href", url + "?first=param&Static=query&Id=3#test");

                for (int i = 0; i < 12; i++)
                {
                    AssertUI.InnerText(browser.ElementAt("a", i), s => !string.IsNullOrWhiteSpace(s), "Not rendered Name");
                }
            });
        }

        [Fact]
        public void Control_Repeater_Separator()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_Separator);

                CheckSeparators(browser, "server-repeater");
                CheckSeparators(browser, "client-repeater");
            });
        }

        [Fact]
        public void Control_Repeater_RequiredResource()
        {
            RunInAllBrowsers(browser => {
                browser.NavigateToUrl(SamplesRouteUrls.ControlSamples_Repeater_RequiredResource);
                browser.Wait();

                var clientRepeater = browser.Single("client-repeater", this.SelectByDataUi);
                var serverRepeater = browser.Single("server-repeater", this.SelectByDataUi);

                Assert.Equal(0, clientRepeater.Children.Count);
                Assert.Equal(0, serverRepeater.Children.Count);
            });
        }

        private void CheckSeparators(IBrowserWrapper browser, string repeaterDataUi)
        {
            var repeater = browser.Single(repeaterDataUi, this.SelectByDataUi);
            for (int i = 0; i < repeater.Children.Count; i++)
            {
                if (i % 2 == 0)
                {
                    AssertUI.Attribute(repeater.Children[i], "data-ui", s => s == "item");
                }
                else
                {
                    AssertUI.Attribute(repeater.Children[i], "data-ui", s => s == "separator");
                }
            }
        }
    }
}
