// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class NoteMask : HitObjectMask
    {
        public NoteMask(DrawableNote note)
            : base(note)
        {
            Scale = note.Scale;

            CornerRadius = 5;
            Masking = true;

            AddInternal(new NotePiece());
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            Size = HitObject.DrawSize;
            Position = Parent.ToLocalSpace(HitObject.ScreenSpaceDrawQuad.TopLeft);
        }
    }
}
