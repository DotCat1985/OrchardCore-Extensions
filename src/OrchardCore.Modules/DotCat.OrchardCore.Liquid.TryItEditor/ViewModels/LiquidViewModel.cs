﻿using System.Collections.Generic;

namespace DotCat.OrchardCore.Liquid.TryItEditor.ViewModels
{
    public class LiquidViewModel
    {
        public string Liquid { get; set; }
        public string Output { get; set; }
        public List<string> Errors { get; set; }
    }
}
