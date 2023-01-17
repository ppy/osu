// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public partial class HoldNoteSelectionBlueprint : ManiaSelectionBlueprint<HoldNote>
    {
        [Resolved]
        private OsuColour colours { get; set; }

        private EditNotePiece head;
        private EditNotePiece tail;

        public HoldNoteSelectionBlueprint(HoldNote hold)
            : base(hold)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                head = new EditNotePiece { RelativeSizeAxes = Axes.X },
                tail = new EditNotePiece { RelativeSizeAxes = Axes.X },
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

            head.Y = HitObjectContainer.PositionAtTime(HitObject.Head.StartTime, HitObject.StartTime);
            tail.Y = HitObjectContainer.PositionAtTime(HitObject.Tail.StartTime, HitObject.StartTime);
            Height = HitObjectContainer.LengthAtTime(HitObject.StartTime, HitObject.EndTime) + tail.DrawHeight;
        }

        public override Quad SelectionQuad => ScreenSpaceDrawQuad;

        public override Vector2 ScreenSpaceSelectionPoint => head.ScreenSpaceDrawQuad.Centre;
    }
}
