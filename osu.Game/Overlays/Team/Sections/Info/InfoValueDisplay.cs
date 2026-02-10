// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Team.Sections.Info
{
    public partial class InfoValueDisplay : CompositeDrawable
    {
        private readonly OsuSpriteText title;
        public OsuSpriteText Content { get; }

        public LocalisableString Title
        {
            set => title.Text = value;
        }

        public InfoValueDisplay(InfoValueSize size = InfoValueSize.Small)
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    title = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 12),
                    },
                    Content = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(
                            size: size switch
                            {
                                InfoValueSize.Small => 12,
                                InfoValueSize.Large => 30,
                                _ => throw new ArgumentOutOfRangeException(nameof(size), size, null)
                            },
                            weight: FontWeight.Regular
                        ),
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            title.Colour = colourProvider.Content1;
            Content.Colour = colourProvider.Content2;
        }
    }

    public enum InfoValueSize
    {
        Small,
        Large,
    }
}
