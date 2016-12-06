//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Transformations;

namespace osu.Game.Graphics.Containers
{
    class ParallaxContainer : Container
    {
        public float ParallaxAmount = 0.02f;

        public override bool Contains(Vector2 screenSpacePos) => true;

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

        private Container content;
        private InputManager input;

        protected override Container<Drawable> Content => content;

        [BackgroundDependencyLoader]
        private void load(UserInputManager input)
        {
            this.input = input;
        }

        bool firstUpdate = true;

        protected override void Update()
        {
            base.Update();

            content.MoveTo((ToLocalSpace(input.CurrentState.Mouse.NativeState.Position) - DrawSize / 2) * ParallaxAmount, firstUpdate ? 0 : 1000, EasingTypes.OutQuint);
            content.Scale = new Vector2(1 + ParallaxAmount);

            firstUpdate = false;
        }
    }
}
