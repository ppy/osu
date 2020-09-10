using System;
using osuTK;

namespace osu.Game.Rulesets.Tau
{
    public static class Extensions
    {
        public static Vector2 GetCircularPosition(float distance, float angle)
            => new Vector2(-(distance * (float)Math.Cos((angle + 90f) * (float)(Math.PI / 180))), -(distance * (float)Math.Sin((angle + 90f) * (float)(Math.PI / 180))));

        public static float GetDegreesFromPosition(this Vector2 a, Vector2 b)
        {
            Vector2 direction = b - a;
            float angle = MathHelper.RadiansToDegrees(MathF.Atan2(direction.Y, direction.X));
            if (angle < 0f) angle += 360f;

            return angle + 90;
        }

        public static float GetHitObjectAngle(this Vector2 target)
        {
            Vector2 offset = new Vector2(256, 192) - target; // Using centre of playfield.

            return (float)MathHelper.RadiansToDegrees(Math.Atan2(offset.X, -offset.Y)) - 180;
        }
    }
}
