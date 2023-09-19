// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public partial class DrawableGrade : CompositeDrawable
    {
        private readonly Grade grade;

        public DrawableGrade(Grade grade)
        {
            this.grade = grade;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            FillAspectRatio = 2;

            var rankColour = OsuColour.ForRank(grade);
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
                            Colour = getRankNameColour(),
                            Font = OsuFont.Numeric.With(size: 25),
                            Text = GetGradeName(grade),
                            ShadowColour = Color4.Black.Opacity(0.3f),
                            ShadowOffset = new Vector2(0, 0.08f),
                            Shadow = true,
                        },
                    }
                }
            };
        }

        public static string GetGradeName(Grade grade) => grade.GetDescription().TrimEnd('+');

        /// <summary>
        ///  Retrieves the grade text colour.
        /// </summary>
        private ColourInfo getRankNameColour()
        {
            switch (grade)
            {
                case Grade.XH:
                case Grade.SH:
                    return ColourInfo.GradientVertical(Color4.White, Color4Extensions.FromHex("afdff0"));

                case Grade.X:
                case Grade.S:
                    return ColourInfo.GradientVertical(Color4Extensions.FromHex(@"ffe7a8"), Color4Extensions.FromHex(@"ffb800"));

                case Grade.A:
                    return Color4Extensions.FromHex(@"275227");

                case Grade.B:
                    return Color4Extensions.FromHex(@"553a2b");

                case Grade.C:
                    return Color4Extensions.FromHex(@"473625");

                default:
                    return Color4Extensions.FromHex(@"512525");
            }
        }
    }
}
