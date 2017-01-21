using System;
using osu.Framework.Input;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Configuration;
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
        private readonly Spinner s;
        private Box trigger;

        public SpinnerDisc(Spinner spinner)
        {
            s = spinner;

            //Position = spinner.Position;
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

        private bool canCurrentlySpin => Time.Current >= s.StartTime && Time.Current < s.EndTime;

        private float? lastAngle;
        public float? ActualAngle;
        private float angleAdded;
        private float lastAngleAdded;
        private float totalAngleSpinned = 0;
        public float SpinProgress = 0;
        private float spinsPerMinuteNeeded = 100 + (5 * 15); //TODO: read per-map OD and place it on the 5
        private float rotationsNeeded => (float)(spinsPerMinuteNeeded * (s.EndTime - s.StartTime) / 60000f);
        public float DistanceToCentre;
        public bool IsSpinningLeft;
        private double spinDirectionDiscriminator;

        protected override void Update()
        {
            
            base.Update();  
            SpinProgress = MathHelper.Clamp(((totalAngleSpinned / 360) / rotationsNeeded), 0, 1);
            if (Tracking)
            {
                lastAngleAdded = angleAdded;
                angleAdded += GetAngledDifference();
                if (angleAdded > lastAngleAdded)
                    totalAngleSpinned += angleAdded - lastAngleAdded;
                else
                    totalAngleSpinned += lastAngleAdded - angleAdded;
                DistanceToCentre = GetDistanceToCentre();
                if (spinDirectionDiscriminator > 20)
                    IsSpinningLeft = false;
                else if (spinDirectionDiscriminator < -20)
                    IsSpinningLeft = true;
            }
            Tracking = canCurrentlySpin && lastState != null && Contains(lastState.Mouse.NativeState.Position) && lastState.Mouse.HasMainButtonPressed;
        }

        private float GetMouseAngledPosition()
        {
            float mouseXFromCenter = lastState.Mouse.LastPosition.X - Position.X;
            float mouseYFromCenter = lastState.Mouse.LastPosition.Y - Position.Y;
            return (float)MathHelper.RadiansToDegrees(Math.Atan2(mouseYFromCenter,mouseXFromCenter));
        }

        private float GetDistanceToCentre()
        {
            float distanceX = (float)Math.Pow(lastState.Mouse.LastPosition.X - Position.X, 2);
            float distanceY = (float)Math.Pow(lastState.Mouse.LastPosition.Y - Position.Y, 2);
            return (float)Math.Sqrt(distanceX + distanceY);
        }

        public float GetAngledDifference()
        {
            lastAngle = ActualAngle ?? GetMouseAngledPosition();
            Delay(1);
            ActualAngle = GetMouseAngledPosition();
            if (lastAngle < -60 && ActualAngle > 60)
                lastAngle += 360;
            else if (lastAngle > 60 && ActualAngle < -60)
                lastAngle -= 360;
            spinDirectionDiscriminator = MathHelper.Clamp(spinDirectionDiscriminator += (float)(ActualAngle - lastAngle), -110, 110);
            return (float)(ActualAngle - lastAngle);
        }
    }
}
