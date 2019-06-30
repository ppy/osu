// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
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
    public class DrawableRank : CompositeDrawable
    {
        private readonly ScoreRank rank;

        private readonly Box background;
        private readonly Triangles triangles;
        private readonly OsuSpriteText name;

        public DrawableRank(ScoreRank rank)
        {
            this.rank = rank;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            FillAspectRatio = 2;

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
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        triangles = new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            TriangleScale = 1,
                            Velocity = 0.5f,
                        },
                        name = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding { Top = 5 },
                            Font = OsuFont.GetFont(Typeface.Venera, 25),
                            Text = getRankName(),
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            var rankColour = getRankColour(colours);
            background.Colour = rankColour;
            triangles.ColourDark = rankColour.Darken(0.3f);
            triangles.ColourLight = rankColour.Lighten(0.1f);

            name.Colour = getRankNameColour(colours);
        }

        private string getRankName() => rank.GetDescription().TrimEnd('+');

        /// <summary>
        ///  Retrieves the grade background colour.
        /// </summary>
        private Color4 getRankColour(OsuColour colours)
        {
            switch (rank)
            {
                case ScoreRank.XH:
                case ScoreRank.X:
                    return colours.PinkDarker;

                case ScoreRank.SH:
                case ScoreRank.S:
                    return Color4.DarkCyan;

                case ScoreRank.A:
                    return colours.Green;

                case ScoreRank.B:
                    return Color4.Orange;

                case ScoreRank.C:
                    return Color4.OrangeRed;

                default:
                    return colours.Red;
            }
        }

        /// <summary>
        ///  Retrieves the grade text colour.
        /// </summary>
        private ColourInfo getRankNameColour(OsuColour colours)
        {
            switch (rank)
            {
                case ScoreRank.XH:
                case ScoreRank.SH:
                    return ColourInfo.GradientVertical(Color4.White, Color4.LightGray);

                case ScoreRank.X:
                case ScoreRank.S:
                    return ColourInfo.GradientVertical(Color4.Yellow, Color4.Orange);

                default:
                    return getRankColour(colours).Darken(2);
            }
        }
    }
}
