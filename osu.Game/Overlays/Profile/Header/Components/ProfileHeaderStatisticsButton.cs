// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public abstract class ProfileHeaderStatisticsButton : ProfileHeaderButton
    {
        private readonly OsuSpriteText drawableText;

        protected ProfileHeaderStatisticsButton()
        {
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Direction = FillDirection.Horizontal,
                Padding = new MarginPadding { Right = 10 },
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = CreateIcon(),
                        FillMode = FillMode.Fit,
                        Size = new Vector2(50, 14)
                    },
                    drawableText = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.Bold)
                    }
                }
            };
        }

        [NotNull]
        protected abstract IconUsage CreateIcon();

        protected void SetValue(string value) => drawableText.Text = value;
    }
}
