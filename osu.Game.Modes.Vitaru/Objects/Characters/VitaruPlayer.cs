using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Input;
using osu.Game.Modes.Vitaru.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using System.Collections.Generic;
using System;
using osu.Game.Modes.Vitaru.Objects.Projectiles;
using OpenTK.Graphics;

namespace osu.Game.Modes.Vitaru.Objects.Characters
{
    public class VitaruPlayer : Character
    {
        //stores if a key is pressed or not
        private Dictionary<Key, bool> keys = new Dictionary<Key, bool>();

        //stores the player position
        public Vector2 playerPosition = new Vector2(0, 200);
        public Vector4 PlayerBounds = new Vector4(-200, 200, -200, 300);  //MinX,MaxX,MinY,MaxY
        public Vector2 playerSpeed { get; set; } = new Vector2(0.5f, 0.5f);
        //useful when mods get involved or slow debuffs become a thing, pixels per millisecond, different values for x and y


        private bool _kiaiActivated = false;
        public bool KiaiActivated
        {
            get
            {
                return _kiaiActivated;
            }
            set
            {
                _kiaiActivated = value;
                player.setKiai(value);
            }
        }
        private DrawablePlayer player;

        public VitaruPlayer(Container parent) : base(parent)
        {

            //initialize the Dictionary values so it wont throw exceptions
            keys[Key.Up] = false;
            keys[Key.Right] = false;
            keys[Key.Down] = false;
            keys[Key.Left] = false;
            keys[Key.Z] = false;
            keys[Key.X] = false;
            keys[Key.LShift] = false;
            keys[Key.RShift] = false;
            Children = new[]
            {
                player = new DrawablePlayer()
                {
                    Origin = Anchor.Centre,
                },

            };
            Health = 100;
            Add(hitbox = new Hitbox()
            {
                hitboxWidth = 4,
                hitboxColor = Color4.Cyan,
            });
            Team = 0;
            OnShoot = Shoot;
        }


        public void ToggleKiai()
        {
            KiaiActivated = !KiaiActivated;
        }

        //multi-key-input should work with this
        protected override void Update()
        {
            base.Update();
            float ySpeed = playerSpeed.Y * (float)(Clock.ElapsedFrameTime);
            float xSpeed = playerSpeed.X * (float)(Clock.ElapsedFrameTime);
            if (keys[Key.LShift] | keys[Key.RShift])
            {
                xSpeed /= 2;
                ySpeed /= 2;
                //Add hitbox showing here
            }
            if (keys[Key.Z])
            {
                Shoot();
            }
            if (keys[Key.Up])
            {
                playerPosition.Y -= ySpeed;
            }
            if (keys[Key.Left])
            {
                playerPosition.X -= xSpeed;
            }
            if (keys[Key.Down])
            {
                playerPosition.Y += ySpeed;
            }
            if (keys[Key.Right])
            {
                playerPosition.X += xSpeed;
            }
            playerPosition = Vector2.ComponentMin(playerPosition, PlayerBounds.Yw);
            playerPosition = Vector2.ComponentMax(playerPosition, PlayerBounds.Xz);
            Position = playerPosition;
        }

        private void Shoot()
        {
            Bullet b;
            Bullet l;
            Bullet r;
            parent.Add(b = new Bullet(Team)
            {
                Depth = 1,
                Anchor = Anchor.Centre,
                bulletAngle = 0f,
                bulletSpeed = 1.5f,
            });
            parent.Add(l = new Bullet(Team)
            {
                Depth = 2,
                Anchor = Anchor.Centre,
                bulletAngle = 355f,
                bulletSpeed = 1.5f,
            });
            parent.Add(r = new Bullet(Team)
            {
                Depth = 2,
                Anchor = Anchor.Centre,
                bulletAngle = 5f,
                bulletSpeed = 1.5f,
            });
            b.MoveTo(ToSpaceOfOtherDrawable(new Vector2(0, 0), b));
            r.MoveTo(ToSpaceOfOtherDrawable(new Vector2(0, 0), r));
            l.MoveTo(ToSpaceOfOtherDrawable(new Vector2(0, 0), l));
        }

        //saves if key is pressed
        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            keys[args.Key] = true;
            if (args.Key == Key.LShift || args.Key == Key.RShift)
                hitbox.Alpha = 1;
            return base.OnKeyDown(state, args);
        }
        //saves if key is released
        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            keys[args.Key] = false;
            if (args.Key == Key.LShift || args.Key == Key.RShift)
                hitbox.Alpha = 0;
            return base.OnKeyUp(state, args);
        }

       
    }
}