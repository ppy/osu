// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.Leaderboards
{
    public partial class DrawableRank : CompositeDrawable
    {
        public DrawableRank(ScoreRank rank)
        {
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            FillAspectRatio = 2;

            var rankColour = OsuColour.ForRank(rank);
            InternalChild = new DrawSizePreservingFillContainer
            {
                TargetDrawSize = new Vector2(64, 32),
                Strategy = DrawSizePreservationStrategy.Minimum,
                Child = new CircularContainer
                {
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = rankColour,
                        },
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourDark = rankColour.Darken(0.1f),
                            ColourLight = rankColour.Lighten(0.1f),
                            Velocity = 0.25f,
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(-3, 0),
                            Padding = new MarginPadding { Top = 5 },
                            Colour = GetRankLetterColour(rank),
                            Font = OsuFont.Numeric.With(size: 25),
                            Text = GetRankLetter(rank),
                            ShadowColour = Color4.Black.Opacity(0.3f),
                            ShadowOffset = new Vector2(0, 0.08f),
                            Shadow = true,
                        },
                    }
                }
            };
        }

        /// <summary>
        /// Returns letters to be shown in places where ranks are shown on a badge or similar to the user.
        /// </summary>
        public static string GetRankLetter(ScoreRank rank)
        {
            switch (rank)
            {
                case ScoreRank.SH:
                    return @"S";

                case ScoreRank.X:
                case ScoreRank.XH:
                    return @"SS";

                default:
                    return rank.ToString();
            }
        }

        /// <summary>
        ///  Retrieves the grade text colour.
        /// </summary>
        public static ColourInfo GetRankLetterColour(ScoreRank rank)
        {
            switch (rank)
            {
                case ScoreRank.XH:
                case ScoreRank.SH:
                    return ColourInfo.GradientVertical(Color4.White, Color4Extensions.FromHex("afdff0"));

                case ScoreRank.X:
                case ScoreRank.S:
                    return ColourInfo.GradientVertical(Color4Extensions.FromHex(@"ffe7a8"), Color4Extensions.FromHex(@"ffb800"));

                case ScoreRank.A:
                    return Color4Extensions.FromHex(@"275227");

                case ScoreRank.B:
                    return Color4Extensions.FromHex(@"553a2b");

                case ScoreRank.C:
                    return Color4Extensions.FromHex(@"473625");

                case ScoreRank.D:
                    return Color4Extensions.FromHex(@"512525");

                case ScoreRank.F:
                default:
                    return Color4Extensions.FromHex(@"CC3333");
            }
        }
    }
}
