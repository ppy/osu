// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Overlays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Edit.Audio
{
    /// <summary>
    /// Only handles sample, normal bank and addition bank, but not include volume
    /// </summary>
    [Cached]
    public partial class HitSoundTrackSamplePointBlueprint : FillFlowContainer
    {
        protected const float CIRCLE_WIDTH = 15;

        public IList<HitSampleInfo> Samples;

        public HitObject HitObject;

        public new Bindable<Colour4> Colour = new Bindable<Colour4>();

        public event Action? OnSampleChange;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private HitSoundTrackSamplePointBlueprintContainer samplePointsContainer { get; set; } = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public HitSoundTrackSamplePointBlueprint(HitObject hitObject, IList<HitSampleInfo> samples)
        {
            HitObject = hitObject;
            Samples = samples;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Origin = Anchor.TopCentre;

            Spacing = new Vector2(12.5f);
        }

        protected string BuildTarget(string sample, string bank) => samplePointsContainer.Mode == HitSoundTrackMode.Sample ? sample : bank;

        protected virtual IReadOnlyList<Drawable> CreateControls()
        {
            return new[]
            {
                new HitSoundTrackSamplePointToggle(BuildTarget(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_NORMAL)),
                new HitSoundTrackSamplePointToggle(BuildTarget(HitSampleInfo.HIT_FINISH, HitSampleInfo.BANK_SOFT)),
                new HitSoundTrackSamplePointToggle(BuildTarget(HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM)),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Children = CreateControls();

            HitObject.StartTimeBindable.BindValueChanged(v =>
            {
                UpdateWidthAndPosition();
            }, true);

            editorBeatmap.HitObjectUpdated += updatedHitObject =>
            {
                if (updatedHitObject == HitObject)
                    OnSampleChange?.Invoke();
            };

            HitObject.SamplesBindable.BindCollectionChanged((obj, e) => OnSampleChange?.Invoke(), true);

            #region bind colour change

            void updateColour()
            {
                Colour4 colour;

                switch (HitObject)
                {
                    case IHasDisplayColour displayColour:
                        colour = displayColour.DisplayColour.Value;
                        break;

                    case IHasComboInformation combo:
                        colour = combo.GetComboColour(skin);
                        break;

                    default:
                        colour = colourProvider.Highlight1;
                        break;
                }

                Colour.Value = colour;
            }

            if (HitObject is IHasDisplayColour displayColour)
                displayColour.DisplayColour.BindValueChanged(_ => updateColour());

            if (HitObject is IHasComboInformation comboInfo)
            {
                comboInfo.IndexInCurrentComboBindable.BindValueChanged(_ => updateColour());
                comboInfo.ComboIndexBindable.BindValueChanged(_ => updateColour());
                comboInfo.ComboIndexWithOffsetsBindable.BindValueChanged(_ => updateColour());
            }

            skin.SourceChanged += updateColour;

            updateColour();

            #endregion
        }

        #region sample manipulation method

        public void Toggle(string target)
        {
            switch (samplePointsContainer.Mode)
            {
                case HitSoundTrackMode.Sample:
                    toggleSample(target);
                    break;
                case HitSoundTrackMode.NormalBank:
                    setNormalBank(target);
                    break;
                case HitSoundTrackMode.AdditionBank:
                    setAdditionBank(target);
                    break;
            }

            editorBeatmap.Update(HitObject);
        }

        private void toggleSample(string targetSample)
        {
            var existSample = Samples.FirstOrDefault(sample => sample.Name == targetSample);

            if (existSample == null)
                Samples.Add(new HitSampleInfo(
                    targetSample,
                    bank: SamplePointPiece.GetBankValue(Samples) ?? HitSampleInfo.BANK_NORMAL,
                    volume: Samples.FirstOrDefault()?.Volume ?? 100
                ));
            else
                Samples.Remove(existSample);

        }

        private void setNormalBank(string targetBank)
        {
            string? currentBank = SamplePointPiece.GetBankValue(Samples);

            if (currentBank == targetBank)
                return;

            var originalSample = Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL);
            if (originalSample == null)
                return;

            Samples.Add(originalSample.With(newBank: targetBank));
            Samples.Remove(originalSample);
        }

        private void setAdditionBank(string targetAdditionBank)
        {
            foreach (var originalSample in Samples.Where(s => s.Name != HitSampleInfo.HIT_NORMAL))
            {
                Scheduler.Add(() =>
                {
                    if (SamplePointPiece.GetAdditionBankValue(Samples) == targetAdditionBank)
                        Samples.Add(originalSample.With(newEditorAutoBank: true));
                    else
                        Samples.Add(originalSample.With(newBank: targetAdditionBank, newEditorAutoBank: false));

                    Samples.Remove(originalSample);
                });
            }

            Scheduler.Update();
        }

        #endregion

        public bool GetActiveState(string target)
        {
            bool active = false;

            switch (samplePointsContainer.Mode)
            {
                case HitSoundTrackMode.Sample:
                    active = Samples.FirstOrDefault(sample => sample.Name == target) != null;
                    break;

                case HitSoundTrackMode.NormalBank:
                    active = SamplePointPiece.GetBankValue(Samples) == target;
                    break;

                case HitSoundTrackMode.AdditionBank:
                    active = SamplePointPiece.GetAdditionBankValue(Samples) == target;
                    break;
            }

            return active;
        }

        public void UpdateWidthAndPosition()
        {
            Width = (float)GetWidth();
            X = (float)GetStartTime();
        }

        protected virtual double GetStartTime()
        {
            if (HitObject is IHasDuration)
                return HitObject.StartTime + Width / 2 - CIRCLE_WIDTH / 2;

            return HitObject.StartTime;
        }

        protected virtual double GetWidth()
        {
            if (HitObject is IHasDuration duration)
            {
                RelativeSizeAxes = Axes.Both;
                return duration.Duration + CIRCLE_WIDTH;
            }

            return CIRCLE_WIDTH;
        }
    }

    public partial class NodeHitSoundTrackSamplePointBlueprint : HitSoundTrackSamplePointBlueprint
    {
        public IHasRepeats HasRepeat;
        public int NodeIndex;

        public NodeHitSoundTrackSamplePointBlueprint(HitObject hitObject, IList<HitSampleInfo> samples, int nodeIndex)
            : base(hitObject, samples)
        {
            if (hitObject is not IHasRepeats hasRepeat)
                throw new ArgumentException("HitObject must be implement IHasRepeats");

            HasRepeat = hasRepeat;
            NodeIndex = nodeIndex;
        }

        protected override void Update()
        {
            base.Update();

            if (HasRepeat.RepeatCount < NodeIndex - 1)
                Expire();
        }

        protected override double GetStartTime() => HitObject.StartTime + HasRepeat.Duration * NodeIndex / HasRepeat.SpanCount();

        protected override double GetWidth() => CIRCLE_WIDTH;
    }
}
