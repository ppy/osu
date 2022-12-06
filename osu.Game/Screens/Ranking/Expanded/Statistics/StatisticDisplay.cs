// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    /// <summary>
    /// A statistic from the score to be displayed in the <see cref="ExpandedPanelMiddleContent"/>.
    /// </summary>
    public abstract partial class StatisticDisplay : CompositeDrawable
    {
        protected SpriteText HeaderText { get; private set; }

        private readonly LocalisableString header;
        private Drawable content;

        /// <summary>
        /// Creates a new <see cref="StatisticDisplay"/>.
        /// </summary>
        /// <param name="header">The name of the statistic.</param>
        protected StatisticDisplay(LocalisableString header)
        {
            this.header = header;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new[]
                {
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 12,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4Extensions.FromHex("#222")
                            },
                            HeaderText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.Torus.With(size: 12, weight: FontWeight.SemiBold),
                                Text = header.ToUpper(),
                            }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            content = CreateContent().With(d =>
                            {
                                d.Anchor = Anchor.TopCentre;
                                d.Origin = Anchor.TopCentre;
                                d.Alpha = 0;
                                d.AlwaysPresent = true;
                            }),
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Shows the statistic value.
        /// </summary>
        public virtual void Appear() => content.FadeIn(100);

        /// <summary>
        /// Creates the content for this <see cref="StatisticDisplay"/>.
        /// </summary>
        protected abstract Drawable CreateContent();
    }
}
