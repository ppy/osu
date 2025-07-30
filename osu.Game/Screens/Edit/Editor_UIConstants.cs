// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit
{
    public partial class Editor
    {
        public const float BUTTON_HEIGHT = 32;
        public const float BUTTON_CORNER_RADIUS = 3;
        public const float BUTTON_ICON_SIZE = 16;

        public static class Fonts
        {
            public static FontUsage Default => OsuFont.Style.Caption1;

            public static FontUsage Heading => OsuFont.Style.Body.With(weight: FontWeight.Bold);
        }
    }
}
