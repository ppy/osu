// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="HoldNote"/> hit object.
    /// </summary>
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>, IKeyBindingHandler<ManiaAction>
    {
        public override bool DisplayResult => false;

        public DrawableNote Head => headContainer.Child;
        public DrawableNote Tail => tailContainer.Child;

        private readonly Container<DrawableHoldNoteHead> headContainer;
        private readonly Container<DrawableHoldNoteTail> tailContainer;
        private readonly Container<DrawableHoldNoteTick> tickContainer;

        private readonly BodyPiece bodyPiece;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        internal double? HoldStartTime;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        internal bool HasBroken;

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;

            AddRangeInternal(new Drawable[]
            {
                bodyPiece = new BodyPiece { RelativeSizeAxes = Axes.X },
                tickContainer = new Container<DrawableHoldNoteTick> { RelativeSizeAxes = Axes.Both },
                headContainer = new Container<DrawableHoldNoteHead> { RelativeSizeAxes = Axes.Both },
                tailContainer = new Container<DrawableHoldNoteTail> { RelativeSizeAxes = Axes.Both },
            });

            AccentColour.BindValueChanged(colour =>
            {
                bodyPiece.AccentColour = colour.NewValue;
            }, true);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableHoldNoteHead head:
                    headContainer.Child = head;
                    break;

                case DrawableHoldNoteTail tail:
                    tailContainer.Child = tail;
                    break;

                case DrawableHoldNoteTick tick:
                    tickContainer.Add(tick);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            headContainer.Clear();
            tailContainer.Clear();
            tickContainer.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case TailNote _:
                    return new DrawableHoldNoteTail(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };

                case Note _:
                    return new DrawableHoldNoteHead(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };

                case HoldNoteTick tick:
                    return new DrawableHoldNoteTick(tick)
                    {
                        HoldStartTime = () => HoldStartTime,
                        AccentColour = { BindTarget = AccentColour }
                    };
            }

            return base.CreateNestedHitObject(hitObject);
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            base.OnDirectionChanged(e);

            bodyPiece.Anchor = bodyPiece.Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Tail.AllJudged)
                ApplyResult(r => r.Type = HitResult.Perfect);
        }

        protected override void Update()
        {
            base.Update();

            // Make the body piece not lie under the head note
            bodyPiece.Y = (Direction.Value == ScrollingDirection.Up ? 1 : -1) * Head.Height / 2;
            bodyPiece.Height = DrawHeight - Head.Height / 2 + Tail.Height / 2;
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            using (BeginDelayedSequence(HitObject.Duration, true))
                base.UpdateStateTransforms(state);
        }

        internal void BeginHold()
        {
            HoldStartTime = Time.Current;
            bodyPiece.Hitting = true;
        }

        protected void EndHold()
        {
            HoldStartTime = null;
            bodyPiece.Hitting = false;
        }

        public bool OnPressed(ManiaAction action)
        {
            // Make sure the action happened within the body of the hold note
            if (Time.Current < HitObject.StartTime || Time.Current > HitObject.EndTime)
                return false;

            if (action != Action.Value)
                return false;

            // The user has pressed during the body of the hold note, after the head note and its hit windows have passed
            // and within the limited range of the above if-statement. This state will be managed by the head note if the
            // user has pressed during the hit windows of the head note.
            BeginHold();
            return true;
        }

        public bool OnReleased(ManiaAction action)
        {
            // Make sure that the user started holding the key during the hold note
            if (!HoldStartTime.HasValue)
                return false;

            if (action != Action.Value)
                return false;

            EndHold();

            // If the key has been released too early, the user should not receive full score for the release
            if (!Tail.IsHit)
                HasBroken = true;

            return true;
        }
    }
}
