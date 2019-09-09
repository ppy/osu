// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public class LegacyCursorTrail : CursorTrail
    {
        public LegacyCursorTrail()
        {
            Blending = BlendingParameters.Additive;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            Texture = skin.GetTexture("cursortrail");
        }
    }
}
