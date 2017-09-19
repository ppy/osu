// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK.Graphics;
using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Input.Bindings;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="HoldNote"/> hit object.
    /// </summary>
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>, IKeyBindingHandler<ManiaAction>
    {
        private readonly DrawableNote head;
        private readonly DrawableNote tail;

        private readonly GlowPiece glowPiece;
        private readonly BodyPiece bodyPiece;
        private readonly Container<DrawableHoldNoteTick> tickContainer;
        private readonly Container fullHeightContainer;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        private double? holdStartTime;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        private bool hasBroken;

        public DrawableHoldNote(HoldNote hitObject, ManiaAction action)
            : base(hitObject, action)
        {
            RelativeSizeAxes = Axes.Both;
            Height = (float)HitObject.Duration;

            AddRange(new Drawable[]
            {
                // The hit object itself cannot be used for various elements because the tail overshoots it
                // So a specialized container that is updated to contain the tail height is used
                fullHeightContainer = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Child = glowPiece = new GlowPiece()
                },
                bodyPiece = new BodyPiece
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                },
                tickContainer = new Container<DrawableHoldNoteTick>
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativeChildOffset = new Vector2(0, (float)HitObject.StartTime),
                    RelativeChildSize = new Vector2(1, (float)HitObject.Duration)
                },
                head = new DrawableHeadNote(this, action)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                tail = new DrawableTailNote(this, action)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre
                }
            });

            foreach (var tick in HitObject.Ticks)
            {
                var drawableTick = new DrawableHoldNoteTick(tick)
                {
                    HoldStartTime = () => holdStartTime
                };

                tickContainer.Add(drawableTick);
                AddNested(drawableTick);
            }

            AddNested(head);
            AddNested(tail);
        }

        public override Color4 AccentColour
        {
            get { return base.AccentColour; }
            set
            {
                if (base.AccentColour == value)
                    return;
                base.AccentColour = value;

                tickContainer.Children.ForEach(t => t.AccentColour = value);

                glowPiece.AccentColour = value;
                bodyPiece.AccentColour = value;
                head.AccentColour = value;
                tail.AccentColour = value;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
        }

        protected override void Update()
        {
            base.Update();

            // Make the body piece not lie under the head note
            bodyPiece.Y = head.Height;
            bodyPiece.Height = DrawHeight - head.Height;

            // Make the fullHeightContainer "contain" the height of the tail note, keeping in mind
            // that the tail note overshoots the height of this hit object
            fullHeightContainer.Height = DrawHeight + tail.Height;
        }

        public bool OnPressed(ManiaAction action)
        {
            // Make sure the action happened within the body of the hold note
            if (Time.Current < HitObject.StartTime || Time.Current > HitObject.EndTime)
                return false;

            if (action != Action)
                return false;

            // The user has pressed during the body of the hold note, after the head note and its hit windows have passed
            // and within the limited range of the above if-statement. This state will be managed by the head note if the
            // user has pressed during the hit windows of the head note.
            holdStartTime = Time.Current;

            return true;
        }

        public bool OnReleased(ManiaAction action)
        {
            // Make sure that the user started holding the key during the hold note
            if (!holdStartTime.HasValue)
                return false;

            if (action != Action)
                return false;

            holdStartTime = null;

            // If the key has been released too early, the user should not receive full score for the release
            if (!tail.AllJudged)
                hasBroken = true;

            return true;
        }

        /// <summary>
        /// The head note of a hold.
        /// </summary>
        private class DrawableHeadNote : DrawableNote
        {
            private readonly DrawableHoldNote holdNote;

            public DrawableHeadNote(DrawableHoldNote holdNote, ManiaAction action)
                : base(holdNote.HitObject.Head, action)
            {
                this.holdNote = holdNote;

                RelativePositionAxes = Axes.None;
                Y = 0;

                // Life time managed by the parent DrawableHoldNote
                LifetimeStart = double.MinValue;
                LifetimeEnd = double.MaxValue;

                GlowPiece.Alpha = 0;
            }

            public override bool OnPressed(ManiaAction action)
            {
                if (!base.OnPressed(action))
                    return false;

                // If the key has been released too early, the user should not receive full score for the release
                if (Judgements.Any(j => j.Result == HitResult.Miss))
                    holdNote.hasBroken = true;

                // The head note also handles early hits before the body, but we want accurate early hits to count as the body being held
                // The body doesn't handle these early early hits, so we have to explicitly set the holding state here
                holdNote.holdStartTime = Time.Current;

                return true;
            }
        }

        /// <summary>
        /// The tail note of a hold.
        /// </summary>
        private class DrawableTailNote : DrawableNote
        {
            private readonly DrawableHoldNote holdNote;

            public DrawableTailNote(DrawableHoldNote holdNote, ManiaAction action)
                : base(holdNote.HitObject.Tail, action)
            {
                this.holdNote = holdNote;

                RelativePositionAxes = Axes.None;
                Y = 0;

                // Life time managed by the parent DrawableHoldNote
                LifetimeStart = double.MinValue;
                LifetimeEnd = double.MaxValue;

                GlowPiece.Alpha = 0;
            }

            protected override void CheckForJudgements(bool userTriggered, double timeOffset)
            {
                if (!userTriggered)
                {
                    if (timeOffset > HitObject.HitWindows.Bad / 2)
                    {
                        AddJudgement(new HoldNoteTailJudgement
                        {
                            Result = HitResult.Miss,
                            HasBroken = holdNote.hasBroken
                        });
                    }

                    return;
                }

                double offset = Math.Abs(timeOffset);

                if (offset > HitObject.HitWindows.Miss / 2)
                    return;

                AddJudgement(new HoldNoteTailJudgement
                {
                    Result = HitObject.HitWindows.ResultFor(offset) ?? HitResult.Miss,
                    HasBroken = holdNote.hasBroken
                });
            }

            public override bool OnPressed(ManiaAction action) => false; // Tail doesn't handle key down

            public override bool OnReleased(ManiaAction action)
            {
                // Make sure that the user started holding the key during the hold note
                if (!holdNote.holdStartTime.HasValue)
                    return false;

                if (action != Action)
                    return false;

                UpdateJudgement(true);

                // Handled by the hold note, which will set holding = false
                return false;
            }
        }
    }
}
