// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        private Catcher catcher;

        public void Add(DrawableHitObject fruit, Vector2 screenPosition) => catcher.AddToStack(fruit, screenPosition);

        public bool CheckIfWeCanCatch(CatchBaseHit obj) => Math.Abs(catcher.Position.X - obj.Position) < catcher.DrawSize.X / DrawSize.X / 2;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                catcher = new Catcher
                {
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopCentre,
                    X = 0.5f,
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            catcher.Size = new Vector2(DrawSize.Y);
        }

        private class Catcher : Container, IKeyBindingHandler<CatchAction>
        {
            private Texture texture;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                texture = textures.Get(@"Play/Catch/fruit-catcher-idle");

                Child = createCatcherSprite();
            }

            private int currentDirection;

            private bool dashing;

            protected bool Dashing
            {
                get { return dashing; }
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

                var additive = createCatcherSprite();

                additive.RelativePositionAxes = Axes.Both;
                additive.Blending = BlendingMode.Additive;
                additive.Position = Position;
                additive.Scale = Scale;

                ((CatcherArea)Parent).Add(additive);

                additive.FadeTo(0.4f).FadeOut(800, Easing.OutQuint).Expire();

                Scheduler.AddDelayed(addAdditiveSprite, 50);
            }

            private Sprite createCatcherSprite() => new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                FillMode = FillMode.Fit,
                Texture = texture,
                OriginPosition = new Vector2(DrawWidth / 2, 10) //temporary until the sprite is aligned correctly.
            };

            public bool OnPressed(CatchAction action)
            {
                switch (action)
                {
                    case CatchAction.MoveLeft:
                        currentDirection--;
                        return true;
                    case CatchAction.MoveRight:
                        currentDirection++;
                        return true;
                    case CatchAction.Dash:
                        Dashing = true;
                        return true;
                }

                return false;
            }

            public bool OnReleased(CatchAction action)
            {
                switch (action)
                {
                    case CatchAction.MoveLeft:
                        currentDirection++;
                        return true;
                    case CatchAction.MoveRight:
                        currentDirection--;
                        return true;
                    case CatchAction.Dash:
                        Dashing = false;
                        return true;
                }

                return false;
            }

            protected override void Update()
            {
                base.Update();

                if (currentDirection == 0) return;

                float speed = Dashing ? 1.5f : 1;

                Scale = new Vector2(Math.Sign(currentDirection), 1);
                X = (float)MathHelper.Clamp(X + Math.Sign(currentDirection) * Clock.ElapsedFrameTime / 1800 * speed, 0, 1);
            }

            public void AddToStack(DrawableHitObject fruit, Vector2 absolutePosition)
            {
                fruit.RelativePositionAxes = Axes.None;
                fruit.Position = new Vector2(ToLocalSpace(absolutePosition).X - DrawSize.X / 2, 0);

                fruit.Anchor = Anchor.TopCentre;
                fruit.Origin = Anchor.BottomCentre;
                fruit.Scale *= 0.7f;
                fruit.LifetimeEnd = double.MaxValue;

                float distance = fruit.DrawSize.X / 2 * fruit.Scale.X;

                while (Children.OfType<DrawableFruit>().Any(f => Vector2Extensions.DistanceSquared(f.Position, fruit.Position) < distance * distance))
                {
                    fruit.X += RNG.Next(-5, 5);
                    fruit.Y -= RNG.Next(0, 5);
                }

                Add(fruit);
            }
        }
    }
}
