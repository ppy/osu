// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects
{
    public abstract class OsuHitObject : HitObject, IHasComboInformation, IHasPosition
    {
        /// <summary>
        /// The radius of hit objects (ie. the radius of a <see cref="HitCircle"/>).
        /// </summary>
        public const float OBJECT_RADIUS = 64;

        /// <summary>
        /// The width and height any element participating in display of a hitcircle (or similarly sized object) should be.
        /// </summary>
        public static readonly Vector2 OBJECT_DIMENSIONS = new Vector2(OBJECT_RADIUS * 2);

        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second (ie. the speed slider balls move through their track).
        /// </summary>
        internal const float BASE_SCORING_DISTANCE = 100;

        /// <summary>
        /// Minimum preempt time at AR=10.
        /// </summary>
        public const double PREEMPT_MIN = 450;

        /// <summary>
        /// Median preempt time at AR=5.
        /// </summary>
        public const double PREEMPT_MID = 1200;

        /// <summary>
        /// Maximum preempt time at AR=0.
        /// </summary>
        public const double PREEMPT_MAX = 1800;

        public double TimePreempt = 600;
        public double TimeFadeIn = 400;

        private HitObjectProperty<Vector2> position;

        public Bindable<Vector2> PositionBindable => position.Bindable;

        public virtual Vector2 Position
        {
            get => position.Value;
            set => position.Value = value;
        }

        public float X => Position.X;
        public float Y => Position.Y;

        public Vector2 StackedPosition => Position + StackOffset;

        public virtual Vector2 EndPosition => Position;

        public Vector2 StackedEndPosition => EndPosition + StackOffset;

        private HitObjectProperty<int> stackHeight;

        public Bindable<int> StackHeightBindable => stackHeight.Bindable;

        public int StackHeight
        {
            get => stackHeight.Value;
            set => stackHeight.Value = value;
        }

        public virtual Vector2 StackOffset => new Vector2(StackHeight * Scale * -6.4f);

        private const int assumed_height = 681;

        public double Radius => OBJECT_RADIUS * Scale;

        private HitObjectProperty<float> scale = new HitObjectProperty<float>(1);

        public Bindable<float> ScaleBindable => scale.Bindable;

        public float Scale
        {
            get => scale.Value;
            set => scale.Value = value;
        }

        public virtual bool NewCombo { get; set; }

        private HitObjectProperty<int> comboOffset;

        public Bindable<int> ComboOffsetBindable => comboOffset.Bindable;

        public int ComboOffset
        {
            get => comboOffset.Value;
            set => comboOffset.Value = value;
        }

        private HitObjectProperty<int> indexInCurrentCombo;

        public Bindable<int> IndexInCurrentComboBindable => indexInCurrentCombo.Bindable;

        public virtual int IndexInCurrentCombo
        {
            get => indexInCurrentCombo.Value;
            set => indexInCurrentCombo.Value = value;
        }

        private HitObjectProperty<int> comboIndex;

        public Bindable<int> ComboIndexBindable => comboIndex.Bindable;

        public virtual int ComboIndex
        {
            get => comboIndex.Value;
            set => comboIndex.Value = value;
        }

        private HitObjectProperty<int> comboIndexWithOffsets;

        public Bindable<int> ComboIndexWithOffsetsBindable => comboIndexWithOffsets.Bindable;

        public int ComboIndexWithOffsets
        {
            get => comboIndexWithOffsets.Value;
            set => comboIndexWithOffsets.Value = value;
        }

        private HitObjectProperty<bool> lastInCombo;

        public Bindable<bool> LastInComboBindable => lastInCombo.Bindable;

        public bool LastInCombo
        {
            get => lastInCombo.Value;
            set => lastInCombo.Value = value;
        }

        protected OsuHitObject()
        {
            StackHeightBindable.BindValueChanged(height =>
            {
                foreach (var nested in NestedHitObjects.OfType<OsuHitObject>())
                    nested.StackHeight = height.NewValue;
            });
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, PREEMPT_MAX, PREEMPT_MID, PREEMPT_MIN);

            // Preempt time can go below 450ms. Normally, this is achieved via the DT mod which uniformly speeds up all animations game wide regardless of AR.
            // This uniform speedup is hard to match 1:1, however we can at least make AR>10 (via mods) feel good by extending the upper linear function above.
            // Note that this doesn't exactly match the AR>10 visuals as they're classically known, but it feels good.
            // This adjustment is necessary for AR>10, otherwise TimePreempt can become smaller leading to hitcircles not fully fading in.
            TimeFadeIn = 400 * Math.Min(1, TimePreempt / PREEMPT_MIN);

            // Circle size scale works differently in droid.
            Scale = (assumed_height / 480f) * ((54.42f - difficulty.CircleSize * 4.48f) * 2f) /
                    128 + (0.5f * (11 - 5.2450170716245195f)) / 5f;

            Scale = (float)Math.Max(1e-3, Scale) / (assumed_height * 0.85f / 384);
        }

        public void UpdateComboInformation(IHasComboInformation? lastObj)
        {
            // Note that this implementation is shared with the osu!catch ruleset's implementation.
            // If a change is made here, CatchHitObject.cs should also be updated.
            ComboIndex = lastObj?.ComboIndex ?? 0;
            ComboIndexWithOffsets = lastObj?.ComboIndexWithOffsets ?? 0;
            IndexInCurrentCombo = (lastObj?.IndexInCurrentCombo + 1) ?? 0;

            if (this is Spinner)
            {
                // For the purpose of combo colours, spinners never start a new combo even if they are flagged as doing so.
                return;
            }

            // At decode time, the first hitobject in the beatmap and the first hitobject after a spinner are both enforced to be a new combo,
            // but this isn't directly enforced by the editor so the extra checks against the last hitobject are duplicated here.
            if (NewCombo || lastObj == null || lastObj is Spinner)
            {
                IndexInCurrentCombo = 0;
                ComboIndex++;
                ComboIndexWithOffsets += ComboOffset + 1;

                if (lastObj != null)
                    lastObj.LastInCombo = true;
            }
        }

        protected override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
