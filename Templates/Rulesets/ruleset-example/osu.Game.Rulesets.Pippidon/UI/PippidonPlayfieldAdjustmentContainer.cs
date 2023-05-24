// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Pippidon.UI
{
    public partial class PippidonPlayfieldAdjustmentContainer : PlayfieldAdjustmentContainer
    {
        public PippidonPlayfieldAdjustmentContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Size = new Vector2(0.8f);
        }
    }
}
