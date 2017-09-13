// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCaseBeatSnapVisualiser : TestCase
    {
        public TestCaseBeatSnapVisualiser()
        {
            Add(new BeatSnapVisualiser
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Items = new[]
                {
                    new BeatSnapDivisor(1),
                    new BeatSnapDivisor(2),
                    new BeatSnapDivisor(3),
                    new BeatSnapDivisor(4),
                    new BeatSnapDivisor(6, BeatSnapDivisorType.Uncommon),
                    new BeatSnapDivisor(8, BeatSnapDivisorType.Uncommon),
                    new BeatSnapDivisor(12, BeatSnapDivisorType.Rare),
                    new BeatSnapDivisor(16, BeatSnapDivisorType.Rare),
                    new BeatSnapDivisor(32, BeatSnapDivisorType.Rare),
                }
            });
        }

        public class BeatSnapDivisor
        {
            /// <summary>
            /// The beat divisor which elements will be snapped to.
            /// </summary>
            public readonly int Divisor;

            /// <summary>
            /// The type of this <see cref="BeatSnapDivisor"/>.
            /// </summary>
            public readonly BeatSnapDivisorType Type;

            public BeatSnapDivisor(int divisor)
            {
                if (divisor <= 0) throw new ArgumentOutOfRangeException(nameof(divisor));

                Divisor = divisor;
            }

            public BeatSnapDivisor(int divisor, BeatSnapDivisorType type)
                : this(divisor)
            {
                Type = type;
            }
        }

        public enum BeatSnapDivisorType
        {
            /// <summary>
            /// A <see cref="BeatSnapDivisor"/> that is very commonly used.
            /// </summary>
            Common,
            /// <summary>
            /// A <see cref="BeatSnapDivisor"/> which has a very specific, but uncommon, use case.
            /// </summary>
            Uncommon,
            /// <summary>
            /// A <see cref="BeatSnapDivisor"/> which should rarely, if ever, be used.
            /// </summary>
            Rare
        }

        public class BeatSnapVisualiser : CompositeDrawable, IHasCurrentValue<int>
        {
            private const float tick_width = 1;
            private const float tick_height = 8;
            private const float tick_padding = 8;
            private const float marker_height = 5;
            private const float marker_width = 10;
            private const float content_padding = 16;
            private const float inner_content_padding = 8;

            public Bindable<int> Current { get; } = new Bindable<int>();

            private readonly FillFlowContainer<Tick> ticks;
            private readonly Box background;

            private readonly Triangle marker;
            private readonly Box contentBackground;

            private readonly SpriteText text;
            private BufferedContainer textContainer;

            private int currentIndex;

            public BeatSnapVisualiser()
            {
                Size = new Vector2(94, 100);

                Masking = true;
                CornerRadius = 5;

                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 5,
                    Colour = Color4.Black,
                };

                InternalChildren = new Drawable[]
                {
                    background = new Box { RelativeSizeAxes = Axes.Both },
                    ticks = new FillFlowContainer<Tick>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Padding = new MarginPadding { Left = tick_padding, Right = tick_padding },
                        Direction = FillDirection.Horizontal
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = content_padding },
                        Children = new Drawable[]
                        {
                            marker = new Triangle
                            {
                                Origin = Anchor.TopCentre,
                                Width = marker_width,
                                Height = marker_height,
                                EdgeSmoothness = new Vector2(1f),
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Top = marker_height },
                                Children = new Drawable[]
                                {
                                    contentBackground = new Box { RelativeSizeAxes = Axes.Both },
                                    new FillFlowContainer
                                    {
                                        Name = "Inner content",
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding(inner_content_padding),
                                        Spacing = new Vector2(0, inner_content_padding),
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                Name = "Inner content top",
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                Children = new Drawable[]
                                                {
                                                    new IconButton
                                                    {
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        BypassAutoSizeAxes = Axes.Both,
                                                        Scale = new Vector2(0.67f),
                                                        Icon = FontAwesome.fa_chevron_left,
                                                        IconColour = Color4.Black,
                                                        Action = decreaseDivisor
                                                    },
                                                    (text = new OsuSpriteText { TextSize = 18, Font = "Exo2.0-Medium" }).WithEffect(new GlowEffect { CacheDrawnEffect = true }, effect =>
                                                    {
                                                        textContainer = effect;
                                                        effect.Anchor = Anchor.TopCentre;
                                                        effect.Origin = Anchor.TopCentre;
                                                    }),
                                                    new IconButton
                                                    {
                                                        Anchor = Anchor.CentreRight,
                                                        Origin = Anchor.CentreRight,
                                                        BypassAutoSizeAxes = Axes.Both,
                                                        Scale = new Vector2(0.67f),
                                                        Icon = FontAwesome.fa_chevron_right,
                                                        IconColour = Color4.Black,
                                                        Action = increaseDivisor
                                                    },
                                                }
                                            },
                                            new TextFlowContainer(s => s.TextSize = 12)
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                AutoSizeAxes = Axes.Y,
                                                RelativeSizeAxes = Axes.X,
                                                TextAnchor = Anchor.TopCentre,
                                                ParagraphSpacing = 0,
                                                LineSpacing = 0,
                                                Text = "beat snap\ndivisor"
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                Current.ValueChanged += currentValueChanged;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                background.Colour = colours.Gray1;
                marker.Colour = colours.Gray5;
                contentBackground.Colour = colours.Gray5;
                textContainer.EffectColour = colours.Blue;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (ticks.Count > 0)
                    Current.Value = ticks[0].Item.Divisor;
            }

            /// <summary>
            /// Sets the allowable <see cref="BeatSnapDivisor"/>s.
            /// </summary>
            public IReadOnlyList<BeatSnapDivisor> Items
            {
                set
                {
                    ticks.Clear();
                    value.ForEach(Add);
                }
            }

            /// <summary>
            /// Adds a <see cref="BeatSnapDivisor"/> as an allowable value of this <see cref="BeatSnapVisualiser"/>.
            /// </summary>
            /// <param name="divisor">The <see cref="BeatSnapDivisor"/> to add.</param>
            public void Add(BeatSnapDivisor divisor)
            {
                if (ticks.Any(t => t.Item.Divisor == divisor.Divisor))
                    return;
                ticks.Add(new Tick(divisor));
            }

            protected override void Update()
            {
                base.Update();

                if (ticks.Count > 1)
                {
                    // Evenly space out the ticks
                    float availableWidth = ticks.ChildSize.X - ticks.Count * tick_width;
                    ticks.Spacing = new Vector2(availableWidth / (ticks.Count - 1), 0);
                }
                else
                    ticks.Spacing = Vector2.Zero;
            }

            private void decreaseDivisor()
            {
                currentIndex = Math.Max(0, currentIndex - 1);
                Current.Value = ticks[currentIndex].Item.Divisor;
            }

            private void increaseDivisor()
            {
                currentIndex = Math.Min(ticks.Count - 1, currentIndex + 1);
                Current.Value = ticks[currentIndex].Item.Divisor;
            }

            private void currentValueChanged(int newValue)
            {
                Schedule(() =>
                {
                    int index = 0;
                    for (; index < ticks.Count; index++)
                    {
                        if (ticks[index].Item.Divisor == newValue)
                            break;
                    }

                    if (index == ticks.Count)
                        throw new ArgumentException(nameof(newValue));

                    currentIndex = index;

                    text.Text = $"1/{newValue}";
                    textContainer.ForceRedraw();

                    Schedule(() =>
                    {
                        var pos = ToLocalSpace(ticks[index].ScreenSpaceDrawQuad).Centre.X;
                        marker.MoveToX(pos, 100, Easing.OutQuint);
                    });
                });
            }

            private class Tick : Box
            {
                internal readonly BeatSnapDivisor Item;

                public Tick(BeatSnapDivisor item)
                {
                    Item = item;

                    Anchor = Anchor.TopCentre;
                    Size = new Vector2(tick_width, tick_height);
                    EdgeSmoothness = new Vector2(1, 0);
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    switch (Item.Type)
                    {
                        case BeatSnapDivisorType.Common:
                            Colour = colours.Gray5;
                            break;
                        case BeatSnapDivisorType.Uncommon:
                            Colour = colours.Yellow;
                            break;
                        case BeatSnapDivisorType.Rare:
                            Colour = colours.Red;
                            break;
                    }
                }
            }
        }
    }
}
