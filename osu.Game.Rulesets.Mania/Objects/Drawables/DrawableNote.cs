// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    public class DrawableNote : DrawableManiaHitObject<Note>
    {
        private NotePiece headPiece;

        public DrawableNote(Note hitObject)
            : base(hitObject)
        {
            Add(headPiece = new NotePiece());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            headPiece.AccentColour = AccentColour;
        }

        protected override void UpdateState(ArmedState state)
        {
        }
    }
}
