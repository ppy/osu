// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints.Components
{
    public class EditBodyPiece : BodyPiece
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.Yellow;

            Background.Alpha = 0.5f;
            Foreground.Alpha = 0;
        }
    }
}
