// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays;

namespace osu.Desktop.Tests.Visual
{
    public class TestCaseKeyConfiguration : OsuTestCase
    {
        private readonly KeyConfigurationOverlay configuration;

        public override string Description => @"Key configuration";

        public TestCaseKeyConfiguration()
        {
            Child = configuration = new KeyConfigurationOverlay();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            configuration.Show();
        }
    }
}
