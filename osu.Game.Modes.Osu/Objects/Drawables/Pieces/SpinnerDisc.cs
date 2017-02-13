using System;
using osu.Framework.Input;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SpinnerDisc : Container
    {
        public override bool HandleInput => true;
        private readonly Spinner spinner;
        private Sprite disc;
        private Box trigger;

        const float size = 500;
        public SpinnerDisc(Spinner spinner)
        {
            this.spinner = spinner;

            Position = spinner.Position;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;
            Alpha = 1;
            Masking = true;

            Children = new Drawable[]
            {
                trigger = new Box
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = Color4.Black,
                    Alpha = 0.01f,
                    Width = 1080,
                    Height = 1080
                },
                disc = new Sprite
                {
                    Size = new Vector2(size,size),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }

        private InputState lastState;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            lastState = state;
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            lastState = state;
            return base.OnMouseUp(state, args);
        }

        protected override bool OnMouseMove(InputState state)
        {
            lastState = state;
            return base.OnMouseMove(state);
        }
        bool tracking;
        public bool Tracking
        {
            get { return tracking; }
            set
            {
                if (value == tracking) return;

                tracking = value;
            }
        }

        private bool canCurrentlySpin => Time.Current >= spinner.StartTime && Time.Current < spinner.EndTime;

        private float? lastAngle;
        private float? actualAngle;
        private float angleAdded;
        private float lastAngleAdded;
        private float totalAngleSpinned = 0;
        public float Progress = 0;
        private float spinsPerMinuteNeeded = 100 + (5 * 15); //TODO: read per-map OD and place it on the 5
        private float rotationsNeeded;
        

        protected override void Update()
        {
            
            base.Update();
            Progress = MathHelper.Clamp(((totalAngleSpinned / 360) / rotationsNeeded), 0, 1);
            if (Tracking)
            {
                rotationsNeeded = (float)(spinsPerMinuteNeeded * (spinner.EndTime - spinner.StartTime) / 60000f);
                lastAngleAdded = angleAdded;
                angleAdded += GetAngledDifference();
                SetAngleSpinned(lastAngleAdded, angleAdded);
                discScale();
                disc.RotateTo(Rotation + angleAdded, 500, EasingTypes.OutExpo);
            }
            else
                actualAngle = null;
            if(!canCurrentlySpin)
                disc.RotateTo(angleAdded);
                
            Tracking = canCurrentlySpin && lastState != null && Contains(lastState.Mouse.NativeState.Position) && lastState.Mouse.HasMainButtonPressed;
        }
        private float GetMouseAngledPosition()
        {
            float mouseXFromCenter = lastState.Mouse.LastPosition.X - spinner.Position.X;
            float mouseYFromCenter = lastState.Mouse.LastPosition.Y - spinner.Position.Y;
            return (float)MathHelper.RadiansToDegrees(Math.Atan2(mouseYFromCenter,mouseXFromCenter));
        }

        private void discScale()
        {
            disc.ScaleTo(1 + (MathHelper.Clamp(((totalAngleSpinned / 360) / rotationsNeeded), 0, 1)/10), 100);
        }

        private float GetAngledDifference()
        {
            lastAngle = actualAngle ?? GetMouseAngledPosition();
            Delay(1);
            actualAngle = GetMouseAngledPosition();
            if (lastAngle < -60 && actualAngle > 60)
            {
                lastAngle += 360;
            }
            else if (lastAngle > 60 && actualAngle < -60)
            {
                lastAngle -= 360;
            }
            return (float)(actualAngle - lastAngle);
        }

        private void SetAngleSpinned(float a, float b)
        {
            if (b > a)
                totalAngleSpinned += b - a;
            else
                totalAngleSpinned += a - b;
        }
        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            disc.Texture = textures.Get(@"Menu/logo@2x");
        }
    }
}
