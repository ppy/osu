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
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        private Catcher catcher;
        private Container explodingFruitContainer;

        public void Add(DrawableHitObject fruit, Vector2 screenPosition) => catcher.AddToStack(fruit, screenPosition);

        public bool CheckIfWeCanCatch(CatchBaseHit obj) => Math.Abs(catcher.Position.X - obj.X) < catcher.DrawSize.X / DrawSize.X / 2;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                explodingFruitContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                catcher = new Catcher
                {
                    RelativePositionAxes = Axes.Both,
                    ExplodingFruitTarget = explodingFruitContainer,
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

            private Container<DrawableHitObject> caughtFruit;

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                texture = textures.Get(@"Play/Catch/fruit-catcher-idle");

                Children = new Drawable[]
                {
                    createCatcherSprite(),
                    caughtFruit = new Container<DrawableHitObject>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.BottomCentre,
                    }
                };
            }

            private int currentDirection;

            private bool dashing;

            public Container ExplodingFruitTarget;

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

            /// <summary>
            /// The relative space to cover in 1 millisecond. based on 1 game pixel per millisecond as in osu-stable.
            /// </summary>
            private const double base_speed = 1.0 / 512;

            protected override void Update()
            {
                base.Update();

                if (currentDirection == 0) return;

                double dashModifier = Dashing ? 1 : 0.5;

                Scale = new Vector2(Math.Sign(currentDirection), 1);
                X = (float)MathHelper.Clamp(X + Math.Sign(currentDirection) * Clock.ElapsedFrameTime * base_speed * dashModifier, 0, 1);
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

                while (caughtFruit.Any(f => f.LifetimeEnd == double.MaxValue && Vector2Extensions.DistanceSquared(f.Position, fruit.Position) < distance * distance))
                {
                    fruit.X += RNG.Next(-5, 5);
                    fruit.Y -= RNG.Next(0, 5);
                }

                caughtFruit.Add(fruit);

                if (((CatchBaseHit)fruit.HitObject).LastInCombo)
                    explode();
            }

            private void explode()
            {
                var fruit = caughtFruit.ToArray();

                foreach (var f in fruit)
                {
                    var originalX = f.X * Scale.X;

                    if (ExplodingFruitTarget != null)
                    {
                        f.Anchor = Anchor.TopLeft;
                        f.Position = caughtFruit.ToSpaceOfOtherDrawable(f.DrawPosition, ExplodingFruitTarget);

                        caughtFruit.Remove(f);

                        ExplodingFruitTarget.Add(f);
                    }

                    f.MoveToY(f.Y - 50, 250, Easing.OutSine)
                    .Then()
                    .MoveToY(f.Y + 50, 500, Easing.InSine);

                    f.MoveToX(f.X + originalX * 6, 1000);
                    f.FadeOut(750);

                    f.Expire();
                }
            }
        }
    }
}
