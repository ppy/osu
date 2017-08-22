// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using OpenTK.Input;
using osu.Framework.Input;
using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="HoldNote"/> hit object.
    /// </summary>
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>
    {
        private readonly DrawableNote head;
        private readonly DrawableNote tail;

        private readonly BodyPiece bodyPiece;
        private readonly Container<DrawableHoldNoteTick> tickContainer;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        private double? holdStartTime;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        private bool hasBroken;

        public DrawableHoldNote(HoldNote hitObject, Bindable<Key> key = null)
            : base(hitObject, key)
        {
            RelativeSizeAxes = Axes.Both;
            Height = (float)HitObject.Duration;

            AddRange(new Drawable[]
            {
                // For now the body piece covers the entire height of the container
                // whereas possibly in the future we don't want to extend under the head/tail.
                // This will be fixed when new designs are given or the current design is finalized.
                bodyPiece = new BodyPiece
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                },
                tickContainer = new Container<DrawableHoldNoteTick>
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativeChildOffset = new Vector2(0, (float)HitObject.StartTime),
                    RelativeChildSize = new Vector2(1, (float)HitObject.Duration)
                },
                head = new DrawableHeadNote(this, key)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                tail = new DrawableTailNote(this, key)
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

                bodyPiece.AccentColour = value;
                head.AccentColour = value;
                tail.AccentColour = value;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            // Make sure the keypress happened within the body of the hold note
            if (Time.Current < HitObject.StartTime || Time.Current > HitObject.EndTime)
                return false;

            if (args.Key != Key)
                return false;

            if (args.Repeat)
                return false;

            // The user has pressed during the body of the hold note, after the head note and its hit windows have passed
            // and within the limited range of the above if-statement. This state will be managed by the head note if the
            // user has pressed during the hit windows of the head note.
            holdStartTime = Time.Current;

            return true;
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            // Make sure that the user started holding the key during the hold note
            if (!holdStartTime.HasValue)
                return false;

            if (args.Key != Key)
                return false;

            holdStartTime = null;

            // If the key has been released too early, the user should not receive full score for the release
            if (!tail.Judged)
                hasBroken = true;

            return true;
        }

        /// <summary>
        /// The head note of a hold.
        /// </summary>
        private class DrawableHeadNote : DrawableNote
        {
            private readonly DrawableHoldNote holdNote;

            public DrawableHeadNote(DrawableHoldNote holdNote, Bindable<Key> key = null)
                : base(holdNote.HitObject.Head, key)
            {
                this.holdNote = holdNote;

                RelativePositionAxes = Axes.None;
                Y = 0;
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                if (!base.OnKeyDown(state, args))
                    return false;

                // We only want to trigger a holding state from the head if the head has received a judgement
                if (!Judged)
                    return false;

                // If the key has been released too early, the user should not receive full score for the release
                if (Judgement.Result == HitResult.Miss)
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

            public DrawableTailNote(DrawableHoldNote holdNote, Bindable<Key> key = null)
                : base(holdNote.HitObject.Tail, key)
            {
                this.holdNote = holdNote;

                RelativePositionAxes = Axes.None;
                Y = 0;
            }

            protected override ManiaJudgement CreateJudgement() => new HoldNoteTailJudgement();

            protected override void CheckJudgement(bool userTriggered)
            {
                base.CheckJudgement(userTriggered);

                var tailJudgement = Judgement as HoldNoteTailJudgement;
                if (tailJudgement == null)
                    return;

                tailJudgement.HasBroken = holdNote.hasBroken;
            }

            protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
            {
                // Make sure that the user started holding the key during the hold note
                if (!holdNote.holdStartTime.HasValue)
                    return false;

                if (Judgement.Result != HitResult.None)
                    return false;

                if (args.Key != Key)
                    return false;

                UpdateJudgement(true);

                // Handled by the hold note, which will set holding = false
                return false;
            }

            protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
            {
                // Tail doesn't handle key down
                return false;
            }
        }
    }
}
