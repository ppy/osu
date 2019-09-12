// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class KeyCounterDisplay : FillFlowContainer<KeyCounter>
    {
        private const int duration = 100;
        private const double key_fade_time = 80;

        public readonly Bindable<bool> Visible = new Bindable<bool>(true);
        private readonly Bindable<bool> configVisibility = new Bindable<bool>();

        public KeyCounterDisplay()
        {
            Direction = FillDirection.Horizontal;
            AutoSizeAxes = Axes.Both;
        }

        public override void Add(KeyCounter key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            base.Add(key);
            key.IsCounting = IsCounting;
            key.FadeTime = key_fade_time;
            key.KeyDownTextColor = KeyDownTextColor;
            key.KeyUpTextColor = KeyUpTextColor;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.KeyOverlay, configVisibility);

            Visible.BindValueChanged(_ => updateVisibility());
            configVisibility.BindValueChanged(_ => updateVisibility(), true);
        }

        private bool isCounting = true;

        public bool IsCounting
        {
            get => isCounting;
            set
            {
                if (value == isCounting) return;

                isCounting = value;
                foreach (var child in Children)
                    child.IsCounting = value;
            }
        }

        private Color4 keyDownTextColor = Color4.DarkGray;

        public Color4 KeyDownTextColor
        {
            get => keyDownTextColor;
            set
            {
                if (value != keyDownTextColor)
                {
                    keyDownTextColor = value;
                    foreach (var child in Children)
                        child.KeyDownTextColor = value;
                }
            }
        }

        private Color4 keyUpTextColor = Color4.White;

        public Color4 KeyUpTextColor
        {
            get => keyUpTextColor;
            set
            {
                if (value != keyUpTextColor)
                {
                    keyUpTextColor = value;
                    foreach (var child in Children)
                        child.KeyUpTextColor = value;
                }
            }
        }

        private void updateVisibility() => this.FadeTo(Visible.Value || configVisibility.Value ? 1 : 0, duration);

        public override bool HandleNonPositionalInput => receptor == null;
        public override bool HandlePositionalInput => receptor == null;

        private Receptor receptor;

        public void SetReceptor(Receptor receptor)
        {
            if (this.receptor != null)
                throw new InvalidOperationException("Cannot set a new receptor when one is already active");

            this.receptor = receptor;
        }

        public class Receptor : Drawable
        {
            protected readonly KeyCounterDisplay Target;

            public Receptor(KeyCounterDisplay target)
            {
                RelativeSizeAxes = Axes.Both;
                Depth = float.MinValue;
                Target = target;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            protected override bool Handle(UIEvent e)
            {
                switch (e)
                {
                    case KeyDownEvent _:
                    case KeyUpEvent _:
                    case MouseDownEvent _:
                    case MouseUpEvent _:
                        return Target.Children.Any(c => c.TriggerEvent(e));
                }

                return base.Handle(e);
            }
        }
    }
}
