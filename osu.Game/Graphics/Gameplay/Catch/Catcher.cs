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

namespace osu.Game.Graphics.Gameplay.Catch
{
    class Catcher : Container
    {
        private Sprite catcherSprite;

        public Anchor catcherAnchor => facingRight ? Anchor.BottomLeft : Anchor.BottomRight;

        private float hyperMultiplier;

        private bool isMoving;
        public bool IsMoving
        {
            get { return isMoving; }
            protected set
            {
                if (isMoving != value)
                {
                    isMoving = value;
                }
            }
        }

        private bool facingRight;
        public bool FacingRight
        {
            get { return facingRight; }
            protected set
            {
                if (facingRight != value)
                {
                    facingRight = value;

                    FlipHorizontal = facingRight ? false : true;

                    //todo: why isn't this working any more?
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
            if (catcherSprite == null)
            {
                facingRight = true;
                IsMoving = false;

                catcherSprite = new Sprite
                {
                    Texture = Game.Textures.Get(@"Gameplay/Catch/fruit-catcher-idle"),
                    Scale = this.Scale,
                    Anchor = catcherAnchor
                };

                IsDashing = false;
                isHyperDashing = false;
                hyperMultiplier = 1.0f;

                Add(catcherSprite);
            }
        }

        protected override void Update()
        {
            float speedMultiplier = 1;
            if (IsDashing) speedMultiplier *= 2;
            if (IsHyperDashing) speedMultiplier *= hyperMultiplier;

            if (IsMoving)
            {
                MoveToX(Position.X + (FacingRight ? speedMultiplier : speedMultiplier * -1));
                if (Position.X < 0) MoveToX(0);
                if (Position.X > (768 - (Width * Scale.X))) MoveToX(768 - (Width * Scale.X));
            }

            
            base.Update();
        }

        public void UpdateMovement(bool direction, bool move)
        {
            IsMoving = move;
            FacingRight = direction;
        }

        public void ToggleHyperDash(bool value, float multiplier = 1.0f)
        {
            hyperMultiplier = !value ? 1.0f : multiplier;
            IsHyperDashing = value;
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
                Scale = this.Scale,
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
                StartValue = this.Scale,
                EndValue = new Vector2(this.Scale.X * 1.2f, this.Scale.Y * 1.2f)
            });
            hyperSprite.Expire(true);
            Add(hyperSprite);
        }
    }
}
