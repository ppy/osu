// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

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
                new HoldNoteNoteSelectionBlueprint(DrawableObject, HoldNotePosition.Start),
                new HoldNoteNoteSelectionBlueprint(DrawableObject, HoldNotePosition.End),
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 1,
                    BorderColour = colours.Yellow,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    }
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            // Todo: This shouldn't exist, mania should not reference the drawable hitobject directly.
            if (DrawableObject.IsLoaded)
            {
                Size = DrawableObject.DrawSize + new Vector2(0, DrawableObject.Tail.DrawHeight);

                // This is a side-effect of not matching the hitobject's anchors/origins, which is kinda hard to do
                // When scrolling upwards our origin is already at the top of the head note (which is the intended location),
                // but when scrolling downwards our origin is at the _bottom_ of the tail note (where we need to be at the _top_ of the tail note)
                if (direction.Value == ScrollingDirection.Down)
                    Y -= DrawableObject.Tail.DrawHeight;
            }
        }

        public override Quad SelectionQuad => ScreenSpaceDrawQuad;

        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.Head.ScreenSpaceDrawQuad.Centre;
    }
}
