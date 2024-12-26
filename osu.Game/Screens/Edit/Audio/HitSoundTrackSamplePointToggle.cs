// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackSamplePointToggleButton : ClickableContainer
    {
        public IBindable<bool>? Active;

        private readonly Circle circle;

        public new IBindable<Colour4> Colour = new Bindable<Colour4>();

        public HitSoundTrackSamplePointToggleButton()
        {
            RelativeSizeAxes = Axes.Both;
            Child = circle = new Circle
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Masking = true,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Colour.BindValueChanged(v => updateColour(), true);
            Active?.BindValueChanged(v => updateColour(), true);
        }

        private void updateColour()
        {
            circle.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Active?.Value == true ? Colour.Value.Darken(0.2f) : Colour4.Black.Opacity(0.2f),
                Radius = Active?.Value == true ? 3f : 2f,
                Hollow = true,
            };
            circle.FadeColour(Active?.Value == true ? Colour.Value : Colour.Value.Darken(0.8f));
            circle.FadeTo(Active?.Value == true ? 1f : 0.5f);
        }

        protected override bool OnHover(HoverEvent e)
        {
            circle.ResizeHeightTo(1.5f, 50);
            base.OnHover(e);
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            circle.ResizeHeightTo(1, 50);
        }
    }

    public partial class HitSoundTrackSamplePointToggle : Container
    {
        private IList<HitSampleInfo> samples = null!;
        private HitObject hitObject = null!;

        public readonly string Target;
        private readonly Bindable<bool> active = new Bindable<bool>(false);

        public Action<string>? Action;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private HitSoundTrackSamplePointBlueprintContainer samplePointsContainer { get; set; } = null!;

        [Resolved]
        private HitSoundTrackSamplePointBlueprint samplePoint { get; set; } = null!;

        public HitSoundTrackSamplePointToggle(string target)
        {
            Target = target;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            RelativeSizeAxes = Axes.X;

            Height = 15;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Child = new HitSoundTrackSamplePointToggleButton
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Action = Toggle,
                Active = active,
                Colour = samplePoint.Colour,
            };

            hitObject = samplePoint.HitObject;

            editorBeatmap.HitObjectUpdated += updatedHitObject =>
            {
                if (updatedHitObject == hitObject)
                    UpdateActiveStateAndSample();
            };

            hitObject.SamplesBindable.BindCollectionChanged((obj, e) => UpdateActiveStateAndSample(), true);
        }

        protected void UpdateActiveStateAndSample()
        {
            if (hitObject is IHasRepeats repeatedHitObject && samplePoint is NodeHitSoundTrackSamplePointBlueprint nodeSamplePoint)
                samples = nodeSamplePoint.NodeIndex < repeatedHitObject.NodeSamples.Count ? repeatedHitObject.NodeSamples[nodeSamplePoint.NodeIndex] : hitObject.Samples;
            else
                samples = hitObject.Samples;

            switch (samplePointsContainer.Mode)
            {
                case HitSoundTrackMode.Sample:
                    active.Value = samples.FirstOrDefault(sample => sample.Name == Target) != null;
                    break;

                case HitSoundTrackMode.NormalBank:
                    active.Value = SamplePointPiece.GetBankValue(samples) == Target;
                    break;

                case HitSoundTrackMode.AdditionBank:
                    active.Value = SamplePointPiece.GetAdditionBankValue(samples) == Target;
                    break;
            }
        }

        protected void Toggle()
        {
            editorBeatmap.BeginChange();

            switch (samplePointsContainer.Mode)
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
                var targetSample = samples.FirstOrDefault(sample => sample.Name == Target);
                if (targetSample == null)
                    return;

                samples.Remove(targetSample);
            }
            else
            {
                var elapsedSample = samples.FirstOrDefault();
                samples.Add(new HitSampleInfo(Target, bank: SamplePointPiece.GetBankValue(samples) ?? HitSampleInfo.BANK_NORMAL, volume: elapsedSample?.Volume ?? 100));
            }
        }

        private void setNormalBank(IList<HitSampleInfo> samples)
        {
            if (active.Value)
                return;

            var originalNormalBank = samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL);
            if (originalNormalBank == null)
                return;

            samples.Add(originalNormalBank.With(newBank: Target));
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
                        samples.Add(originalSample.With(newBank: Target, newEditorAutoBank: false));

                    samples.Remove(originalSample);
                });
            }

            Scheduler.Update();
        }
    }
}
