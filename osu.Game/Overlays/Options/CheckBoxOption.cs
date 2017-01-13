//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class CheckBoxOption : CheckBox
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
            }
        }

        public Color4 CheckedColor { get; set; } = Color4.Cyan;
        public Color4 UncheckedColor { get; set; } = Color4.White;
        public int FadeDuration { get; set; }

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

        private Light light;
        private SpriteText labelSpriteText;
        private AudioSample sampleChecked;
        private AudioSample sampleUnchecked;

        public CheckBoxOption()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                labelSpriteText = new SpriteText(),
                light = new Light()
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 5 },
                }
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
            light.Glowing = true;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            light.Glowing = false;
            base.OnHoverLost(state);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Sample.Get(@"Checkbox/check-on");
            sampleUnchecked = audio.Sample.Get(@"Checkbox/check-off");
        }

        protected override void OnChecked()
        {
            if (bindable != null)
                bindable.Value = true;

            sampleChecked?.Play();
            light.State = CheckBoxState.Checked;
        }

        protected override void OnUnchecked()
        {
            if (bindable != null)
                bindable.Value = false;

            sampleUnchecked?.Play();
            light.State = CheckBoxState.Unchecked;
        }

        private class Light : Container, IStateful<CheckBoxState>
        {
            private Box fill;

            const float border_width = 3;
            private Color4 glowingColour, idleColour;

            public Light()
            {
                Size = new Vector2(40, 12);

                Masking = true;                

                CornerRadius = Height / 2;
                Masking = true;
                BorderColour = Color4.White;
                BorderThickness = border_width;

                Children = new[]
                {
                    fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.01f, //todo: remove once we figure why containers aren't drawing at all times
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Colour = idleColour = colours.Pink;

                EdgeEffect = new EdgeEffect
                {
                    Colour = glowingColour = colours.PinkDarker,
                    Type = EdgeEffectType.Glow,
                    Radius = 10,
                    Roundness = 8,
                };
            }

            public bool Glowing
            {
                set
                {
                    if (value)
                    {
                        FadeColour(glowingColour, 500, EasingTypes.OutQuint);
                        FadeGlowTo(1, 500, EasingTypes.OutQuint);
                    }
                    else
                    {
                        FadeGlowTo(0, 500);
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
                            fill.FadeIn(200, EasingTypes.OutQuint);
                            break;
                        case CheckBoxState.Unchecked:
                            fill.FadeTo(0.01f, 200, EasingTypes.OutQuint); //todo: remove once we figure why containers aren't drawing at all times
                            break;
                    }
                }
            }
        }

    }
}
