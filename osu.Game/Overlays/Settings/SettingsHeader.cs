// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Settings
{
    public class SettingsHeader : Container
    {
        private readonly LocalisableString heading;
        private readonly LocalisableString subheading;

        public SettingsHeader(LocalisableString heading, LocalisableString subheading)
        {
            this.heading = heading;
            this.subheading = subheading;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                new OsuTextFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Padding = new MarginPadding
                    {
                        Horizontal = SettingsPanel.CONTENT_MARGINS,
                        Top = Toolbar.Toolbar.TOOLTIP_HEIGHT,
                        Bottom = 30
                    }
                }.With(flow =>
                {
                    flow.AddText(heading, header => header.Font = OsuFont.TorusAlternate.With(size: 40));
                    flow.NewLine();
                    flow.AddText(subheading, subheader =>
                    {
                        subheader.Colour = colourProvider.Content2;
                        subheader.Font = OsuFont.GetFont(size: 18);
                    });
                })
            };
        }
    }
}
