// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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

        private readonly Container<DrawableHeadNote> headContainer;
        private readonly Container<DrawableTailNote> tailContainer;
        private readonly Container<DrawableHoldNoteTick> tickContainer;

        private readonly BodyPiece bodyPiece;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        private double? holdStartTime;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        private bool hasBroken;

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;

            AddRangeInternal(new Drawable[]
            {
                bodyPiece = new BodyPiece { RelativeSizeAxes = Axes.X },
                tickContainer = new Container<DrawableHoldNoteTick> { RelativeSizeAxes = Axes.Both },
                headContainer = new Container<DrawableHeadNote> { RelativeSizeAxes = Axes.Both },
                tailContainer = new Container<DrawableTailNote> { RelativeSizeAxes = Axes.Both },
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
                case DrawableHeadNote head:
                    headContainer.Child = head;
                    break;

                case DrawableTailNote tail:
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
                    return new DrawableTailNote(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };

                case Note _:
                    return new DrawableHeadNote(this)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AccentColour = { BindTarget = AccentColour }
                    };

                case HoldNoteTick tick:
                    return new DrawableHoldNoteTick(tick)
                    {
                        HoldStartTime = () => holdStartTime,
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

        protected void BeginHold()
        {
            holdStartTime = Time.Current;
            bodyPiece.Hitting = true;
        }

        protected void EndHold()
        {
            holdStartTime = null;
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
            if (!holdStartTime.HasValue)
                return false;

            if (action != Action.Value)
                return false;

            EndHold();

            // If the key has been released too early, the user should not receive full score for the release
            if (!Tail.IsHit)
                hasBroken = true;

            return true;
        }

        /// <summary>
        /// The head note of a hold.
        /// </summary>
        private class DrawableHeadNote : DrawableNote
        {
            private readonly DrawableHoldNote holdNote;

            public DrawableHeadNote(DrawableHoldNote holdNote)
                : base(holdNote.HitObject.Head)
            {
                this.holdNote = holdNote;
            }

            public override bool OnPressed(ManiaAction action)
            {
                if (!base.OnPressed(action))
                    return false;

                // If the key has been released too early, the user should not receive full score for the release
                if (Result.Type == HitResult.Miss)
                    holdNote.hasBroken = true;

                // The head note also handles early hits before the body, but we want accurate early hits to count as the body being held
                // The body doesn't handle these early early hits, so we have to explicitly set the holding state here
                holdNote.BeginHold();

                return true;
            }
        }

        /// <summary>
        /// The tail note of a hold.
        /// </summary>
        private class DrawableTailNote : DrawableNote
        {
            /// <summary>
            /// Lenience of release hit windows. This is to make cases where the hold note release
            /// is timed alongside presses of other hit objects less awkward.
            /// Todo: This shouldn't exist for non-LegacyBeatmapDecoder beatmaps
            /// </summary>
            private const double release_window_lenience = 1.5;

            private readonly DrawableHoldNote holdNote;

            public DrawableTailNote(DrawableHoldNote holdNote)
                : base(holdNote.HitObject.Tail)
            {
                this.holdNote = holdNote;
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                Debug.Assert(HitObject.HitWindows != null);

                // Factor in the release lenience
                timeOffset /= release_window_lenience;

                if (!userTriggered)
                {
                    if (!HitObject.HitWindows.CanBeHit(timeOffset))
                        ApplyResult(r => r.Type = HitResult.Miss);

                    return;
                }

                var result = HitObject.HitWindows.ResultFor(timeOffset);
                if (result == HitResult.None)
                    return;

                ApplyResult(r =>
                {
                    if (holdNote.hasBroken && (result == HitResult.Perfect || result == HitResult.Perfect))
                        result = HitResult.Good;

                    r.Type = result;
                });
            }

            public override bool OnPressed(ManiaAction action) => false; // Tail doesn't handle key down

            public override bool OnReleased(ManiaAction action)
            {
                // Make sure that the user started holding the key during the hold note
                if (!holdNote.holdStartTime.HasValue)
                    return false;

                if (action != Action.Value)
                    return false;

                UpdateResult(true);

                // Handled by the hold note, which will set holding = false
                return false;
            }
        }
    }
}
