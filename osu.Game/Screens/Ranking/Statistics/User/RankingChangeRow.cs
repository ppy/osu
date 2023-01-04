// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Solo;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Ranking.Statistics.User
{
    public abstract partial class RankingChangeRow<T> : CompositeDrawable
    {
        public Bindable<SoloStatisticsUpdate?> StatisticsUpdate { get; } = new Bindable<SoloStatisticsUpdate?>();

        private readonly Func<UserStatistics, T> accessor;

        private OsuSpriteText currentValueText = null!;
        private SpriteIcon changeIcon = null!;
        private OsuSpriteText changeText = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected RankingChangeRow(
            Func<UserStatistics, T> accessor)
        {
            this.accessor = accessor;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = Label,
                    Font = OsuFont.Default.With(size: 18)
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                changeIcon = new SpriteIcon
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Size = new Vector2(18)
                                },
                                currentValueText = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Font = OsuFont.Default.With(size: 18, weight: FontWeight.Bold)
                                },
                            }
                        },
                        changeText = new OsuSpriteText
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Font = OsuFont.Default.With(weight: FontWeight.Bold)
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            StatisticsUpdate.BindValueChanged(onStatisticsUpdate, true);
        }

        private void onStatisticsUpdate(ValueChangedEvent<SoloStatisticsUpdate?> statisticsUpdate)
        {
            var update = statisticsUpdate.NewValue;

            if (update == null)
                return;

            T previousValue = accessor.Invoke(update.Before);
            T currentValue = accessor.Invoke(update.After);
            int comparisonResult = CalculateDifference(previousValue, currentValue, out var formattedDifference);

            Colour4 comparisonColour;
            IconUsage icon;

            if (comparisonResult < 0)
            {
                comparisonColour = colours.Red1;
                icon = FontAwesome.Solid.ArrowDown;
            }
            else if (comparisonResult > 0)
            {
                comparisonColour = colours.Lime1;
                icon = FontAwesome.Solid.ArrowUp;
            }
            else
            {
                comparisonColour = colours.Orange1;
                icon = FontAwesome.Solid.Minus;
            }

            currentValueText.Text = FormatCurrentValue(currentValue);

            changeIcon.Icon = icon;
            changeIcon.Colour = comparisonColour;

            changeText.Text = formattedDifference;
            changeText.Colour = comparisonColour;
        }

        protected abstract LocalisableString Label { get; }

        protected abstract LocalisableString FormatCurrentValue(T current);
        protected abstract int CalculateDifference(T previous, T current, out LocalisableString formattedDifference);
    }
}
