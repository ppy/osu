// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Timing;
using osu.Game.Configuration;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Play
{
    public class KeyCounterCollection : FillFlowContainer<KeyCounter>
    {
        private const int duration = 100;

        public readonly Bindable<bool> Visible = new Bindable<bool>(true);
        private readonly Bindable<bool> configVisibility = new Bindable<bool>();

        public KeyCounterCollection()
        {
            Direction = FillDirection.Horizontal;
            AutoSizeAxes = Axes.Both;
        }

        public override void Add(KeyCounter key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            base.Add(key);
            key.IsCounting = IsCounting;
            key.FadeTime = FadeTime;
            key.KeyDownTextColor = KeyDownTextColor;
            key.KeyUpTextColor = KeyUpTextColor;
            // Use the same clock object as SongProgress for saving KeyCounter state
            if (AudioClock != null)
                key.Clock = AudioClock;
        }

        public void ResetCount()
        {
            foreach (var counter in Children)
                counter.ResetCount();
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
            get { return isCounting; }
            set
            {
                if (value == isCounting) return;

                isCounting = value;
                foreach (var child in Children)
                    child.IsCounting = value;
            }
        }

        private int fadeTime;
        public int FadeTime
        {
            get { return fadeTime; }
            set
            {
                if (value != fadeTime)
                {
                    fadeTime = value;
                    foreach (var child in Children)
                        child.FadeTime = value;
                }
            }
        }

        private Color4 keyDownTextColor = Color4.DarkGray;
        public Color4 KeyDownTextColor
        {
            get { return keyDownTextColor; }
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
            get { return keyUpTextColor; }
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

        public IFrameBasedClock AudioClock { get; set; }

        private Receptor receptor;

        public Receptor GetReceptor()
        {
            return receptor ?? (receptor = new Receptor(this));
        }

        public void SetReceptor(Receptor receptor)
        {
            if (this.receptor != null)
                throw new InvalidOperationException("Cannot set a new receptor when one is already active");

            this.receptor = receptor;
        }

        public class Receptor : Drawable
        {
            protected readonly KeyCounterCollection Target;

            public Receptor(KeyCounterCollection target)
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
