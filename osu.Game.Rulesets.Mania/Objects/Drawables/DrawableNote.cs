// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Skinning.Default;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Objects.Drawables
{
    /// <summary>
    /// Visualises a <see cref="Note"/> hit object.
    /// </summary>
    public class DrawableNote : DrawableManiaHitObject<Note>, IKeyBindingHandler<ManiaAction>
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private Bindable<ManiaColourCode> configColourCode { get; set; }

        [Resolved(canBeNull: true)]
        private ManiaBeatmap beatmap { get; set; }

        protected virtual ManiaSkinComponents Component => ManiaSkinComponents.Note;

        private readonly Drawable headPiece;

        public readonly Bindable<int> SnapBindable = new Bindable<int>();

        public int Snap
        {
            get => SnapBindable.Value;
            set => SnapBindable.Value = value;
        }

        public DrawableNote(Note hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            AddInternal(headPiece = new SkinnableDrawable(new ManiaSkinComponent(Component, hitObject.Column), _ => new DefaultNotePiece())
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            HitObject.StartTimeBindable.BindValueChanged(_ => SnapToBeatmap(), true);

            SnapBindable.BindValueChanged(snap => UpdateSnapColour(configColourCode.Value, snap.NewValue), true);
            configColourCode.BindValueChanged(colourCode => UpdateSnapColour(colourCode.NewValue, Snap));
        }

        protected override void OnDirectionChanged(ValueChangedEvent<ScrollingDirection> e)
        {
            base.OnDirectionChanged(e);

            headPiece.Anchor = headPiece.Origin = e.NewValue == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            ApplyResult(r => r.Type = result);
        }

        public virtual bool OnPressed(ManiaAction action)
        {
            if (action != Action.Value)
                return false;

            if (CheckHittable?.Invoke(this, Time.Current) == false)
                return false;

            return UpdateResult(true);
        }

        public virtual void OnReleased(ManiaAction action)
        {
        }
        private void SnapToBeatmap()
        {
            if (beatmap != null)
            {
                TimingControlPoint currentTimingPoint = beatmap.ControlPointInfo.TimingPointAt(HitObject.StartTime);
                int timeSignature = (int)currentTimingPoint.TimeSignature;
                double startTime = currentTimingPoint.Time;
                double secondsPerFourCounts = currentTimingPoint.BeatLength * 4;

                double offset = startTime % secondsPerFourCounts;
                double snapResult = HitObject.StartTime % secondsPerFourCounts - offset;

                if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 4.0))
                {
                    Snap = 1;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 8.0))
                {
                    Snap = 2;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 12.0))
                {
                    Snap = 3;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 16.0))
                {
                    Snap = 4;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 24.0))
                {
                    Snap = 6;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 32.0))
                {
                    Snap = 8;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 48.0))
                {
                    Snap = 12;
                }
                else if (AlmostDivisibleBy(snapResult, secondsPerFourCounts / 64.0))
                {
                    Snap = 16;
                }
                else
                {
                    Snap = 0;
                }
            }
        }

        private const double LENIENCY_MS = 1.0;
        private static bool AlmostDivisibleBy(double dividend, double divisor)
        {
            double remainder = Math.Abs(dividend) % divisor;
            return Precision.AlmostEquals(remainder, 0, LENIENCY_MS) || Precision.AlmostEquals(remainder - divisor, 0, LENIENCY_MS);
        }

        private void UpdateSnapColour(ManiaColourCode colourCode, int snap)
        {
            if (colourCode == ManiaColourCode.On)
            {
                Colour = BindableBeatDivisor.GetColourFor(Snap, colours);
            }
            else
            {
                Colour = Colour4.White;
            }
        }
    }
}