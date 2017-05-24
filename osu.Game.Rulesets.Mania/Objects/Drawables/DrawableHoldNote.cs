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
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>
    {
        private readonly DrawableNote headNote;
        private readonly DrawableNote tailNote;
        private readonly BodyPiece bodyPiece;
        private readonly Container<DrawableHoldNoteTick> tickContainer;

        /// <summary>
        /// Relative time at which the user started holding this note.
        /// </summary>
        private double holdStartTime;

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        private bool hasBroken;

        private bool _holding;
        /// <summary>
        /// Whether the user is holding the hold note.
        /// </summary>
        private bool holding
        {
            get { return _holding; }
            set
            {
                if (_holding == value)
                    return;
                _holding = value;

                if (holding)
                    holdStartTime = Time.Current;
            }
        }

        public DrawableHoldNote(HoldNote hitObject, Bindable<Key> key = null)
            : base(hitObject, key)
        {
            RelativeSizeAxes = Axes.Both;
            Height = (float)HitObject.Duration;

            Add(new Drawable[]
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
                    RelativeCoordinateSpace = new Vector2(1, (float)HitObject.Duration)
                },
                headNote = new DrawableHoldNoteHead(this, key)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                tailNote = new DrawableHoldNoteTail(this, key)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.TopCentre
                }
            });

            foreach (var tick in HitObject.Ticks)
            {
                var drawableTick = new DrawableHoldNoteTick(tick)
                {
                    IsHolding = () => holding,
                    HoldStartTime = () => holdStartTime
                };

                drawableTick.Y -= (float)HitObject.StartTime;

                tickContainer.Add(drawableTick);
                AddNested(drawableTick);
            }

            AddNested(headNote);
            AddNested(tailNote);
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
                headNote.AccentColour = value;
                tailNote.AccentColour = value;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
        }

        /// <summary>
        /// Handles key down events on the body of the hold note.
        /// </summary>
        /// <param name="state">The input state.</param>
        /// <param name="args">The key down args.</param>
        /// <returns>Whether the key press was handled.</returns>
        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            // Make sure the keypress happened within reasonable bounds of the hold note
            if (Time.Current < HitObject.StartTime || Time.Current > HitObject.EndTime)
                return false;

            if (args.Key != Key)
                return false;

            if (args.Repeat)
                return false;

            holding = true;

            return true;
        }

        /// <summary>
        /// Handles key up events on the body of the hold note.
        /// </summary>
        /// <param name="state">The input state.</param>
        /// <param name="args">The key down args.</param>
        /// <returns>Whether the key press was handled.</returns>
        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            // Make sure that the user started holding the key during the hold note
            if (!holding)
                return false;

            if (args.Key != Key)
                return false;

            holding = false;

            // If the key has been released too early, they should not receive full score for the release
            if (!tailNote.Judged)
                hasBroken = true;

            return true;
        }

        private class DrawableHoldNoteHead : DrawableNote
        {
            private readonly DrawableHoldNote holdNote;

            public DrawableHoldNoteHead(DrawableHoldNote holdNote, Bindable<Key> key = null)
                : base(holdNote.HitObject.HeadNote, key)
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
                if (Judgement.Result == HitResult.None)
                    return false;

                // If the head has been missed, make sure the user also can't receive a full score for the release
                if (Judgement.Result == HitResult.Miss)
                    holdNote.hasBroken = true;

                holdNote.holding = true;

                return true;
            }
        }

        private class DrawableHoldNoteTail : DrawableNote
        {
            private readonly DrawableHoldNote holdNote;

            public DrawableHoldNoteTail(DrawableHoldNote holdNote, Bindable<Key> key = null)
                : base(holdNote.HitObject.TailNote, key)
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
                if (!holdNote.holding)
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
