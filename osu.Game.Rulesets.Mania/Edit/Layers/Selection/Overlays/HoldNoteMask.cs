// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class HoldNoteMask : HitObjectMask
    {
        private readonly BodyPiece body;
        private readonly DrawableHoldNote holdNote;

        public HoldNoteMask(DrawableHoldNote hold)
            : base(hold)
        {
            holdNote = hold;

            Position = hold.Position;

            var holdObject = hold.HitObject;

            InternalChildren = new Drawable[]
            {
                new NoteMask(hold.Head),
                new NoteMask(hold.Tail),
                body = new BodyPiece()
                {
                    AccentColour = Color4.Transparent
                },
            };

            holdObject.ColumnChanged += _ => Position = hold.Position;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;
        }
    }
}
