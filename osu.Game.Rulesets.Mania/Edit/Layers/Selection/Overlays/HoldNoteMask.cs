// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI.Scrolling;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit.Layers.Selection.Overlays
{
    public class HoldNoteMask : HitObjectMask
    {
        public new DrawableHoldNote HitObject => (DrawableHoldNote)base.HitObject;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

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
        private void load(OsuColour colours, IScrollingInfo scrollingInfo)
        {
            body.BorderColour = colours.Yellow;

            direction.BindTo(scrollingInfo.Direction);
        }

        protected override void Update()
        {
            base.Update();

            Size = HitObject.DrawSize + new Vector2(0, HitObject.Tail.DrawHeight);
            Position = Parent.ToLocalSpace(HitObject.ScreenSpaceDrawQuad.TopLeft);

            // This is a side-effect of not matching the hitobject's anchors/origins, which is kinda hard to do
            // When scrolling upwards our origin is already at the top of the head note (which is the intended location),
            // but when scrolling downwards our origin is at the _bottom_ of the tail note (where we need to be at the _top_ of the tail note)
            if (direction.Value == ScrollingDirection.Down)
                Y -= HitObject.Tail.DrawHeight;
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

                Anchor = HitObject.Anchor;
                Origin = HitObject.Origin;

                Position = HitObject.DrawPosition;
            }

            // Todo: This is temporary, since the note masks don't do anything special yet. In the future they will handle input.
            public override bool HandlePositionalInput => false;
        }
    }
}
