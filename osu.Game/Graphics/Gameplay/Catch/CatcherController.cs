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
    public class CatcherController : Container
    {
        private Catcher mainCatcher;

        private Vector2 catcherScale => new Vector2(0.7f, 0.7f);

        public int GamefieldWidth = 512;

        int lastDashFrame;

        public CatcherController()
        {
            Additive = true;
        }

        public override void Load()
        {
            base.Load();

            mainCatcher = new Catcher
            {
                Scale = catcherScale
            };
            
            Height = 320 * catcherScale.Y; // static size to prevent skinning changing the gameplay
            Width = GamefieldWidth;

            lastDashFrame = 0;

            Add(mainCatcher);

            ResetPosition();
        }

        protected override void Update()
        {
            if (mainCatcher.IsDashing && lastDashFrame < Clock.CurrentTime - 16)
            {
                Drawable glowSprite = mainCatcher.Clone();
                glowSprite.Colour = mainCatcher.IsHyperDashing ? Color4.Red : Color4.White;
                
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
                    mainCatcher.UpdateMovement(false, true);
                    break;
                case Key.Right:
                    mainCatcher.UpdateMovement(true, true);
                    break;
                case Key.ShiftLeft:
                    mainCatcher.IsDashing = true;
                    break;
                case Key.ControlLeft: // hyper testing
                    mainCatcher.ToggleHyperDash(true, 5.0f);
                    break;
            }

            return true;
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Left:
                    if (!state.Keyboard.Keys.Contains(Key.Right))
                        mainCatcher.UpdateMovement(mainCatcher.FacingRight, false);
                    else
                        mainCatcher.UpdateMovement(true, mainCatcher.IsMoving);
                    break;
                case Key.Right:
                    if (!state.Keyboard.Keys.Contains(Key.Left))
                        mainCatcher.UpdateMovement(mainCatcher.FacingRight, false);
                    else
                        mainCatcher.UpdateMovement(false, mainCatcher.IsMoving);
                    break;
                case Key.ShiftLeft:
                    mainCatcher.IsDashing = false;
                    break;
                case Key.ControlLeft: // hyper testing
                    mainCatcher.ToggleHyperDash(false);
                    break;
            }

            return true;
        }

        public void ResetPosition()
        {
            mainCatcher.Position = new Vector2(240, Position.Y);
        }
    }
}