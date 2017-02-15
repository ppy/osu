// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuCheckbox : CheckBox
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

        private Nub nub;
        private SpriteText labelSpriteText;
        private AudioSample sampleChecked;
        private AudioSample sampleUnchecked;

        public OsuCheckbox()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                labelSpriteText = new OsuSpriteText(),
                nub = new Nub
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
            nub.Glowing = true;
            nub.Expanded = true;
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            nub.Glowing = false;
            nub.Expanded = false;
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
            sampleChecked?.Play();
            nub.State = CheckBoxState.Checked;

            if (bindable != null)
                bindable.Value = true;
        }

        protected override void OnUnchecked()
        {
            sampleUnchecked?.Play();
            nub.State = CheckBoxState.Unchecked;

            if (bindable != null)
                bindable.Value = false;
        }
    }
}
