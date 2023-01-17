// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Skinning.Default;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    /// <summary>
    /// Represents a <see cref="Banana"/> caught by the catcher.
    /// </summary>
    public partial class CaughtBanana : CaughtObject
    {
        public CaughtBanana()
            : base(CatchSkinComponents.Banana, _ => new BananaPiece())
        {
        }
    }
}
