// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Mania.Edit.Blueprints.Components;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Mania.Edit.Blueprints
{
    public partial class HoldNoteSelectionBlueprint : ManiaSelectionBlueprint<HoldNote>
    {
        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved]
        private EditorBeatmap? editorBeatmap { get; set; }

        [Resolved]
        private ManiaHitObjectComposer? positionSnapProvider { get; set; }

        private EditBodyPiece body = null!;
        private EditHoldNoteEndPiece head = null!;
        private EditHoldNoteEndPiece tail = null!;

        protected new DrawableHoldNote DrawableObject => (DrawableHoldNote)base.DrawableObject;

        public HoldNoteSelectionBlueprint(HoldNote hold)
            : base(hold)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                body = new EditBodyPiece
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
                head = new EditHoldNoteEndPiece
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    DragStarted = () => changeHandler?.BeginChange(),
                    Dragging = pos =>
                    {
                        double endTimeBeforeDrag = HitObject.EndTime;
                        double proposedStartTime = positionSnapProvider?.FindSnappedPositionAndTime(pos).Time ?? HitObjectContainer.TimeAtScreenSpacePosition(pos);
                        double proposedEndTime = endTimeBeforeDrag;

                        if (proposedStartTime >= proposedEndTime)
                            return;

                        HitObject.StartTime = proposedStartTime;
                        HitObject.EndTime = proposedEndTime;
                        editorBeatmap?.Update(HitObject);
                    },
                    DragEnded = () => changeHandler?.EndChange(),
                },
                tail = new EditHoldNoteEndPiece
                {
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    DragStarted = () => changeHandler?.BeginChange(),
                    Dragging = pos =>
                    {
                        double proposedStartTime = HitObject.StartTime;
                        double proposedEndTime = positionSnapProvider?.FindSnappedPositionAndTime(pos).Time ?? HitObjectContainer.TimeAtScreenSpacePosition(pos);

                        if (proposedStartTime >= proposedEndTime)
                            return;

                        HitObject.StartTime = proposedStartTime;
                        HitObject.EndTime = proposedEndTime;
                        editorBeatmap?.Update(HitObject);
                    },
                    DragEnded = () => changeHandler?.EndChange(),
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            head.Height = DrawableObject.Head.DrawHeight;
            head.Y = HitObjectContainer.PositionAtTime(HitObject.Head.StartTime, HitObject.StartTime);
            tail.Height = DrawableObject.Tail.DrawHeight;
            tail.Y = HitObjectContainer.PositionAtTime(HitObject.Tail.StartTime, HitObject.StartTime);
            Height = HitObjectContainer.LengthAtTime(HitObject.StartTime, HitObject.EndTime) + tail.DrawHeight;
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            Origin = direction.NewValue == ScrollingDirection.Down ? Anchor.BottomCentre : Anchor.TopCentre;

            foreach (var child in InternalChildren)
                child.Anchor = Origin;

            head.Scale = tail.Scale = body.Scale = new Vector2(1, direction.NewValue == ScrollingDirection.Down ? 1 : -1);
        }

        public override Quad SelectionQuad => ScreenSpaceDrawQuad;

        public override Vector2 ScreenSpaceSelectionPoint => head.ScreenSpaceDrawQuad.Centre;
    }
}
