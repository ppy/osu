// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackObjectToggle : Container
    {
        private IList<HitSampleInfo> samples = null!;
        private HitObject hitObject = null!;

        private string target;

        private IconButton button;
        private Bindable<bool> active = new();

        public Action<string>? Action;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private SoundTrackObjectsDisplay soundTrackObjectsDisplay { get; set; } = null!;

        [Resolved]
        private HitSoundTrackPart hitSoundTrackPart { get; set; } = null!;

        public HitSoundTrackObjectToggle(string target)
        {
            this.target = target;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Child = button = new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = OsuIcon.FilledCircle,
                Enabled = { Value = true },
                Action = Toggle,
            };

            active.BindValueChanged(v =>
            {
                button.Alpha = v.NewValue ? 1f : 0.1f;
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            hitObject = hitSoundTrackPart.HitObject;

            editorBeatmap.HitObjectUpdated += (HitObject updatedHitObject) =>
            {
                if (updatedHitObject == hitObject)
                    UpdateActiveStateAndSample();
            };

            hitObject.SamplesBindable.BindCollectionChanged((object? obj, NotifyCollectionChangedEventArgs e) => UpdateActiveStateAndSample(), true);
        }

        protected void UpdateActiveStateAndSample()
        {
            if (hitObject is IHasRepeats repeatedHitObject && hitSoundTrackPart is NodeHitSoundTrackPart nodeHitSoundTrackPart)
                samples = nodeHitSoundTrackPart.NodeIndex < repeatedHitObject.NodeSamples.Count ? repeatedHitObject.NodeSamples[nodeHitSoundTrackPart.NodeIndex] : hitObject.Samples;
            else
                samples = hitObject.Samples;

            switch (soundTrackObjectsDisplay.Mode)
            {
                case HitSoundTrackMode.Sample:
                    active.Value = samples.FirstOrDefault(sample => sample.Name == target) != null;
                    break;
                case HitSoundTrackMode.NormalBank:
                    active.Value = SamplePointPiece.GetBankValue(samples) == target;
                    break;
                case HitSoundTrackMode.AdditionBank:
                    active.Value = SamplePointPiece.GetAdditionBankValue(samples) == target;
                    break;
            }
        }

        protected void Toggle()
        {
            editorBeatmap.BeginChange();
            switch (soundTrackObjectsDisplay.Mode)
            {
                case HitSoundTrackMode.Sample:
                    setSample(samples);
                    break;
                case HitSoundTrackMode.NormalBank:
                    setNormalBank(samples);
                    break;
                case HitSoundTrackMode.AdditionBank:
                    setAdditionBank(samples);
                    break;
            }
            editorBeatmap.Update(hitObject);
            editorBeatmap.EndChange();
        }

        private void setSample(IList<HitSampleInfo> samples)
        {
            if (active.Value)
            {
                var targetSample = samples.FirstOrDefault(sample => sample.Name == target);
                if (targetSample == null)
                    return;
                samples.Remove(targetSample);
            }
            else
                samples.Add(new HitSampleInfo(target, bank: SamplePointPiece.GetBankValue(samples) ?? HitSampleInfo.BANK_NORMAL));
        }

        private void setNormalBank(IList<HitSampleInfo> samples)
        {
            if (active.Value)
                return;
            var originalNormalBank = samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL);
            if (originalNormalBank == null)
                return;
            samples.Add(originalNormalBank.With(newBank: target));
            samples.Remove(originalNormalBank);
        }

        private void setAdditionBank(IList<HitSampleInfo> samples)
        {
            foreach (var originalSample in samples.Where(s => s.Name != HitSampleInfo.HIT_NORMAL))
            {
                Scheduler.Add(() =>
                {
                    if (active.Value)
                        samples.Add(originalSample.With(newEditorAutoBank: true));
                    else
                        samples.Add(originalSample.With(newBank: target, newEditorAutoBank: false));
                    samples.Remove(originalSample);
                });
            }

            Scheduler.Update();
        }
    }
}
