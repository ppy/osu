//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Framework.Timing;

namespace osu.Game.Graphics.Gameplay.Catch
{
    public class Catcher : Container
    {
        private Sprite catcherSprite;

        int lastDashFrame;

        private bool moving;

        private FramedOffsetClock localClock;

        public float HyperMultiplier = 5.0f; // these will probably have to be set by the ruleset later

        private Vector2 catcherScale => new Vector2(0.7f, 0.7f);
        private Anchor catcherAnchor => facingRight ? Anchor.BottomLeft : Anchor.BottomRight;

        private bool facingRight;
        public bool FacingRight
        {
            get { return facingRight; }
            protected set
            {
                if (facingRight != value)
                {
                    facingRight = value;

                    catcherSprite.FlipHorizontal = facingRight ? false : true;

                    catcherSprite.Anchor = catcherAnchor;
                }
            }
        }

        public bool IsDashing { get; set; }

        private bool isHyperDashing;
        public bool IsHyperDashing
        {
            get { return isHyperDashing; }
            protected set
            {
                if (isHyperDashing != value)
                {
                    isHyperDashing = value;
                    updateHyperEffects(value);
                }
            }
        }

        public Catcher()
        {
            Additive = true;
        }

        public override void Load()
        {
            base.Load();

            facingRight = true;
            moving = false;

            if (localClock == null) localClock = Clock as FramedOffsetClock;
            catcherSprite = new Sprite
            {
                Texture = Game.Textures.Get(@"Gameplay/Catch/fruit-catcher-idle"),
                Scale = catcherScale,
                Anchor = catcherAnchor
            };
            
            Height = 320 * catcherScale.Y; // static size to prevent skinning changing the gameplay
            Width = 307 * catcherScale.X; 

            lastDashFrame = 0;
            IsDashing = false;

            Add(catcherSprite);

            ResetPosition();
        }

        protected override void Update()
        {
            float speedMultiplier = 1;
            if (IsDashing) speedMultiplier *= 2;
            if (IsHyperDashing) speedMultiplier *= HyperMultiplier;

            if (moving)
            {
                MoveToX(Position.X + (FacingRight ? speedMultiplier : speedMultiplier * -1));
                if (Position.X < 0) MoveToX(0);
                if (Position.X > (768 - (Width * catcherScale.X))) MoveToX(768 - (Width * catcherScale.X));
            }

            if (IsDashing && lastDashFrame < localClock.CurrentTime - 16)
            {
                Sprite glowSprite = new Sprite
                {
                    Texture = catcherSprite.Texture,
                    Scale = catcherScale,
                    Additive = true,
                    FlipHorizontal = catcherSprite.FlipHorizontal,
                    Anchor = catcherAnchor
                };
                glowSprite.Transforms.Add(new TransformAlpha(Clock)
                {
                    StartTime = Clock.CurrentTime,
                    EndTime = Clock.CurrentTime + 100,
                    StartValue = 1,
                    EndValue = 0
                });
                glowSprite.Expire(true);
                Add(glowSprite);

                lastDashFrame = (int)Clock.CurrentTime;
            }

            base.Update();
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Left:
                    FacingRight = false;
                    moving = true;
                    break;
                case Key.Right:
                    FacingRight = true;
                    moving = true;
                    break;
                case Key.ShiftLeft:
                    IsDashing = true;
                    break;
                case Key.ControlLeft: // hyper testing
                    IsHyperDashing = true;
                    break;
            }

            return true;
            //return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Left:
                    if (!state.Keyboard.Keys.Contains(Key.Right))
                        moving = false;
                    else
                        FacingRight = true;
                    break;
                case Key.Right:
                    if (!state.Keyboard.Keys.Contains(Key.Left))
                        moving = false;
                    else
                        FacingRight = false;
                    break;
                case Key.ShiftLeft:
                    IsDashing = false;
                    break;
                case Key.ControlLeft: // hyper testing
                    IsHyperDashing = false;
                    break;
            }

            return true;
            //return base.OnKeyUp(state, args);
        }

        private void updateHyperEffects(bool begin)
        {
            if (begin)
                catcherSprite.FadeColour(Color4.Red, 100);
            else
                catcherSprite.FadeColour(Color4.White, 100);

            Sprite hyperSprite = new Sprite
            {
                Texture = catcherSprite.Texture,
                Scale = catcherScale,
                Additive = true,
                FlipHorizontal = catcherSprite.FlipHorizontal,
                Colour = Color4.Red,
                Anchor = catcherAnchor
            };
            hyperSprite.Transforms.Add(new TransformAlpha(Clock)
            {
                StartTime = Clock.CurrentTime,
                EndTime = Clock.CurrentTime + 1200,
                StartValue = 1,
                EndValue = 0
            });
            hyperSprite.Transforms.Add(new TransformScaleVector(Clock)
            {
                StartTime = Clock.CurrentTime,
                EndTime = Clock.CurrentTime + 1200,
                StartValue = catcherScale,
                EndValue = new Vector2(catcherScale.X * 1.2f, catcherScale.Y * 1.2f)
            });
            hyperSprite.Expire(true);
            Add(hyperSprite);
        }

        public void ResetPosition()
        {
            Position = new Vector2(240, Position.Y);
        }
    }
}