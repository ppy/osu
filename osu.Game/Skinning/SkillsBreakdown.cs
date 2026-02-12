// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Linq;
using System.Threading;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Framework.Layout;

namespace osu.Game.Skinning
{
    [UsedImplicitly]
    public partial class SkillsBreakdown : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;
        private IBindable<StarDifficulty> starDifficulty = null!;
        private CancellationTokenSource? cancellationSource;

        private Color4[] colors = null!;
        private Container<SkillCircle> circles = null!;

        public SkillsBreakdown()
        {
            Size = new Vector2(50);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colorSource)
        {
            InternalChild = circles = new Container<SkillCircle>
            {
                RelativeSizeAxes = Axes.Both
            };

            colors = new Color4[]
            {
                colorSource.Blue3,
                colorSource.Lime3,
                colorSource.Red3,
                colorSource.YellowDark
            };

            foreach (var color in colors)
                circles.Add(new SkillCircle(color, 1, 1));

            circles.First().SetProgress(0, 1);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b =>
            {
                cancellationSource?.Cancel();
                starDifficulty = difficultyCache.GetBindableDifficulty(b.NewValue.BeatmapInfo, (cancellationSource = new CancellationTokenSource()).Token);
                starDifficulty.BindValueChanged(d => updateSkillsBreakdown(d.NewValue));
            }, true);
        }

        private void updateSkillsBreakdown(StarDifficulty starDifficulty)
        {
            if (starDifficulty.DifficultyAttributes == null)
                return;

            var skillNameValues = starDifficulty.DifficultyAttributes.GetSkillValues();

            // Square the values to make visual representation more intuitive
            double[] skillValues = skillNameValues.Select(x => Math.Pow(x.Value, 1)).ToArray();
            LocalisableString[] skillNames = skillNameValues.Select(x => x.SkillName).ToArray();

            double sum = skillValues.Sum();
            if (sum == 0) sum = 1;

            double[] skillValuesNormalized = skillValues.Select(x => x / sum).ToArray();

            double cumulativeValue = 0;
            for (int i = 0; i < circles.Count; i++)
            {
                double nextCumulativeValue = i < skillValuesNormalized.Length ? cumulativeValue + skillValuesNormalized[i] : 1;

                circles[i].SetProgress(Math.Round(cumulativeValue, 5), Math.Round(nextCumulativeValue, 5));
                circles[i].SkillName = i < skillNames.Length ? skillNames[i] : "";

                cumulativeValue = nextCumulativeValue;
            }
        }

        private partial class SkillCircle : Container
        {
            public LocalisableString SkillName { set => innerCircle.SkillName = value; }

            // Same as StarRatingDisplay
            private const double animation_duration = 700;
            private const Easing animation_type = Easing.OutQuint;

            private TooltipCircularProgress innerCircle;
            public SkillCircle(Color4 color, double startProgress, double endProgress)
            {
                RelativeSizeAxes = Axes.Both;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                Child = innerCircle = new TooltipCircularProgress
                {
                    InnerRadius = 0.5f,
                    Colour = color,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both
                };

                SetProgress(startProgress, endProgress);
            }

            public void SetProgress(double startProgress, double endProgress)
            {
                this.RotateTo((float)startProgress * 360, animation_duration, animation_type);
                innerCircle.ProgressTo(endProgress - startProgress, animation_duration, animation_type);
            }

            private partial class TooltipCircularProgress : CircularProgress, IHasTooltip
            {
                public LocalisableString SkillName;
                LocalisableString IHasTooltip.TooltipText => SkillName;

                protected override void LoadComplete()
                {
                    base.LoadComplete();
                    precalcInputData();
                }

                protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
                {
                    bool result = base.OnInvalidate(invalidation, source);
                    precalcInputData();
                    return result;
                }

                private void precalcInputData()
                {
                    center = ScreenSpaceDrawQuad.Centre;

                    float radius = ScreenSpaceDrawQuad.Width / 2;
                    innerDistanceSquared = MathF.Pow(radius * InnerRadius, 2);
                    outerDistanceSquared = MathF.Pow(radius, 2);

                    startAngle = Parent.Rotation * MathF.PI / 180;
                    endAngle = startAngle + (float)Progress * 2 * MathF.PI;
                }

                private Vector2 center;
                private double innerDistanceSquared, outerDistanceSquared;
                private float startAngle, endAngle;
                private float deltaAngle;

                public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
                {
                    Vector2 deltaVector = screenSpacePos - center;
                    double distanceSquared = deltaVector.X * deltaVector.X + deltaVector.Y * deltaVector.Y;

                    if (distanceSquared > outerDistanceSquared || distanceSquared < innerDistanceSquared)
                        return false;

                    deltaAngle = MathF.Atan2(-deltaVector.X, deltaVector.Y) + MathF.PI;

                    return deltaAngle > startAngle && deltaAngle < endAngle;

                }
            }
        }
    }
}
