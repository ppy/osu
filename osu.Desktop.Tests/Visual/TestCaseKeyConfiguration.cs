// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Overlays.KeyConfiguration;

namespace osu.Desktop.Tests.Visual
{
    public class TestCaseKeyConfiguration : OsuTestCase
    {
        private readonly KeyConfiguration configuration;

        public override string Description => @"Key configuration";

        public TestCaseKeyConfiguration()
        {
            Child = configuration = new KeyConfiguration();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            configuration.Show();
        }
    }
}
