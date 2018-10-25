// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Core.Screens.Evast;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Mods.Evast.CursorTracers
{
    public class CursorTracerTestScreen : BeatmapScreen
    {
        private readonly CrossCursorTracer tracer;
        private readonly DelaySettings delaySettings;

        public CursorTracerTestScreen()
        {
            Children = new Drawable[]
            {
                tracer = new CrossCursorTracer(),
                delaySettings = new DelaySettings
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding(20),
                }
            };

            delaySettings.Bindable.ValueChanged += newDelay => tracer.Delay = newDelay;
        }

        private class DelaySettings : PlayerSettingsGroup
        {
            protected override string Title => @"delay";

            public readonly BindableDouble Bindable;

            public DelaySettings()
            {
                Child = new PlayerSliderBar<double>
                {
                    RelativeSizeAxes = Axes.X,
                    Bindable = Bindable = new BindableDouble(0)
                    {
                        MinValue = 0,
                        MaxValue = 1000,
                    }
                };
            }
        }
    }
}
