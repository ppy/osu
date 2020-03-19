// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    /// <summary>
    /// The text that appears in the middle of the <see cref="AccuracyCircle"/> displaying the user's rank.
    /// </summary>
    public class RankText : CompositeDrawable
    {
        private readonly ScoreRank rank;

        private BufferedContainer flash;
        private BufferedContainer superFlash;
        private GlowingSpriteText rankText;

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
            InternalChildren = new Drawable[]
            {
                rankText = new GlowingSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    GlowColour = OsuColour.ForRank(rank),
                    Spacing = new Vector2(-15, 0),
                    Text = DrawableRank.GetRankName(rank),
                    Font = OsuFont.Numeric.With(size: 76),
                    UseFullGlyphHeight = false
                },
                superFlash = new BufferedContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BlurSigma = new Vector2(85),
                    Size = new Vector2(600),
                    CacheDrawnFrameBuffer = true,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                    Children = new[]
                    {
                        new Box
                        {
                            Colour = Color4.White,
                            Size = new Vector2(150),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                },
                flash = new BufferedContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BlurSigma = new Vector2(35),
                    BypassAutoSizeAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    CacheDrawnFrameBuffer = true,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                    Size = new Vector2(2f), // increase buffer size to allow for scale
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
            this.FadeIn();

            if (rank < ScoreRank.A)
            {
                this
                    .MoveToOffset(new Vector2(0, -20))
                    .MoveToOffset(new Vector2(0, 20), 200, Easing.OutBounce);

                if (rank <= ScoreRank.D)
                {
                    this.Delay(700)
                        .RotateTo(5, 150, Easing.In)
                        .MoveToOffset(new Vector2(0, 3), 150, Easing.In);
                }

                this.FadeInFromZero(200, Easing.OutQuint);
                return;
            }

            flash.Colour = OsuColour.ForRank(rank);
            flash.FadeIn().Then().FadeOut(1200, Easing.OutQuint);

            if (rank >= ScoreRank.S)
                rankText.ScaleTo(1.05f).ScaleTo(1, 3000, Easing.OutQuint);

            if (rank >= ScoreRank.X)
            {
                flash.FadeIn().Then().FadeOut(3000);
                superFlash.FadeIn().Then().FadeOut(800, Easing.OutQuint);
            }
        }
    }
}
