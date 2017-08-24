// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        public DrawableHoldNote(HoldNote hitObject, ManiaAction action)
            : base(hitObject, action)
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

                bodyPiece.AccentColour = value;
                head.AccentColour = value;
                tail.AccentColour = value;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
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

            public DrawableHeadNote(DrawableHoldNote holdNote, ManiaAction action)
                : base(holdNote.HitObject.Head, action)
            {
                this.holdNote = holdNote;

                RelativePositionAxes = Axes.None;
                Y = 0;
            }

            public override bool OnPressed(ManiaAction action)
            {
                if (!base.OnPressed(action))
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

            public DrawableTailNote(DrawableHoldNote holdNote, ManiaAction action)
                : base(holdNote.HitObject.Tail, action)
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

            public override bool OnPressed(ManiaAction action) => false; // Tail doesn't handle key down

            public override bool OnReleased(ManiaAction action)
            {
                // Make sure that the user started holding the key during the hold note
                if (!holdNote.holdStartTime.HasValue)
                    return false;

                if (Judgement.Result != HitResult.None)
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
