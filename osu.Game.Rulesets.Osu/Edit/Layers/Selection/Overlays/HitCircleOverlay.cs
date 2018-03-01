// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit.Layers.Selection;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Edit.Layers.Selection.Overlays
{
    public class HitCircleOverlay : HitObjectOverlay
    {
        public HitCircleOverlay(DrawableHitCircle hitCircle)
            : base(hitCircle)
        {
            Origin = Anchor.Centre;

            Position = hitCircle.Position;
            Size = hitCircle.Size;
            Scale = hitCircle.Scale;

            AddInternal(new RingPiece());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }
    }
}
