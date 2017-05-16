// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Framework.Configuration;

namespace osu.Game.Graphics.Containers
{
    internal class ParallaxContainer : Container, IRequireHighFrequencyMousePosition
    {
        public float ParallaxAmount = 0.02f;

        private Bindable<bool> parallaxEnabled;

        public ParallaxContainer()
        {
            AlwaysReceiveInput = true;

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
        private void load(UserInputManager input, OsuConfigManager config)
        {
            this.input = input;
            parallaxEnabled = config.GetBindable<bool>(OsuSetting.MenuParallax);
            parallaxEnabled.ValueChanged += delegate
            {
                if (!parallaxEnabled)
                {
                    content.MoveTo(Vector2.Zero, firstUpdate ? 0 : 1000, EasingTypes.OutQuint);
                    content.Scale = new Vector2(1 + ParallaxAmount);
                }
            };
        }

        private bool firstUpdate = true;

        protected override void Update()
        {
            base.Update();

            if (parallaxEnabled)
            {
                Vector2 offset = input.CurrentState.Mouse == null ? Vector2.Zero : ToLocalSpace(input.CurrentState.Mouse.NativeState.Position) - DrawSize / 2;
                content.MoveTo(offset * ParallaxAmount, firstUpdate ? 0 : 1000, EasingTypes.OutQuint);
                content.Scale = new Vector2(1 + ParallaxAmount);
            }

            firstUpdate = false;
        }
    }
}
