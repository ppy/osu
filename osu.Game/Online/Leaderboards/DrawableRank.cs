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
    public class DrawableRank : CompositeDrawable
    {
        public DrawableRank(ScoreRank rank)
        {
            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;
            FillAspectRatio = 2;

            var rankColour = rank switch
            {
                ScoreRank.XH => OsuColour.FromHex(@"ce1c9d"),
                ScoreRank.X => OsuColour.FromHex(@"ce1c9d"),
                ScoreRank.SH => OsuColour.FromHex(@"00a8b5"),
                ScoreRank.S => OsuColour.FromHex(@"00a8b5"),
                ScoreRank.A => OsuColour.FromHex(@"7cce14"),
                ScoreRank.B => OsuColour.FromHex(@"e3b130"),
                ScoreRank.C => OsuColour.FromHex(@"f18252"),
                _ => OsuColour.FromHex(@"e95353"),
            };
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
                            Colour = rank switch
                            {
                                ScoreRank.XH => ColourInfo.GradientVertical(Color4.White, OsuColour.FromHex("afdff0")),
                                ScoreRank.SH => ColourInfo.GradientVertical(Color4.White, OsuColour.FromHex("afdff0")),
                                ScoreRank.X => ColourInfo.GradientVertical(OsuColour.FromHex(@"ffe7a8"), OsuColour.FromHex(@"ffb800")),
                                ScoreRank.S => ColourInfo.GradientVertical(OsuColour.FromHex(@"ffe7a8"), OsuColour.FromHex(@"ffb800")),
                                ScoreRank.A => OsuColour.FromHex(@"275227"),
                                ScoreRank.B => OsuColour.FromHex(@"553a2b"),
                                ScoreRank.C => OsuColour.FromHex(@"473625"),
                                _ => OsuColour.FromHex(@"512525"),
                            },
                            Font = OsuFont.GetFont(Typeface.Venera, 25),
                            Text = rank.GetDescription().TrimEnd('+'),
                            ShadowColour = Color4.Black.Opacity(0.3f),
                            ShadowOffset = new Vector2(0, 0.08f),
                            Shadow = true,
                        },
                    }
                }
            };
        }
    }
}
