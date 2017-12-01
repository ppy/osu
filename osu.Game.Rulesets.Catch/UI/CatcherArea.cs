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
using OpenTK.Graphics;

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

        public bool AttemptCatch(CatchHitObject obj) => catcher.AttemptCatch(obj);

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

                    Trail |= dashing;
                }
            }

            private bool trail;

            /// <summary>
            /// Activate or deactive the trail. Will be automatically deactivated when conditions to keep the trail displayed are no longer met.
            /// </summary>
            protected bool Trail
            {
                get { return trail; }
                set
                {
                    if (value == trail) return;

                    trail = value;

                    if (Trail)
                        beginTrail();
                }
            }

            private void beginTrail()
            {
                Trail &= dashing || HyperDashing;
                Trail &= AdditiveTarget != null;

                if (!Trail) return;

                var additive = createCatcherSprite();

                additive.Anchor = Anchor;
                additive.OriginPosition = additive.OriginPosition + new Vector2(DrawWidth / 2, 0); // also temporary to align sprite correctly.
                additive.Position = Position;
                additive.Scale = Scale;
                additive.Colour = HyperDashing ? Color4.Red : Color4.White;
                additive.RelativePositionAxes = RelativePositionAxes;
                additive.Blending = BlendingMode.Additive;

                AdditiveTarget.Add(additive);

                additive.FadeTo(0.4f).FadeOut(800, Easing.OutQuint).Expire();

                Scheduler.AddDelayed(beginTrail, HyperDashing ? 25 : 50);
            }

            private Sprite createCatcherSprite() => new Sprite
            {
                Size = new Vector2(CATCHER_SIZE),
                FillMode = FillMode.Fill,
                Texture = texture,
                OriginPosition = new Vector2(-3, 10) // temporary until the sprite is aligned correctly.
            };

            /// <summary>
            /// Add a caught fruit to the catcher's stack.
            /// </summary>
            /// <param name="fruit">The fruit that was caught.</param>
            public void Add(DrawableHitObject fruit)
            {
                float distance = fruit.DrawSize.X / 2 * fruit.Scale.X;

                while (caughtFruit.Any(f => f.LifetimeEnd == double.MaxValue && Vector2Extensions.DistanceSquared(f.Position, fruit.Position) < distance * distance))
                {
                    fruit.X += RNG.Next(-5, 5);
                    fruit.Y -= RNG.Next(0, 5);
                }

                caughtFruit.Add(fruit);

                var catchObject = (CatchHitObject)fruit.HitObject;

                if (catchObject.LastInCombo)
                    explode();

                updateHyperDashState(catchObject, true);
            }

            /// <summary>
            /// Let the catcher attempt to catch a fruit.
            /// </summary>
            /// <param name="fruit">The fruit to catch.</param>
            /// <returns>Whether the catch is possible.</returns>
            public bool AttemptCatch(CatchHitObject fruit)
            {
                const double relative_catcher_width = CATCHER_SIZE / 2;

                // this stuff wil disappear once we move fruit to non-relative coordinate space in the future.
                var catchObjectPosition = fruit.X * CatchPlayfield.BASE_WIDTH;
                var catcherPosition = Position.X * CatchPlayfield.BASE_WIDTH;

                var validCatch =
                    catchObjectPosition >= catcherPosition - relative_catcher_width / 2 &&
                    catchObjectPosition <= catcherPosition + relative_catcher_width / 2;

                // if we are hypderdashing in teh next hit is not, let's change our state here (it's our only opportunity to handle missed fruit currently).
                updateHyperDashState(fruit, false);

                return validCatch;
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

                if (hyperDashModifier != 1)
                    dashModifier = hyperDashModifier;

                Scale = new Vector2(Math.Abs(Scale.X) * Math.Sign(currentDirection), Scale.Y);
                X = (float)MathHelper.Clamp(X + Math.Sign(currentDirection) * Clock.ElapsedFrameTime * BASE_SPEED * dashModifier, 0, 1);
            }

            /// <summary>
            /// Whether we are hypderdashing or not.
            /// </summary>
            protected bool HyperDashing => hyperDashModifier != 1;

            private double hyperDashModifier = 1;

            /// <summary>
            /// Update whether we are hyper or not.
            /// </summary>
            /// <param name="fruit">The fruit to use as a condition for deciding our new state.</param>
            /// <param name="allowBegin">Whether to allow entering hyperdash or not. If false, we will only exit if required, but never enter.</param>
            private void updateHyperDashState(CatchHitObject fruit, bool allowBegin)
            {
                const float transition_length = 180;

                if (!fruit.HyperDash)
                {
                    hyperDashModifier = 1;
                    this.FadeColour(Color4.White, transition_length, Easing.OutQuint);
                    this.FadeTo(1, transition_length, Easing.OutQuint);
                    return;
                }

                if (allowBegin)
                {
                    hyperDashModifier = Math.Abs(fruit.HyperDashTarget.X - fruit.X) / Math.Abs(fruit.HyperDashTarget.StartTime - fruit.StartTime) / BASE_SPEED;
                    this.FadeColour(Color4.AliceBlue, transition_length, Easing.OutQuint);
                    this.FadeTo(0.5f, transition_length, Easing.OutQuint);
                    Trail = true;
                }
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
