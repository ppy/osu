// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.Skinning.Default;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    /// <summary>
    /// Represents a <see cref="Droplet"/> caught by the catcher.
    /// </summary>
    public class CaughtDroplet : CaughtObject
    {
        public override bool StaysOnPlate => false;

        public CaughtDroplet()
            : base(CatchSkinComponents.Droplet, _ => new DropletPiece())
        {
        }
    }
}
