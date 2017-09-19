﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    public class TestCaseKeyConfiguration : OsuTestCase
    {
        private readonly KeyBindingOverlay overlay;

        public override string Description => @"Key configuration";

        public TestCaseKeyConfiguration()
        {
            Child = overlay = new KeyBindingOverlay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            overlay.Show();
        }
    }
}
