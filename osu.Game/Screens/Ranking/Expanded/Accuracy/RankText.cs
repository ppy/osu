// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    /// <summary>
    /// The text that appears in the middle of the <see cref="AccuracyCircle"/> displaying the user's rank.
    /// </summary>
    public class RankText : CompositeDrawable
    {
        private readonly ScoreRank rank;

        private Drawable flash;

        public RankText(ScoreRank rank)
        {
            this.rank = rank;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Alpha = 0;
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new[]
            {
                new GlowingSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(-15, 0),
                    Text = DrawableRank.GetRankName(rank),
                    Font = OsuFont.Numeric.With(size: 76),
                    UseFullGlyphHeight = false
                },
                flash = new BufferedContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BlurSigma = new Vector2(35),
                    BypassAutoSizeAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    Size = new Vector2(2f),
                    Scale = new Vector2(1.8f),
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(-15, 0),
                            Text = DrawableRank.GetRankName(rank),
                            Font = OsuFont.Numeric.With(size: 76),
                            UseFullGlyphHeight = false,
                            Shadow = false
                        },
                    },
                },
            };
        }

        public void Appear()
        {
            this.FadeIn(0, Easing.In);

            flash.FadeIn(0, Easing.In).Then().FadeOut(800, Easing.OutQuint);
        }
    }
}
