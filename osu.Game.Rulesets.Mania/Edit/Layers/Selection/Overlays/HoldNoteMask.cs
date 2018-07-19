// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class HoldNoteMask : HitObjectMask
    {
        private readonly BodyPiece body;

        public HoldNoteMask(DrawableHoldNote hold)
            : base(hold)
        {
            InternalChildren = new Drawable[]
            {
                new HoldNoteNoteMask(hold.Head),
                new HoldNoteNoteMask(hold.Tail),
                body = new BodyPiece
                {
                    AccentColour = Color4.Transparent
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            body.BorderColour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            Size = HitObject.DrawSize;
            Position = Parent.ToLocalSpace(HitObject.ScreenSpaceDrawQuad.TopLeft);
        }

        private class HoldNoteNoteMask : NoteMask
        {
            public HoldNoteNoteMask(DrawableNote note)
                : base(note)
            {
                Select();
            }

            protected override void Update()
            {
                base.Update();

                Position = HitObject.DrawPosition;
            }
        }
    }
}
