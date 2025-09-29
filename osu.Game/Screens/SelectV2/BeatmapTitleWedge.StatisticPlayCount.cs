// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapTitleWedge
    {
        public partial class StatisticPlayCount : Statistic, IHasCustomTooltip<StatisticPlayCount.Data>
        {
            public Data? Value
            {
                set
                {
                    base.Text = value?.Total < 0 ? "-" : value?.Total.ToLocalisableString("N0");
                    TooltipContent = value;
                }
            }

            public new LocalisableString? Text
            {
                set => throw new InvalidOperationException($"Use {nameof(Value)} instead.");
            }

            public Data? TooltipContent { get; private set; }

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public StatisticPlayCount(bool background = false, float leftPadding = 10, float? minSize = null)
                : base(OsuIcon.Play, background, leftPadding, minSize)
            {
            }

            ITooltip<Data> IHasCustomTooltip<Data>.GetCustomTooltip() => new PlayCountTooltip(colourProvider);

            public record Data(int Total, int User);

            private partial class PlayCountTooltip : VisibilityContainer, ITooltip<Data>
            {
                private readonly OverlayColourProvider colourProvider;

                private OsuSpriteText totalPlaysText = null!;
                private OsuSpriteText personalPlaysText = null!;

                public PlayCountTooltip(OverlayColourProvider colourProvider)
                {
                    this.colourProvider = colourProvider;
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    AutoSizeAxes = Axes.Both;
                    CornerRadius = 10;
                    Masking = true;

                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Colour = Color4.Black.Opacity(0.25f),
                        Radius = 10f,
                    };

                    InternalChildren = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background4,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding(10),
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(16f, 0f),
                            Children = new[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Colour = colourProvider.Content2,
                                            Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                            Text = SongSelectStrings.TotalPlays,
                                        },
                                        totalPlaysText = new OsuSpriteText
                                        {
                                            Colour = colourProvider.Content1,
                                            Font = OsuFont.Style.Heading1.With(weight: FontWeight.Regular),
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Colour = colourProvider.Content2,
                                            Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                            Text = SongSelectStrings.PersonalPlays,
                                        },
                                        personalPlaysText = new OsuSpriteText
                                        {
                                            Colour = colourProvider.Content1,
                                            Font = OsuFont.Style.Heading1.With(weight: FontWeight.Regular),
                                        },
                                    }
                                },
                            }
                        },
                    };
                }

                public void SetContent(Data content)
                {
                    totalPlaysText.Text = content.Total < 0 ? "-" : content.Total.ToLocalisableString("N0");
                    personalPlaysText.Text = content.User < 0 ? "-" : content.User.ToLocalisableString("N0");
                }

                public void Move(Vector2 pos) => Position = pos;

                protected override void PopIn() => this.FadeIn(300, Easing.OutQuint);
                protected override void PopOut() => this.FadeOut(300, Easing.OutQuint);
            }
        }
    }
}
