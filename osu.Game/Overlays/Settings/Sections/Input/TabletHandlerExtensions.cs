// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Handlers.Tablet;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public static class TabletHandlerExtensions
    {
        public static Vector2 ClampOffset(this ITabletHandler handler, Vector2 offset)
        {
            if (handler.Tablet.Value == null)
                return offset;

            var size = handler.AreaSize.Value;
            var tabletSize = handler.Tablet.Value.Size;

            float rad = float.DegreesToRadians(handler.Rotation.Value);
            float cos = MathF.Abs(MathF.Cos(rad));
            float sin = MathF.Abs(MathF.Sin(rad));

            float maxX = (size.X / 2) * cos + (size.Y / 2) * sin;
            float maxY = (size.X / 2) * sin + (size.Y / 2) * cos;

            float minX = MathF.Min(maxX, tabletSize.X / 2);
            float maxXRange = MathF.Max(tabletSize.X - maxX, tabletSize.X / 2);
            float minY = MathF.Min(maxY, tabletSize.Y / 2);
            float maxYRange = MathF.Max(tabletSize.Y - maxY, tabletSize.Y / 2);

            return new Vector2(
                Math.Clamp(offset.X, minX, maxXRange),
                Math.Clamp(offset.Y, minY, maxYRange)
            );
        }
    }
}
