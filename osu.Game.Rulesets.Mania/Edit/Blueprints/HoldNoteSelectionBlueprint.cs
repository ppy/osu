// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public class HoldNoteSelectionBlueprint : ManiaSelectionBlueprint
    {
        public new DrawableHoldNote DrawableObject => (DrawableHoldNote)base.DrawableObject;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        [Resolved]
        private OsuColour colours { get; set; }

        public HoldNoteSelectionBlueprint(DrawableHoldNote hold)
            : base(hold)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction.BindTo(scrollingInfo.Direction);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new Drawable[]
            {
                new HoldNoteNoteSelectionBlueprint(DrawableObject.Head),
                new HoldNoteNoteSelectionBlueprint(DrawableObject.Tail),
                new BodyPiece
                {
                    AccentColour = Color4.Transparent,
                    BorderColour = colours.Yellow
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            Size = DrawableObject.DrawSize + new Vector2(0, DrawableObject.Tail.DrawHeight);

            // This is a side-effect of not matching the hitobject's anchors/origins, which is kinda hard to do
            // When scrolling upwards our origin is already at the top of the head note (which is the intended location),
            // but when scrolling downwards our origin is at the _bottom_ of the tail note (where we need to be at the _top_ of the tail note)
            if (direction.Value == ScrollingDirection.Down)
                Y -= DrawableObject.Tail.DrawHeight;
        }

        public override Quad SelectionQuad => ScreenSpaceDrawQuad;

        private class HoldNoteNoteSelectionBlueprint : NoteSelectionBlueprint
        {
            public HoldNoteNoteSelectionBlueprint(DrawableNote note)
                : base(note)
            {
                Select();
            }

            protected override void Update()
            {
                base.Update();

                Anchor = DrawableObject.Anchor;
                Origin = DrawableObject.Origin;

                Position = DrawableObject.DrawPosition;
            }

            // Todo: This is temporary, since the note masks don't do anything special yet. In the future they will handle input.
            public override bool HandlePositionalInput => false;
        }
    }
}
