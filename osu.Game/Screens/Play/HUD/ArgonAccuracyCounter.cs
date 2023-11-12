// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonAccuracyCounter : GameplayAccuracyCounter, ISerialisableDrawable
    {
        protected override double RollingDuration => 500;
        protected override Easing RollingEasing => Easing.OutQuint;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wire frames behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.25f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        public bool UsesFixedAnchor { get; set; }

        protected override IHasText CreateText() => new ArgonAccuracyTextComponent
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
        };

        private partial class ArgonAccuracyTextComponent : CompositeDrawable, IHasText
        {
            private readonly ArgonCounterTextComponent wholePart;
            private readonly ArgonCounterTextComponent fractionPart;

            public IBindable<float> WireframeOpacity { get; } = new BindableFloat();

            public LocalisableString Text
            {
                get => wholePart.Text;
                set
                {
                    string[] split = value.ToString().Replace("%", string.Empty).Split(".");

                    wholePart.Text = split[0];
                    fractionPart.Text = "." + split[1];
                }
            }

            public ArgonAccuracyTextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            AutoSizeAxes = Axes.Both,
                            Child = wholePart = new ArgonCounterTextComponent(Anchor.TopRight, "ACCURACY")
                            {
                                RequiredDisplayDigits = { Value = 3 },
                                WireframeOpacity = { BindTarget = WireframeOpacity }
                            }
                        },
                        fractionPart = new ArgonCounterTextComponent(Anchor.TopLeft)
                        {
                            Margin = new MarginPadding { Top = 12f * 2f + 4f }, // +4 to account for the extra spaces above the digits.
                            WireframeOpacity = { BindTarget = WireframeOpacity },
                            Scale = new Vector2(0.5f),
                        },
                        new ArgonCounterTextComponent(Anchor.TopLeft)
                        {
                            Text = @"%",
                            Margin = new MarginPadding { Top = 12f },
                            WireframeOpacity = { BindTarget = WireframeOpacity }
                        },
                    }
                };
            }
        }
    }
}
