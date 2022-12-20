using System;
using osu.Framework.Utils;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Extensions
{
    public static class ProjectileExtensions
    {
        public static ProjectileInfo GetProjectileInfoAt(ProjectileInfo startParameters, float timeOffset, float pixelToMeterRatio = 15f)
        {
            float durationInSeconds = timeOffset / 1000;

            float speedX = startParameters.Speed * (float)Math.Cos(startParameters.Angle * Math.PI / 180f);
            float speedY = startParameters.Speed * (float)Math.Sin(startParameters.Angle * Math.PI / 180f);

            float xOffset = speedX * durationInSeconds;
            float yOffset = speedY * durationInSeconds + 4.9f * durationInSeconds * durationInSeconds;

            Vector2 position = startParameters.Position + new Vector2(xOffset, yOffset) * pixelToMeterRatio;

            float finalSpeedY = speedY + 9.8f * durationInSeconds;
            float finalSpeed = (float)Math.Sqrt(speedX * speedX + finalSpeedY * finalSpeedY);
            float sin = speedX / finalSpeed;
            float finalAngle = (float)Math.Abs(Math.Asin(sin) * 180f / Math.PI); // from 0 to 90

            if (startParameters.Angle > -90)
                finalAngle = finalSpeedY > 0 ? (90 - finalAngle) : finalAngle - 90;
            else
                finalAngle = finalSpeedY > 0 ? finalAngle - 270 : (-90 - finalAngle);

            return new ProjectileInfo(position, finalSpeed, finalAngle);
        }

        public static (ProjectileInfo info, bool collided) ProcessCollision(ProjectileInfo atCollision, float surfaceNormalAngle)
        {
            var result = hitResultAngle(atCollision.Angle, surfaceNormalAngle);

            if (Precision.AlmostEquals(result, 10000))
                return (atCollision, false);

            return (new ProjectileInfo(atCollision.Position, atCollision.Speed, GetAcceptableAngle(result)), true);
        }

        private static float hitResultAngle(float projectileAngle, float surfaceNormalAngle)
        {
            float offset = offsetBetweenAngles(GetOppositeAngle(projectileAngle), surfaceNormalAngle);

            if (Math.Abs(offset) > 90)
                return 10000;

            float x = surfaceNormalAngle - 90;
            if (x < -270)
                x += 360;

            return 360f + 2 * x - projectileAngle;
        }

        private static float offsetBetweenAngles(float a, float b)
        {
            var radA = a * Math.PI / 180f;
            var radB = b * Math.PI / 180f;

            return (float)(Math.Atan2(Math.Sin(radA - radB), Math.Cos(radA - radB)) * 180f / Math.PI);
        }

        public static float GetAcceptableAngle(float a)
        {
            if (a < -270)
            {
                while (a < 270)
                    a += 360;
            }

            if (a > 90)
            {
                while (a > 90)
                    a -= 360;
            }

            return a;
        }

        public static float GetOppositeAngle(float a)
        {
            float oppositeAngle = a - 180f;
            if (oppositeAngle < -270)
                oppositeAngle += 360;

            return oppositeAngle;
        }
    }

    public partial class ProjectileInfo
    {
        public Vector2 Position { get; private set; }

        public float Speed { get; private set; }

        public float Angle { get; private set; }

        public ProjectileInfo(Vector2 position, float speed, float angle)
        {
            Position = position;
            Speed = speed;
            Angle = angle;
        }

        public override string ToString() => $"Position: {Position}, Speed: {Speed}, Angle: {Angle}";
    }
}
