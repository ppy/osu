// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.OnlinePlay.DailyChallenge.Events;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.DailyChallenge
{
    public partial class DailyChallengeTotalsDisplay : CompositeDrawable
    {
        private Container passCountContainer = null!;
        private TotalRollingCounter passCounter = null!;
        private Container totalScoreContainer = null!;
        private TotalRollingCounter totalScoreCounter = null!;

        private long totalPassCountInstantaneous;
        private long cumulativeTotalScoreInstantaneous;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions =
                [
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                ],
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SectionHeader("Total pass count")
                    },
                    new Drawable[]
                    {
                        passCountContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = passCounter = new TotalRollingCounter
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        }
                    },
                    new Drawable[]
                    {
                        new SectionHeader("Cumulative total score")
                    },
                    new Drawable[]
                    {
                        totalScoreContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Child = totalScoreCounter = new TotalRollingCounter
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        }
                    },
                }
            };
        }

        public void SetInitialCounts(long totalPassCount, long cumulativeTotalScore)
        {
            totalPassCountInstantaneous = totalPassCount;
            cumulativeTotalScoreInstantaneous = cumulativeTotalScore;
        }

        public void AddNewScore(NewScoreEvent ev)
        {
            totalPassCountInstantaneous += 1;
            cumulativeTotalScoreInstantaneous += ev.TotalScore;
        }

        protected override void Update()
        {
            base.Update();

            passCounter.Current.Value = totalPassCountInstantaneous;
            totalScoreCounter.Current.Value = cumulativeTotalScoreInstantaneous;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var totalPassCountProportionOfParent = Vector2.Divide(passCountContainer.DrawSize, passCounter.DrawSize);
            passCounter.Scale = new Vector2(Math.Min(Math.Min(totalPassCountProportionOfParent.X, totalPassCountProportionOfParent.Y) * 0.8f, 1));

            var totalScoreTextProportionOfParent = Vector2.Divide(totalScoreContainer.DrawSize, totalScoreCounter.DrawSize);
            totalScoreCounter.Scale = new Vector2(Math.Min(Math.Min(totalScoreTextProportionOfParent.X, totalScoreTextProportionOfParent.Y) * 0.8f, 1));
        }

        private partial class TotalRollingCounter : RollingCounter<long>
        {
            protected override double RollingDuration => 1000;

            protected override Easing RollingEasing => Easing.OutPow10;

            protected override bool IsRollingProportional => true;

            protected override double GetProportionalDuration(long currentValue, long newValue)
            {
                long change = Math.Abs(newValue - currentValue);

                if (change < 10)
                    return 0;

                return Math.Min(6000, RollingDuration * Math.Sqrt(change) / 100);
            }

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 80f, fixedWidth: true),
                Spacing = new Vector2(-4, 0)
            };

            protected override LocalisableString FormatCount(long count) => count.ToLocalisableString(@"N0");
        }
    }
}
