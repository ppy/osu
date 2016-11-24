using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK;
using osu.Framework;
using osu.Framework.Allocation;

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

        protected override void Update()
        {
            base.Update();
            content.Position = (input.CurrentState.Mouse.Position - DrawSize / 2) * ParallaxAmount;
            content.Scale = new Vector2(1 + ParallaxAmount);
        }
    }
}
