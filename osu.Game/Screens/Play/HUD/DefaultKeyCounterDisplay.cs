// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultKeyCounterDisplay : KeyCounterDisplay
    {
        private const double key_fade_time = 80;

        protected override FillFlowContainer<KeyCounter> KeyFlow { get; }

        public DefaultKeyCounterDisplay()
        {
            InternalChild = KeyFlow = new FillFlowContainer<KeyCounter>
            {
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Alpha = 0,
            };
        }

        protected override KeyCounter CreateCounter(InputTrigger trigger) => new DefaultKeyCounter(trigger)
        {
            FadeTime = key_fade_time,
            KeyDownTextColor = KeyDownTextColor,
            KeyUpTextColor = KeyUpTextColor,
        };

        private Color4 keyDownTextColor = Color4.DarkGray;

        public Color4 KeyDownTextColor
        {
            get => keyDownTextColor;
            set
            {
                if (value != keyDownTextColor)
                {
                    keyDownTextColor = value;
                    foreach (var child in KeyFlow.Cast<DefaultKeyCounter>())
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
                    foreach (var child in KeyFlow.Cast<DefaultKeyCounter>())
                        child.KeyUpTextColor = value;
                }
            }
        }
    }
}
