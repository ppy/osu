// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Screens.Ranking.Statistics;

namespace osu.Game.Screens.RankingV2.Extended
{
    public abstract partial class StatisticItemPanel : Container
    {
        public LocalisableString HeaderText { get; }

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        protected StatisticItemPanel(LocalisableString headerText)
        {
            HeaderText = headerText;
            content = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;

            InternalChildren =
            [
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.6f,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Direction = FillDirection.Vertical,
                    Children =
                    [
                        new StatisticItemHeader
                        {
                            Text = HeaderText,
                        },
                        content,
                    ],
                },
            ];
        }
    }
}
