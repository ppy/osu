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
        }

        private Container content;

        protected override Container<Drawable> Content => content;

        [Initializer]
        private void Load()
        {
            AddInternal(content = new Container()
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        protected override bool OnMouseMove(InputState state)
        {
            content.Position = (state.Mouse.Position - DrawSize / 2) * ParallaxAmount;
            return base.OnMouseMove(state);
        }

        protected override void Update()
        {
            base.Update();
            content.Scale = new Vector2(1 + ParallaxAmount);
        }
    }
}
