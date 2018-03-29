using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.SpaContentPlaceHolder;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.SpaContentPlaceHolder_PrefixRouteName
{
    [TurnHistoryApiOffActionFilter]
    public class PageAViewModel : SpaMasterViewModel
	{
        public PageAViewModel()
        {
            HeaderText = "Sample 1";
        }

        public int Value { get; set; }

        public void IncreaseValue()
        {
            Value++;
        }
    }
}

