// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawableProfileWeightedScore : DrawableProfileScore
    {
        private readonly double weight;

        public DrawableProfileWeightedScore(ScoreInfo score, double weight)
            : base(score)
        {
            this.weight = weight;
        }

        protected override Drawable CreateRightContent() => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Y,
                            Width = 60,
                            Child = CreateDrawableAccuracy()
                        },
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold, italics: true),
                            Text = $"{Score.PP * weight:0}pp",
                        },
                    }
                },
                new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 12),
                    Text = $@"weighted {weight:0%}"
                }
            }
        };
    }
}
