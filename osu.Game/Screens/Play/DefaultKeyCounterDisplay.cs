// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public partial class DefaultKeyCounterDisplay : KeyCounterDisplay
    {
        private const int duration = 100;
        private const double key_fade_time = 80;

        private readonly FillFlowContainer<KeyCounter> keyFlow = new FillFlowContainer<KeyCounter>();

        protected override Container<KeyCounter> Content => keyFlow;

        public new IReadOnlyList<DefaultKeyCounter> Children
        {
            get => (IReadOnlyList<DefaultKeyCounter>)base.Children;
            set => base.Children = value;
        }

        public DefaultKeyCounterDisplay()
            : base(typeof(DefaultKeyCounter))
        {
            keyFlow.Direction = FillDirection.Horizontal;
            keyFlow.AutoSizeAxes = Axes.Both;
            keyFlow.Alpha = 0;

            InternalChild = keyFlow;
        }

        protected override void Update()
        {
            base.Update();

            // Don't use autosize as it will shrink to zero when KeyFlow is hidden.
            // In turn this can cause the display to be masked off screen and never become visible again.
            Size = keyFlow.Size;
        }

        public override void AddTrigger(InputTrigger trigger)
        {
            DefaultKeyCounter key = new DefaultKeyCounter(trigger);
            Add(key);
            key.FadeTime = key_fade_time;
            key.KeyDownTextColor = KeyDownTextColor;
            key.KeyUpTextColor = KeyUpTextColor;
        }

        public override void Add(KeyCounter key)
        {
            base.Add(key);

            DefaultKeyCounter defaultKey = (DefaultKeyCounter)key;

            defaultKey.FadeTime = key_fade_time;
            defaultKey.KeyDownTextColor = KeyDownTextColor;
            defaultKey.KeyUpTextColor = KeyUpTextColor;
        }

        protected override void UpdateVisibility() =>
            // Isolate changing visibility of the key counters from fading this component.
            keyFlow.FadeTo(AlwaysVisible.Value || ConfigVisibility.Value ? 1 : 0, duration);

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
    }
}
