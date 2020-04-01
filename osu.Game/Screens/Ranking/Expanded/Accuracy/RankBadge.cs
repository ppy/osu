// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Accuracy
{
    /// <summary>
    /// Contains a <see cref="DrawableRank"/> that is positioned around the <see cref="AccuracyCircle"/>.
    /// </summary>
    public class RankBadge : CompositeDrawable
    {
        /// <summary>
        /// The accuracy value corresponding to the <see cref="ScoreRank"/> displayed by this badge.
        /// </summary>
        public readonly float Accuracy;

        private readonly ScoreRank rank;

        private Drawable rankContainer;
        private Drawable overlay;

        /// <summary>
        /// Creates a new <see cref="RankBadge"/>.
        /// </summary>
        /// <param name="accuracy">The accuracy value corresponding to <paramref name="rank"/>.</param>
        /// <param name="rank">The <see cref="ScoreRank"/> to be displayed in this <see cref="RankBadge"/>.</param>
        public RankBadge(float accuracy, ScoreRank rank)
        {
            Accuracy = accuracy;
            this.rank = rank;

            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = rankContainer = new Container
            {
                Origin = Anchor.Centre,
                Size = new Vector2(28, 14),
                Children = new[]
                {
                    new DrawableRank(rank),
                    overlay = new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Blending = BlendingParameters.Additive,
                        Masking = true,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Colour = OsuColour.ForRank(rank).Opacity(0.2f),
                            Radius = 10,
                        },
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Shows this <see cref="RankBadge"/>.
        /// </summary>
        public void Appear()
        {
            this.FadeIn(50);
            overlay.FadeIn().FadeOut(500, Easing.In);
        }

        protected override void Update()
        {
            base.Update();

            // Starts at -90deg (top) and moves counter-clockwise by the accuracy
            rankContainer.Position = circlePosition(-MathF.PI / 2 - (1 - Accuracy) * MathF.PI * 2);
        }

        private Vector2 circlePosition(float t)
            => DrawSize / 2 + new Vector2(MathF.Cos(t), MathF.Sin(t)) * DrawSize / 2;
    }
}
