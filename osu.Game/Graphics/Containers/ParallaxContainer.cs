// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Graphics.Containers
{
    public partial class ParallaxContainer : Container, IRequireHighFrequencyMousePosition
    {
        public const float DEFAULT_PARALLAX_AMOUNT = 0.02f;

        /// <summary>
        /// The amount of parallax movement. Negative values will reverse the direction of parallax relative to user input.
        /// </summary>
        public float ParallaxAmount { get; set; } = DEFAULT_PARALLAX_AMOUNT;

        private Bindable<float> parallaxScale;

        private const float parallax_duration = 100;

        public ParallaxContainer()
        {
            RelativeSizeAxes = Axes.Both;
            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        private readonly Container content;
        private InputManager input;

        protected override Container<Drawable> Content => content;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            parallaxScale = config.GetBindable<float>(OsuSetting.MenuParallaxScale);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            input = GetContainingInputManager();
        }

        protected override void Update()
        {
            base.Update();

            float amount = ParallaxAmount * parallaxScale.Value;

            if (amount == 0 && content.Position == Vector2.Zero && content.Scale == Vector2.One)
                return;

            Vector2 offset = Vector2.Zero;
            Vector2 scale = new Vector2(1 + Math.Abs(amount));

            if (input.CurrentState.Mouse != null)
            {
                Vector2 sizeDiv2 = DrawSize / 2;
                Vector2 relativeAmount = ToLocalSpace(input.CurrentState.Mouse.Position) - sizeDiv2;

                const float base_factor = 0.999f;

                relativeAmount.X = (float)(Math.Sign(relativeAmount.X) * Interpolation.Damp(0, 1, base_factor, Math.Abs(relativeAmount.X)));
                relativeAmount.Y = (float)(Math.Sign(relativeAmount.Y) * Interpolation.Damp(0, 1, base_factor, Math.Abs(relativeAmount.Y)));

                offset = relativeAmount * sizeDiv2 * amount;
            }

            double elapsed = Math.Clamp(Clock.ElapsedFrameTime, 0, parallax_duration);

            content.Position = Precision.AlmostEquals(content.Position, offset)
                ? offset
                : Interpolation.ValueAt(elapsed, content.Position, offset, 0, parallax_duration, Easing.OutQuint);

            content.Scale = Precision.AlmostEquals(content.Scale, scale)
                ? scale
                : Interpolation.ValueAt(elapsed, content.Scale, scale, 0, 1000, Easing.OutQuint);
        }
    }
}
