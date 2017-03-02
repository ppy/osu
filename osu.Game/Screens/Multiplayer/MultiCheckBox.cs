// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using System;

namespace osu.Game.Screens.Multiplayer
{
    public class MultiCheckBox : CheckBox
    {
        private Bindable<bool> bindable;

        public Bindable<bool> Bindable
        {
            set
            {
                if (bindable != null)
                    bindable.ValueChanged -= bindableValueChanged;
                bindable = value;
                if (bindable != null)
                {
                    bool state = State == CheckBoxState.Checked;
                    if (state != bindable.Value)
                        State = bindable.Value ? CheckBoxState.Checked : CheckBoxState.Unchecked;
                    bindable.ValueChanged += bindableValueChanged;
                }

                if (bindable?.Disabled ?? true)
                    Alpha = 0.3f;
            }
        }

        public string LabelText
        {
            get { return labelSpriteText?.Text; }
            set
            {
                if (labelSpriteText != null)
                    labelSpriteText.Text = value;
            }
        }

        public MarginPadding LabelPadding
        {
            get { return labelSpriteText?.Padding ?? new MarginPadding(); }
            set
            {
                if (labelSpriteText != null)
                    labelSpriteText.Padding = value;
            }
        }

        private Nub nub;
        private SpriteText labelSpriteText;

        public MultiCheckBox()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                nub = new Nub
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    BorderColour = Color4.White,
                    BorderThickness = 3,
                    Margin = new MarginPadding { Top = 2 },
                },
                labelSpriteText = new OsuSpriteText
                {
                    Font = @"Exo2.0-Bold",
                    Colour = Color4.Gold,
                    Margin = new MarginPadding { Left = 17 }
                },
            };
        }

        private void bindableValueChanged(object sender, EventArgs e)
        {
            State = bindable.Value ? CheckBoxState.Checked : CheckBoxState.Unchecked;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (bindable != null)
                bindable.ValueChanged -= bindableValueChanged;
            base.Dispose(isDisposing);
        }

        protected override bool OnHover(InputState state)
        {
            nub.Glowing = true;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            nub.Glowing = false;
            base.OnHoverLost(state);
        }

        protected override void OnChecked()
        {
            nub.State = CheckBoxState.Checked;

            if (bindable != null)
                bindable.Value = true;
        }

        protected override void OnUnchecked()
        {
            nub.State = CheckBoxState.Unchecked;

            if (bindable != null)
                bindable.Value = false;
        }

        public class Nub : CircularContainer, IStateful<CheckBoxState>
        {
            private Box fill;
            private Color4 glowingColour;
            private Color4 idleColour;

            public Nub()
            {
                Size = new Vector2(12, 12);

                Children = new[]
                {
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    AlwaysPresent = true,
                },
            };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = idleColour = Color4.White;
                glowingColour = colours.YellowLighter;

                EdgeEffect = new EdgeEffect
                {
                    Colour = colours.YellowDarker,
                    Type = EdgeEffectType.Glow,
                    Radius = 6,
                    Roundness = 2,
                };

                FadeEdgeEffectTo(0);
            }

            public bool Glowing
            {
                set
                {
                    if (value)
                    {
                        FadeColour(glowingColour, 500, EasingTypes.OutQuint);
                        FadeEdgeEffectTo(1, 500, EasingTypes.OutQuint);
                    }
                    else
                    {
                        FadeEdgeEffectTo(0, 500);
                        FadeColour(idleColour, 500);
                    }
                }
            }

            private CheckBoxState state;

            public CheckBoxState State
            {
                get
                {
                    return state;
                }
                set
                {
                    state = value;

                    switch (state)
                    {
                        case CheckBoxState.Checked:
                            fill.FadeIn(500, EasingTypes.OutQuint);
                            break;
                        case CheckBoxState.Unchecked:
                            fill.FadeTo(0.01f, 500, EasingTypes.OutQuint);
                            break;
                    }
                }
            }
        }
    }
}
