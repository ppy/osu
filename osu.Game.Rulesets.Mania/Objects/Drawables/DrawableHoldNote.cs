// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Rulesets.Mania.Skinning;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="HoldNote"/> hit object.
    /// </summary>
    public partial class DrawableHoldNote : DrawableManiaHitObject<HoldNote>, IKeyBindingHandler<ManiaAction>
    {
        public override bool DisplayResult => false;

        public IBindable<bool> IsHitting => isHitting;

        private IBindable<Anchor> tailOrigin = new Bindable<Anchor>(Anchor.BottomCentre);
        private readonly Bindable<bool> isHitting = new Bindable<bool>();

        public DrawableHoldNoteHead Head => headContainer.Child;
        public DrawableHoldNoteTail Tail => tailContainer.Child;
        public DrawableHoldNoteBody Body => bodyContainer.Child;

        private Container<DrawableHoldNoteHead> headContainer;
        private Container<DrawableHoldNoteTail> tailContainer;
        private Container<DrawableHoldNoteBody> bodyContainer;

        private PausableSkinnableSound slidingSample;

        /// <summary>
        /// Contains the size of the hold note covering the whole head/tail bounds. The size of this container changes as the hold note is being pressed.
        /// </summary>
        private Container sizingContainer;

        /// <summary>
        /// Contains the contents of the hold note that should be masked as the hold note is being pressed. Follows changes in the size of <see cref="sizingContainer"/>.
        /// </summary>
        private Container maskingContainer;

        private SkinnableDrawable bodyPiece;

        /// <summary>
        /// Time at which the user started holding this hold note. Null if the user is not holding this hold note.
        /// </summary>
        public double? HoldStartTime { get; private set; }

        /// <summary>
        /// Used to decide whether to visually clamp the hold note to the judgement line.
        /// </summary>
        private double? releaseTime;

        public DrawableHoldNote()
            : this(null)
        {
        }

        public DrawableHoldNote(HoldNote hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                bodyContainer = new Container<DrawableHoldNoteBody> { RelativeSizeAxes = Axes.Both },
                bodyPiece = new SkinnableDrawable(new ManiaSkinComponentLookup(ManiaSkinComponents.HoldNoteBody), _ => new DefaultBodyPiece
                {
                    RelativeSizeAxes = Axes.Both,
                })
                {
                    RelativeSizeAxes = Axes.X
                },
                tailContainer = new Container<DrawableHoldNoteTail> { RelativeSizeAxes = Axes.Both },
                slidingSample = new PausableSkinnableSound
                {
                    Looping = true,
                    MinimumSampleVolume = MINIMUM_SAMPLE_VOLUME,
                }
            });

            maskedContents.AddRange(new[]
            {
                bodyPiece.CreateProxy(),
                tailContainer.CreateProxy(),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isHitting.BindValueChanged(updateSlidingSample, true);
        }

        protected override void OnApply()
        {
            base.OnApply();

            sizingContainer.Size = Vector2.One;
            HoldStartTime = null;
            releaseTime = null;
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

                case DrawableHoldNoteBody body:
                    bodyContainer.Child = body;
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            headContainer.Clear(false);
            tailContainer.Clear(false);
            bodyContainer.Clear(false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case TailNote tail:
                    return new DrawableHoldNoteTail(tail);

                case HeadNote head:
                    return new DrawableHoldNoteHead(head);

                case HoldNoteBody body:
                    return new DrawableHoldNoteBody(body);
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

            if (Time.Current < HoldStartTime)
                endHold();

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
            // The rationale for this is account for heads/tails with corner radius.
            bodyPiece.Y = (Direction.Value == ScrollingDirection.Up ? 1 : -1) * Head.Height / 2;
            bodyPiece.Height = DrawHeight - Head.Height / 2;
            if (tailOrigin.Value == Anchor.TopCentre)
                bodyPiece.Height -= Tail.Height / 2;
            else
                bodyPiece.Height += Tail.Height / 2;

            // Update the anchor of the tail piece, taking into account the scrolling direction
            if (Direction.Value == ScrollingDirection.Up)
                Tail.Origin = tailOrigin.Value == Anchor.TopCentre ? Anchor.BottomCentre : Anchor.TopCentre;
            else
                Tail.Origin = tailOrigin.Value;

            if (Time.Current >= HitObject.StartTime)
            {
                // As the note is being held, adjust the size of the sizing container. This has two effects:
                // 1. The contained masking container will mask the body and ticks.
                // 2. The head note will move along with the new "head position" in the container.
                //
                // As per stable, this should not apply for early hits, waiting until the object starts to touch the
                // judgement area first.
                if (Head.IsHit && releaseTime == null && DrawHeight > 0)
                {
                    // How far past the hit target this hold note is.
                    float yOffset = Direction.Value == ScrollingDirection.Up ? -Y : Y;
                    sizingContainer.Height = 1 - yOffset / DrawHeight;
                }
            }
            else
                sizingContainer.Height = 1;
        }

        protected override void ApplySkin(ISkinSource skin, bool allowFallback)
        {
            base.ApplySkin(skin, allowFallback);
            var newTailOrigin = skin.GetConfig<ManiaSkinConfigurationLookup, Anchor>(
                new ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.HoldNoteTailOrigin)
            );
            if (newTailOrigin != null)
                tailOrigin = newTailOrigin;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Tail.AllJudged)
            {
                if (Tail.IsHit)
                    ApplyMaxResult();
                else
                    MissForcefully();
            }

            // Make sure that the hold note is fully judged by giving the body a judgement.
            if (Tail.AllJudged && !Body.AllJudged)
                Body.TriggerResult(Tail.IsHit);
        }

        public override void MissForcefully()
        {
            base.MissForcefully();

            // Important that this is always called when a result is applied.
            endHold();
        }

        public bool OnPressed(KeyBindingPressEvent<ManiaAction> e)
        {
            if (AllJudged)
                return false;

            if (e.Action != Action.Value)
                return false;

            // do not run any of this logic when rewinding, as it inverts order of presses/releases.
            if ((Clock as IGameplayClock)?.IsRewinding == true)
                return false;

            if (CheckHittable?.Invoke(this, Time.Current) == false)
                return false;

            // The tail has a lenience applied to it which is factored into the miss window (i.e. the miss judgement will be delayed).
            // But the hold cannot ever be started within the late-lenience window, so we should skip trying to begin the hold during that time.
            // Note: Unlike below, we use the tail's start time to determine the time offset.
            if (Time.Current > Tail.HitObject.StartTime && !Tail.HitObject.HitWindows.CanBeHit(Time.Current - Tail.HitObject.StartTime))
                return false;

            beginHoldAt(Time.Current - Head.HitObject.StartTime);

            return Head.UpdateResult();
        }

        private void beginHoldAt(double timeOffset)
        {
            if (timeOffset < -Head.HitObject.HitWindows.WindowFor(HitResult.Miss))
                return;

            HoldStartTime = Time.Current;
            isHitting.Value = true;
        }

        public void OnReleased(KeyBindingReleaseEvent<ManiaAction> e)
        {
            if (AllJudged)
                return;

            if (e.Action != Action.Value)
                return;

            // do not run any of this logic when rewinding, as it inverts order of presses/releases.
            if ((Clock as IGameplayClock)?.IsRewinding == true)
                return;

            // When our action is released and we are in the middle of a hold, there's a chance that
            // the user has released too early (before the tail).
            //
            // In such a case, we want to record this against the DrawableHoldNoteBody.
            if (HoldStartTime != null)
            {
                Tail.UpdateResult();
                Body.TriggerResult(Tail.IsHit);

                endHold();
                releaseTime = Time.Current;
            }
        }

        private void endHold()
        {
            HoldStartTime = null;
            isHitting.Value = false;
        }

        protected override void LoadSamples()
        {
            // Note: base.LoadSamples() isn't called since the slider plays the tail's hitsounds for the time being.

            slidingSample.Samples = HitObject.CreateSlidingSamples().Cast<ISampleInfo>().ToArray();
        }

        public override void StopAllSamples()
        {
            base.StopAllSamples();
            slidingSample?.Stop();
        }

        private void updateSlidingSample(ValueChangedEvent<bool> tracking)
        {
            if (tracking.NewValue)
                slidingSample?.Play();
            else
                slidingSample?.Stop();
        }

        protected override void OnFree()
        {
            slidingSample.ClearSamples();
            base.OnFree();
        }
    }
}
