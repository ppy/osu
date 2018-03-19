using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using System;

namespace Symcol.Core.Graphics.Containers
{
    public class SymcolDialContainer : CircularContainer
    {
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => true;

        private Vector2 mousePosition;

        private float lastAngle;
        private float currentRotation;
        public float RotationAbsolute;

        private int completeTick;

        private bool updateCompleteTick() => completeTick != (completeTick = (int)(RotationAbsolute / 360));

        private bool rotationTransferred;

        protected override bool OnMouseMove(InputState state)
        {
            mousePosition = Parent.ToLocalSpace(state.Mouse.NativeState.Position);
            return base.OnMouseMove(state);
        }

        protected override void Update()
        {
            base.Update();

            var thisAngle = -(float)MathHelper.RadiansToDegrees(Math.Atan2(mousePosition.X - DrawSize.X / 2, mousePosition.Y - DrawSize.Y / 2));


            if (!rotationTransferred)
            {
                currentRotation = Rotation * 2;
                rotationTransferred = true;
            }

            if (thisAngle - lastAngle > 180)
                lastAngle += 360;
            else if (lastAngle - thisAngle > 180)
                lastAngle -= 360;

            currentRotation += thisAngle - lastAngle;
            RotationAbsolute += Math.Abs(thisAngle - lastAngle);

            lastAngle = thisAngle;

            foreach(Drawable drawable in Children)
                drawable.RotateTo(currentRotation / 2, 200, Easing.OutExpo);
        }
    }
}
