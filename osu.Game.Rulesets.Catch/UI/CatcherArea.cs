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
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatcherArea : Container
    {
        public const float CATCHER_SIZE = 172;

        private readonly Catcher catcher;

        public Container ExplodingFruitTarget
        {
            set { catcher.ExplodingFruitTarget = value; }
        }

        public CatcherArea(BeatmapDifficulty difficulty = null)
        {
            RelativeSizeAxes = Axes.X;
            Height = CATCHER_SIZE;
            Child = catcher = new Catcher(difficulty)
            {
                AdditiveTarget = this,
            };
        }

        public void Add(DrawableHitObject fruit, Vector2 absolutePosition)
        {
            fruit.RelativePositionAxes = Axes.None;
            fruit.Position = new Vector2(catcher.ToLocalSpace(absolutePosition).X - catcher.DrawSize.X / 2, 0);

            fruit.Anchor = Anchor.TopCentre;
            fruit.Origin = Anchor.BottomCentre;
            fruit.Scale *= 0.7f;
            fruit.LifetimeEnd = double.MaxValue;

            catcher.Add(fruit);
        }

        public bool CanCatch(CatchHitObject obj) => Math.Abs(catcher.Position.X - obj.X) < catcher.DrawSize.X * Math.Abs(catcher.Scale.X) / DrawSize.X / 2;

        public class Catcher : Container, IKeyBindingHandler<CatchAction>
        {
            private Texture texture;

            private Container<DrawableHitObject> caughtFruit;

            public Container ExplodingFruitTarget;

            public Container AdditiveTarget;

            public Catcher(BeatmapDifficulty difficulty = null)
            {
                RelativePositionAxes = Axes.X;
                X = 0.5f;

                Origin = Anchor.TopCentre;
                Anchor = Anchor.TopLeft;

                Size = new Vector2(CATCHER_SIZE);
                if (difficulty != null)
                    Scale = new Vector2(1.0f - 0.7f * (difficulty.CircleSize - 5) / 5);
            }

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
                if (!dashing || AdditiveTarget == null) return;

                var additive = createCatcherSprite();

                additive.Anchor = Anchor;
                additive.OriginPosition = additive.OriginPosition + new Vector2(DrawWidth / 2, 0); // also temporary to align sprite correctly.
                additive.Position = Position;
                additive.Scale = Scale;
                additive.RelativePositionAxes = RelativePositionAxes;
                additive.Blending = BlendingMode.Additive;

                AdditiveTarget.Add(additive);

                additive.FadeTo(0.4f).FadeOut(800, Easing.OutQuint).Expire();

                Scheduler.AddDelayed(addAdditiveSprite, 50);
            }

            private Sprite createCatcherSprite() => new Sprite
            {
                Size = new Vector2(CATCHER_SIZE),
                FillMode = FillMode.Fill,
                Texture = texture,
                OriginPosition = new Vector2(-3, 10) // temporary until the sprite is aligned correctly.
            };

            public void Add(DrawableHitObject fruit)
            {
                float distance = fruit.DrawSize.X / 2 * fruit.Scale.X;

                while (caughtFruit.Any(f => f.LifetimeEnd == double.MaxValue && Vector2Extensions.DistanceSquared(f.Position, fruit.Position) < distance * distance))
                {
                    fruit.X += RNG.Next(-5, 5);
                    fruit.Y -= RNG.Next(0, 5);
                }

                caughtFruit.Add(fruit);

                if (((CatchHitObject)fruit.HitObject).LastInCombo)
                    explode();
            }

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
            public const double BASE_SPEED = 1.0 / 512;

            protected override void Update()
            {
                base.Update();

                if (currentDirection == 0) return;

                double dashModifier = Dashing ? 1 : 0.5;

                Scale = new Vector2(Math.Abs(Scale.X) * Math.Sign(currentDirection), Scale.Y);
                X = (float)MathHelper.Clamp(X + Math.Sign(currentDirection) * Clock.ElapsedFrameTime * BASE_SPEED * dashModifier, 0, 1);
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
