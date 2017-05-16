// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Testing;
using osu.Game.Overlays;

namespace osu.Desktop.VisualTests.Tests
{
    internal class TestCaseOnScreenDisplay : TestCase
    {
        private FrameworkConfigManager config;
        private Bindable<FrameSync> frameSyncMode;

        public override string Description => @"Make it easier to see setting changes";

        public override void Reset()
        {
            base.Reset();

            Add(new OnScreenDisplay());

            frameSyncMode = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync);

            FrameSync initial = frameSyncMode.Value;

            AddRepeatStep(@"Change frame limiter", setNextMode, 3);

            AddStep(@"Restore frame limiter", () => frameSyncMode.Value = initial);
        }

        private void setNextMode()
        {
            var nextMode = frameSyncMode.Value + 1;
            if (nextMode > FrameSync.Unlimited)
                nextMode = FrameSync.VSync;
            frameSyncMode.Value = nextMode;
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            this.config = config;
        }
    }
}
