// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.Sprites
{
    public sealed partial class TruncatingSpriteText : OsuSpriteText, IHasTooltip
    {
        public LocalisableString TooltipText => Text;

        public override bool HandlePositionalInput => IsTruncated;

        public TruncatingSpriteText()
        {
            Truncate = true;
        }
    }
}
