using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK;

namespace osu.Game.Graphics.Containers
{
    class ParallaxContainer : LargeContainer
    {
        public float ParallaxAmount = 0.02f;

        public override bool Contains(Vector2 screenSpacePos) => true;

        private Container content = new LargeContainer()
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };

        protected override Container AddTarget => content;

        public override void Load()
        {
            base.Load();
            Add(content);
        }

        protected override bool OnMouseMove(InputState state)
        {
            content.Position = (state.Mouse.Position - ActualSize / 2) * ParallaxAmount;
            return base.OnMouseMove(state);
        }

        protected override void Update()
        {
            base.Update();
            content.Scale = new Vector2(1 + ParallaxAmount);
        }
    }
}