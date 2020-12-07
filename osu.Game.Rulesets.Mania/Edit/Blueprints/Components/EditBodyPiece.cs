// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Skinning.Default;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints.Components
{
    public class EditBodyPiece : DefaultBodyPiece
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour.Value = colours.Yellow;

            Background.Alpha = 0.5f;
        }

        protected override Drawable CreateForeground() => base.CreateForeground().With(d => d.Alpha = 0);
    }
}
