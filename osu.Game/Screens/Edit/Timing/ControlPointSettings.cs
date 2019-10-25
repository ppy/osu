// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Timing
{
    public class ControlPointSettings : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Gray3,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = createSections()
                    },
                }
            };
        }

        private IReadOnlyList<Drawable> createSections() => new Drawable[]
        {
            new TimingSection(),
            new DifficultySection(),
            new SampleSection(),
            new EffectSection(),
        };

        private class TimingSection : Section<TimingControlPoint>
        {
            private OsuSpriteText bpm;
            private OsuSpriteText timeSignature;

            [BackgroundDependencyLoader]
            private void load()
            {
                Flow.AddRange(new[]
                {
                    bpm = new OsuSpriteText(),
                    timeSignature = new OsuSpriteText(),
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ControlPoint.BindValueChanged(point =>
                {
                    bpm.Text = $"BPM: {point.NewValue?.BeatLength}";
                    timeSignature.Text = $"Signature: {point.NewValue?.TimeSignature}";
                });
            }
        }

        private class DifficultySection : Section<DifficultyControlPoint>
        {
            private OsuSpriteText multiplier;

            [BackgroundDependencyLoader]
            private void load()
            {
                Flow.AddRange(new[]
                {
                    multiplier = new OsuSpriteText(),
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ControlPoint.BindValueChanged(point => { multiplier.Text = $"Multiplier: {point.NewValue?.SpeedMultiplier}"; });
            }
        }

        private class SampleSection : Section<SampleControlPoint>
        {
            private OsuSpriteText bank;
            private OsuSpriteText volume;

            [BackgroundDependencyLoader]
            private void load()
            {
                Flow.AddRange(new[]
                {
                    bank = new OsuSpriteText(),
                    volume = new OsuSpriteText(),
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ControlPoint.BindValueChanged(point =>
                {
                    bank.Text = $"Bank: {point.NewValue?.SampleBank}";
                    volume.Text = $"Volume: {point.NewValue?.SampleVolume}";
                });
            }
        }

        private class EffectSection : Section<EffectControlPoint>
        {
            private OsuSpriteText kiai;
            private OsuSpriteText omitBarLine;

            [BackgroundDependencyLoader]
            private void load()
            {
                Flow.AddRange(new[]
                {
                    kiai = new OsuSpriteText(),
                    omitBarLine = new OsuSpriteText(),
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ControlPoint.BindValueChanged(point =>
                {
                    kiai.Text = $"Kiai: {point.NewValue?.KiaiMode}";
                    omitBarLine.Text = $"Skip Bar Line: {point.NewValue?.OmitFirstBarLine}";
                });
            }
        }

        private class Section<T> : CompositeDrawable
            where T : ControlPoint
        {
            private OsuCheckbox checkbox;
            private Container content;

            protected FillFlowContainer Flow { get; private set; }

            protected Bindable<T> ControlPoint { get; } = new Bindable<T>();

            private const float header_height = 20;

            [Resolved]
            private Bindable<ControlPointGroup> selectedPoints { get; set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeDuration = 200;
                AutoSizeEasing = Easing.OutQuint;
                AutoSizeAxes = Axes.Y;

                Masking = true;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.Gray1,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = header_height,
                        Children = new Drawable[]
                        {
                            checkbox = new OsuCheckbox
                            {
                                LabelText = typeof(T).Name.Replace(typeof(ControlPoint).Name, string.Empty)
                            }
                        }
                    },
                    content = new Container
                    {
                        Y = header_height,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colours.Gray2,
                                RelativeSizeAxes = Axes.Both,
                            },
                            Flow = new FillFlowContainer
                            {
                                Padding = new MarginPadding(10),
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                            },
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                selectedPoints.BindValueChanged(points =>
                {
                    ControlPoint.Value = points.NewValue?.ControlPoints.OfType<T>().FirstOrDefault();

                    checkbox.Current.Value = ControlPoint.Value != null;
                }, true);

                checkbox.Current.BindValueChanged(selected => { content.BypassAutoSizeAxes = selected.NewValue ? Axes.None : Axes.Y; }, true);
            }
        }
    }
}
