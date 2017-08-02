using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Rulesets.Catch.UI
{
    public class Catcher : Sprite
    {
        public override bool HandleInput => true;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get(@"Play/Catch/fruit-catcher-idle");
        }

        private bool leftPressed;
        private bool rightPressed;

        private int currentDirection;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return true;

            switch (args.Key)
            {
                case Key.Left:
                    currentDirection = -1;
                    leftPressed = true;
                    return true;
                case Key.Right:
                    currentDirection = 1;
                    rightPressed = true;
                    return true;
            }

            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Left:
                    currentDirection = rightPressed ? 1 : 0;
                    leftPressed = false;

                    return true;
                case Key.Right:
                    currentDirection = leftPressed ? -1 : 0;
                    rightPressed = false;
                    return true;
            }

            return base.OnKeyUp(state, args);
        }

        protected override void Update()
        {
            base.Update();

            if (currentDirection == 0) return;

            Scale = new Vector2(Scale.X * (Math.Sign(currentDirection) != Math.Sign(Scale.X) ? -1 : 1), Scale.Y);
            X = (float)MathHelper.Clamp(X + currentDirection * Clock.ElapsedFrameTime / 1000, 0, 1);
        }
    }
}
