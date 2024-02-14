// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Solo;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Overlays.Toolbar
{
    public partial class TransientUserStatisticsUpdateDisplay : CompositeDrawable
    {
        public Bindable<SoloStatisticsUpdate?> LatestUpdate { get; } = new Bindable<SoloStatisticsUpdate?>();

        private Statistic<int> globalRank = null!;
        private Statistic<decimal> pp = null!;

        [BackgroundDependencyLoader]
        private void load(SoloStatisticsWatcher? soloStatisticsWatcher)
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
            Alpha = 0;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Padding = new MarginPadding { Horizontal = 10 },
                Spacing = new Vector2(10),
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    globalRank = new Statistic<int>(UsersStrings.ShowRankGlobalSimple, @"#", Comparer<int>.Create((before, after) => before - after)),
                    pp = new Statistic<decimal>(RankingsStrings.StatPerformance, string.Empty, Comparer<decimal>.Create((before, after) => Math.Sign(after - before))),
                }
            };

            if (soloStatisticsWatcher != null)
                ((IBindable<SoloStatisticsUpdate?>)LatestUpdate).BindTo(soloStatisticsWatcher.LatestUpdate);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LatestUpdate.BindValueChanged(val =>
            {
                if (val.NewValue == null)
                    return;

                var update = val.NewValue;

                // null handling here is best effort because it is annoying.

                globalRank.Alpha = update.After.GlobalRank == null ? 0 : 1;
                pp.Alpha = update.After.PP == null ? 0 : 1;

                if (globalRank.Alpha == 0 && pp.Alpha == 0)
                    return;

                FinishTransforms(true);

                this.FadeIn(500, Easing.OutQuint);

                if (update.After.GlobalRank != null)
                {
                    globalRank.Display(
                        update.Before.GlobalRank ?? update.After.GlobalRank.Value,
                        Math.Abs((update.After.GlobalRank.Value - update.Before.GlobalRank) ?? 0),
                        update.After.GlobalRank.Value);
                }

                if (update.After.PP != null)
                    pp.Display(update.Before.PP ?? update.After.PP.Value, Math.Abs((update.After.PP - update.Before.PP) ?? 0M), update.After.PP.Value);

                this.Delay(5000).FadeOut(500, Easing.OutQuint);
            });
        }

        private partial class Statistic<T> : CompositeDrawable
            where T : struct, IEquatable<T>, IFormattable
        {
            private readonly LocalisableString title;
            private readonly string mainValuePrefix;
            private readonly IComparer<T> valueComparer;

            private Counter<T> mainValue = null!;
            private Counter<T> deltaValue = null!;
            private OsuSpriteText titleText = null!;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public Statistic(LocalisableString title, string mainValuePrefix, IComparer<T> valueComparer)
            {
                this.title = title;
                this.mainValuePrefix = mainValuePrefix;
                this.valueComparer = valueComparer;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Y;
                AutoSizeAxes = Axes.X;
                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        mainValue = new Counter<T>
                        {
                            ValuePrefix = mainValuePrefix,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Font = OsuFont.GetFont(),
                        },
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children = new Drawable[]
                            {
                                deltaValue = new Counter<T>
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Font = OsuFont.GetFont(size: 12, fixedWidth: false, weight: FontWeight.SemiBold),
                                    AlwaysPresent = true,
                                },
                                titleText = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold),
                                    Text = title,
                                    AlwaysPresent = true,
                                }
                            }
                        }
                    }
                };
            }

            public void Display(T before, T delta, T after)
            {
                int comparison = valueComparer.Compare(before, after);

                if (comparison > 0)
                {
                    deltaValue.Colour = colours.Lime1;
                    deltaValue.ValuePrefix = "+";
                }
                else if (comparison < 0)
                {
                    deltaValue.Colour = colours.Red1;
                    deltaValue.ValuePrefix = "-";
                }
                else
                {
                    deltaValue.Colour = Colour4.White;
                    deltaValue.ValuePrefix = string.Empty;
                }

                mainValue.SetCountWithoutRolling(before);
                deltaValue.SetCountWithoutRolling(delta);

                titleText.Alpha = 1;
                deltaValue.Alpha = 0;

                using (BeginDelayedSequence(1500))
                {
                    titleText.FadeOut(250, Easing.OutQuint);
                    deltaValue.FadeIn(250, Easing.OutQuint)
                              .Then().Delay(1500)
                              .Then().Schedule(() =>
                              {
                                  mainValue.Current.Value = after;
                                  deltaValue.Current.SetDefault();
                              });
                }
            }
        }

        private partial class Counter<T> : RollingCounter<T>
            where T : struct, IEquatable<T>, IFormattable
        {
            public const double ROLLING_DURATION = 500;

            public FontUsage Font { get; init; } = OsuFont.Default;

            public string ValuePrefix
            {
                get => valuePrefix;
                set
                {
                    valuePrefix = value;
                    UpdateDisplay();
                }
            }

            private string valuePrefix = string.Empty;

            protected override LocalisableString FormatCount(T count) => LocalisableString.Format(@"{0}{1:N0}", ValuePrefix, count);
            protected override OsuSpriteText CreateSpriteText() => base.CreateSpriteText().With(t => t.Font = Font);
            protected override double RollingDuration => ROLLING_DURATION;
        }
    }
}
