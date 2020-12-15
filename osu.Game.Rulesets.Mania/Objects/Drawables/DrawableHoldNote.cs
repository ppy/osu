// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

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
        /// Contains the size of the hold note covering the whole head/tail bounds. The size of this container changes as the hold note is being pressed.
        /// </summary>
        private readonly Container sizingContainer;

        /// <summary>
        /// Contains the contents of the hold note that should be masked as the hold note is being pressed. Follows changes in the size of <see cref="sizingContainer"/>.
        /// </summary>
        private readonly Container maskingContainer;

        private readonly SkinnableDrawable bodyPiece;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        /// <summary>
        /// Time at which the hold note has been broken, i.e. released too early, resulting in a reduced score.
        /// </summary>
        public double? HoldBrokenTime { get; private set; }

        /// <summary>
        /// Whether the hold note has been released potentially without having caused a break.
        /// </summary>
        private double? releaseTime;

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;

            Container maskedContents;

            AddRangeInternal(new Drawable[]
            {
                sizingContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        maskingContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = maskedContents = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                            }
                        },
                        headContainer = new Container<DrawableHoldNoteHead> { RelativeSizeAxes = Axes.Both }
                    }
                },
                bodyPiece = new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.HoldNoteBody, hitObject.Column), _ => new DefaultBodyPiece
                {
                    RelativeSizeAxes = Axes.Both,
                })
                {
                    RelativeSizeAxes = Axes.X
                },
                tickContainer = new Container<DrawableHoldNoteTick> { RelativeSizeAxes = Axes.Both },
                tailContainer = new Container<DrawableHoldNoteTail> { RelativeSizeAxes = Axes.Both },
            });

            maskedContents.AddRange(new[]
            {
                bodyPiece.CreateProxy(),
                tickContainer.CreateProxy(),
                tailContainer.CreateProxy(),
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

            if (e.NewValue == ScrollingDirection.Up)
            {
                bodyPiece.Anchor = bodyPiece.Origin = Anchor.TopLeft;
                sizingContainer.Anchor = sizingContainer.Origin = Anchor.BottomLeft;
            }
            else
            {
                bodyPiece.Anchor = bodyPiece.Origin = Anchor.BottomLeft;
                sizingContainer.Anchor = sizingContainer.Origin = Anchor.TopLeft;
            }
        }

        public override void PlaySamples()
        {
            // Samples are played by the head/tail notes.
        }

        public override void OnKilled()
        {
            base.OnKilled();
            (bodyPiece.Drawable as IHoldNoteBody)?.Recycle();
        }

        protected override void Update()
        {
            base.Update();

            if (Time.Current < releaseTime)
                releaseTime = null;

            // Pad the full size container so its contents (i.e. the masking container) reach under the tail.
            // This is required for the tail to not be masked away, since it lies outside the bounds of the hold note.
            sizingContainer.Padding = new MarginPadding
            {
                Top = Direction.Value == ScrollingDirection.Down ? -Tail.Height : 0,
                Bottom = Direction.Value == ScrollingDirection.Up ? -Tail.Height : 0,
            };

            // Pad the masking container to the starting position of the body piece (half-way under the head).
            // This is required to make the body start getting masked immediately as soon as the note is held.
            maskingContainer.Padding = new MarginPadding
            {
                Top = Direction.Value == ScrollingDirection.Up ? Head.Height / 2 : 0,
                Bottom = Direction.Value == ScrollingDirection.Down ? Head.Height / 2 : 0,
            };

            // Position and resize the body to lie half-way under the head and the tail notes.
            bodyPiece.Y = (Direction.Value == ScrollingDirection.Up ? 1 : -1) * Head.Height / 2;
            bodyPiece.Height = DrawHeight - Head.Height / 2 + Tail.Height / 2;

            // As the note is being held, adjust the size of the sizing container. This has two effects:
            // 1. The contained masking container will mask the body and ticks.
            // 2. The head note will move along with the new "head position" in the container.
            if (Head.IsHit && releaseTime == null)
            {
                // How far past the hit target this hold note is. Always a positive value.
                float yOffset = Math.Max(0, Direction.Value == ScrollingDirection.Up ? -Y : Y);
                sizingContainer.Height = Math.Clamp(1 - yOffset / DrawHeight, 0, 1);
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Tail.AllJudged)
            {
                ApplyResult(r => r.Type = r.Judgement.MaxResult);
                endHold();
            }

            if (Tail.Judged && !Tail.IsHit)
                HoldBrokenTime = Time.Current;
        }

        public bool OnPressed(ManiaAction action)
        {
            if (AllJudged)
                return false;

            if (action != Action.Value)
                return false;

            // do not run any of this logic when rewinding, as it inverts order of presses/releases.
            if (Time.Elapsed < 0)
                return false;

            if (CheckHittable?.Invoke(this, Time.Current) == false)
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

            // do not run any of this logic when rewinding, as it inverts order of presses/releases.
            if (Time.Elapsed < 0)
                return;

            // Make sure a hold was started
            if (HoldStartTime == null)
                return;

            Tail.UpdateResult();
            endHold();

            // If the key has been released too early, the user should not receive full score for the release
            if (!Tail.IsHit)
                HoldBrokenTime = Time.Current;

            releaseTime = Time.Current;
        }

        private void endHold()
        {
            HoldStartTime = null;
            isHitting.Value = false;
        }
    }
}
