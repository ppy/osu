// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osuTK;


namespace osu.Game.Overlays.Mods
{
    public partial class ModMapInfoDisplay : Container, IHasCurrentValue<double>
    {
        public const float HEIGHT = 42;
        private const float transition_duration = 200;

        private readonly Box contentBackground;
        private readonly Box labelBackground;
        private readonly FillFlowContainer content;

        //public Bindable<double> Current
        //{
        //    get => current.Current;
        //    set => current.Current = value;
        //}
        //private readonly BindableWithCurrent<double> current = new BindableWithCurrent<double>();

        public Bindable<double> Current { get; set; } = new BindableWithCurrent<double>();

        //[Resolved]
        //private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        protected Func<double, osuTK.Graphics.Color4> GetColor;

        /// <summary>
        /// Text to display in the left area of the display.
        /// </summary>
        protected LocalisableString Label;

        protected virtual float ValueAreaWidth => 56;

        protected virtual string CounterFormat => @"0.00";

        protected override Container<Drawable> Content => content;

        protected readonly RollingCounter<double> Counter;

        public ModMapInfoDisplay(LocalisableString label, Func<double, osuTK.Graphics.Color4> colorFunc)
        {
            Label = label;
            GetColor = colorFunc;
            Height = HEIGHT;
            AutoSizeAxes = Axes.X;

            InternalChild = new InputBlockingContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = ModSelectPanel.CORNER_RADIUS,
                Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0),
                Children = new Drawable[]
                {
                    contentBackground = new Box
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = ValueAreaWidth + ModSelectPanel.CORNER_RADIUS
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, ValueAreaWidth)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Masking = true,
                                    CornerRadius = ModSelectPanel.CORNER_RADIUS,
                                    Children = new Drawable[]
                                    {
                                        labelBackground = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Margin = new MarginPadding { Horizontal = 18 },
                                            Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                            Text = Label,
                                            Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
                                        }
                                    }
                                },
                                content = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Direction = FillDirection.Horizontal,
                                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    Spacing = new Vector2(2, 0),
                                    Child = Counter = new EffectCounter(CounterFormat)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Current = { BindTarget = Current }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            labelBackground.Colour = colourProvider.Background4;
        }

        protected override void LoadComplete()
        {
            Current.BindValueChanged(e =>
            {
                //var effect = CalculateEffectForComparison(e.NewValue.CompareTo(Current.Default));
                setColours(e.NewValue);
            }, true);
        }

        /// <summary>
        /// Fades colours of text and its background according to displayed value.
        /// </summary>
        /// <param name="value">value</param>
        private void setColours(double value)
        {
            contentBackground.FadeColour(GetColor(value), transition_duration, Easing.OutQuint);
        }

        private partial class EffectCounter : RollingCounter<double>
        {
            private readonly string? format;

            public EffectCounter(string? format)
            {
                this.format = format;
            }

            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString(format);

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
            };
        }
    }
}
