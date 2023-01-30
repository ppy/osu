// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public abstract partial class ProfileHeaderStatisticsButton : ProfileHeaderButton
    {
        private readonly OsuSpriteText drawableText;

        protected ProfileHeaderStatisticsButton()
        {
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = Icon,
                        FillMode = FillMode.Fit,
                        Size = new Vector2(50, 14)
                    },
                    drawableText = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Margin = new MarginPadding { Right = 10 },
                        Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold)
                    }
                }
            };
        }

        protected abstract IconUsage Icon { get; }

        protected void SetValue(int value) => drawableText.Text = value.ToLocalisableString("#,##0");
    }
}
