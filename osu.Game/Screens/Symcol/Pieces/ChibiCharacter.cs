using OpenTK;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using System.Diagnostics;
using osu.Game.Graphics.Containers;
using osu.Framework.Graphics;

namespace osu.Game.Screens.Symcol.Pieces
{
    public abstract class ChibiCharacter : BeatSyncedContainer
    {
        protected Sprite CharacterIdleSprite;
        protected Sprite CharacterGrabbedSprite;

        public Vector2 Velocity = Vector2.Zero;

        public ChibiCharacter(string characterName)
        {
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                CharacterIdleSprite = new Sprite
                {
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    //Texture = OsuGame.SymcolTextures.Get(characterName + "Idle")
                },
                CharacterGrabbedSprite = new Sprite
                {
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    //Texture = OsuGame.SymcolTextures.Get(characterName + "Grabbed")
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            //Just a IsGrabbed bool basically
            if (CharacterGrabbedSprite.Alpha < 1)
                movement();

            float r = RNG.Next(0, 1001);
            if (r == 500)
                jump(RNG.NextBool());
        }

        private void movement()
        {
            //Gravity
            Velocity.Y += (float)Clock.ElapsedFrameTime / 1000 * 9.8f;
            //Air Friction
            Velocity *= new Vector2((float)Clock.ElapsedFrameTime / 1000 * 0.1f);

            //Lets make sure we don't fall through the floor
            if (Position.Y <= 0 && Velocity.Y >= 9.8f)
                Velocity.Y *= 0.5f;
            else if (Position.Y <= 0 && Velocity.Y > 0)
                Velocity.Y = 0;

            //"Move that gear up" -Engineer
            this.MoveToOffset(Velocity);
        }

        private void jump(bool left)
        {
            if (left)
            {

            }
            else
            {

            }
        }

        #region Drag Crap
        protected override bool OnDragStart(InputState state) => true;

        private Vector2 startPosition;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            startPosition = Position;

            CharacterIdleSprite.Alpha = 0;
            CharacterGrabbedSprite.Alpha = 1;

            return base.OnMouseDown(state, args);
        }

        protected override bool OnDrag(InputState state)
        {
            Trace.Assert(state.Mouse.PositionMouseDown != null, "state.Mouse.PositionMouseDown != null");

            Position = startPosition + state.Mouse.Position - state.Mouse.PositionMouseDown.Value;

            return base.OnDrag(state);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            CharacterIdleSprite.Alpha = 1;
            CharacterGrabbedSprite.Alpha = 0;

            return base.OnMouseUp(state, args);
        }
        #endregion
    }
}
