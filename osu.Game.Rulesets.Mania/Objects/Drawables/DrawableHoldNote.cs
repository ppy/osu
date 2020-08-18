// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects.Drawables.Pieces;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="HoldNote"/> hit object.
    /// </summary>
    public class DrawableHoldNote : DrawableManiaHitObject<HoldNote>, IKeyBindingHandler<ManiaAction>
    {
        public override bool DisplayResult => false;

        public IBindable<bool> IsHitting => isHitting;

        private readonly Bindable<bool> isHitting = new Bindable<bool>();

        public DrawableHoldNoteHead Head => headContainer.Child;
        public DrawableHoldNoteTail Tail => tailContainer.Child;

        private readonly Container<DrawableHoldNoteHead> headContainer;
        private readonly Container<DrawableHoldNoteTail> tailContainer;
        private readonly Container<DrawableHoldNoteTick> tickContainer;

        /// <summary>
        /// Contains the maximum size/position of the body prior to any offset or size adjustments.
        /// </summary>
        private readonly Container bodyContainer;

        /// <summary>
        /// Contains the offset size/position of the body such that the body extends half-way between the head and tail pieces.
        /// </summary>
        private readonly Container bodyOffsetContainer;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        /// <summary>
        /// Whether the hold note has been released too early and shouldn't give full score for the release.
        /// </summary>
        public bool HasBroken { get; private set; }

        /// <summary>
        /// Whether the hold note has been released potentially without having caused a break.
        /// </summary>
        private bool hasReleased;

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;

            AddRangeInternal(new[]
            {
                bodyContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        bodyOffsetContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Child = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HoldNoteBody, hitObject.Column), _ => new DefaultBodyPiece
                            {
                                RelativeSizeAxes = Axes.Both
                            })
                        },
                        // The head needs to move along with changes in the size of the body.
                        headContainer = new Container<DrawableHoldNoteHead> { RelativeSizeAxes = Axes.Both }
                    }
                },
                tickContainer = new Container<DrawableHoldNoteTick> { RelativeSizeAxes = Axes.Both },
                headContainer.CreateProxy(),
                tailContainer = new Container<DrawableHoldNoteTail> { RelativeSizeAxes = Axes.Both },
            });
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

            // The body container is anchored from the position of the tail, since its height is changed when the hold note is being hit.
            // The body offset container is anchored from the position of the head (inverse of the above).
            if (e.NewValue == ScrollingDirection.Up)
            {
                bodyContainer.Anchor = bodyContainer.Origin = Anchor.BottomLeft;
                bodyOffsetContainer.Anchor = bodyOffsetContainer.Origin = Anchor.TopLeft;
            }
            else
            {
                bodyContainer.Anchor = bodyContainer.Origin = Anchor.TopLeft;
                bodyOffsetContainer.Anchor = bodyOffsetContainer.Origin = Anchor.BottomLeft;
            }
        }

        public override void PlaySamples()
        {
            // Samples are played by the head/tail notes.
        }

        protected override void Update()
        {
            base.Update();

            // Decrease the size of the body while the hold note is held and the head has been hit.
            if (Head.IsHit && !hasReleased)
            {
                float heightDecrease = (float)(Math.Max(0, Time.Current - HitObject.StartTime) / HitObject.Duration);
                bodyContainer.Height = MathHelper.Clamp(1 - heightDecrease, 0, 1);
            }

            // Offset the body to extend half-way under the head and tail.
            bodyOffsetContainer.Y = (Direction.Value == ScrollingDirection.Up ? 1 : -1) * Head.Height / 2;
            bodyOffsetContainer.Height = bodyContainer.DrawHeight - Head.Height / 2 + Tail.Height / 2;
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            using (BeginDelayedSequence(HitObject.Duration, true))
                base.UpdateStateTransforms(state);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Tail.AllJudged)
                ApplyResult(r => r.Type = HitResult.Perfect);

            if (Tail.Result.Type == HitResult.Miss)
                HasBroken = true;
        }

        public bool OnPressed(ManiaAction action)
        {
            if (AllJudged)
                return false;

            if (action != Action.Value)
                return false;

            // The tail has a lenience applied to it which is factored into the miss window (i.e. the miss judgement will be delayed).
            // But the hold cannot ever be started within the late-lenience window, so we should skip trying to begin the hold during that time.
            // Note: Unlike below, we use the tail's start time to determine the time offset.
            if (Time.Current > Tail.HitObject.StartTime && !Tail.HitObject.HitWindows.CanBeHit(Time.Current - Tail.HitObject.StartTime))
                return false;

            beginHoldAt(Time.Current - Head.HitObject.StartTime);
            Head.UpdateResult();

            return true;
        }

        private void beginHoldAt(double timeOffset)
        {
            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return;

            HoldStartTime = Time.Current;
            isHitting.Value = true;
        }

        public void OnReleased(ManiaAction action)
        {
            if (AllJudged)
                return;

            if (action != Action.Value)
                return;

            // Make sure a hold was started
            if (HoldStartTime == null)
                return;

            Tail.UpdateResult();
            endHold();

            // If the key has been released too early, the user should not receive full score for the release
            if (!Tail.IsHit)
                HasBroken = true;

            hasReleased = true;
        }

        private void endHold()
        {
            HoldStartTime = null;
            isHitting.Value = false;
        }
    }
}
