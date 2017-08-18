﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Settings
{
    public class SettingsHeader : Container
    {
        private readonly string heading;
        private readonly string subheading;

        public SettingsHeader(string heading, string subheading)
        {
            this.heading = heading;
            this.subheading = subheading;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = heading,
                            TextSize = 40,
                            Margin = new MarginPadding
                            {
                                Left = SettingsOverlay.CONTENT_MARGINS,
                                Top = Toolbar.Toolbar.TOOLTIP_HEIGHT
                            },
                        },
                        new OsuSpriteText
                        {
                            Colour = colours.Pink,
                            Text = subheading,
                            TextSize = 18,
                            Margin = new MarginPadding
                            {
                                Left = SettingsOverlay.CONTENT_MARGINS,
                                Bottom = 30
                            },
                        },
                    }
                }
            };
        }
    }
}