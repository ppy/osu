// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Taiko.Skinning.Default;

namespace osu.Game.Rulesets.Taiko.Skinning.Argon
{
    public partial class ArgonSwell : DefaultSwell
    {
        protected override Drawable CreateCentreCircle()
        {
            return new ArgonSwellCirclePiece
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }
    }
}
