using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        public override bool HandleInput => true;

        private Sprite catcher;

        private Drawable createAdditiveFrame() => new Sprite
        {
            RelativePositionAxes = Axes.Both,
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,

            Texture = catcher.Texture,
            BlendingMode = BlendingMode.Additive,
            Position = catcher.Position,
            Scale = catcher.Scale,
        };

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Children = new Drawable[]
            {
                catcher = new Sprite
                {
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fit,

                    X = 0.5f,
                    Texture = textures.Get(@"Play/Catch/fruit-catcher-idle"),
                },
            };
        }

        private bool leftPressed;
        private bool rightPressed;

        private int currentDirection;

        private bool dashing;

        protected bool Dashing
        {
            get
            {
                return dashing;
            }
            set
            {
                if (value == dashing) return;

                dashing = value;

                if (dashing)
                    Schedule(addAdditiveSprite);
            }
        }

        private void addAdditiveSprite()
        {
            if (!dashing) return;

            var additive = createAdditiveFrame();

            Add(additive);

            additive.FadeTo(0.4f).FadeOut(800, Easing.OutQuint).Expire();

            Scheduler.AddDelayed(addAdditiveSprite, 50);
        }

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
                case Key.ShiftLeft:
                    Dashing = true;
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
                case Key.ShiftLeft:
                    Dashing = false;
                    return true;
            }

            return base.OnKeyUp(state, args);
        }

        protected override void Update()
        {
            base.Update();

            if (currentDirection == 0) return;

            float speed = Dashing ? 1.5f : 1;

            catcher.Scale = new Vector2(Math.Sign(currentDirection), 1);
            catcher.X = (float)MathHelper.Clamp(catcher.X + currentDirection * Clock.ElapsedFrameTime / 1800 * speed, 0, 1);
        }
    }
}
