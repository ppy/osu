// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using osu.Game.Skinning;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class HitSoundTrackObjectToggleButton : Container
    {
        private Bindable<Color4> colour = new();

        private Circle circle;

        public new Color4 Colour
        {
            get => colour.Value;
            set
            {
                base.Colour = value;
                colour.Value = value;
            }
        }

        public Action Action { get; set; }

        public HitSoundTrackObjectToggleButton()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Child = new ClickableContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = circle = new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 15,
                    Height = 15,
                },
                Action = () =>
                {
                    Action?.Invoke();
                }
            };

            colour.BindValueChanged(v =>
            {
                circle.Masking = true;
                circle.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = v.NewValue.Darken(0.2f),
                    Radius = 10f,
                };
                circle.Colour = v.NewValue;
            }, true);
        }
    }

    public partial class HitSoundTrackObjectToggle : Container
    {
        private IList<HitSampleInfo> samples = null!;
        private HitObject hitObject = null!;

        private string target;

        private HitSoundTrackObjectToggleButton button;
        private Bindable<bool> active = new();

        public Action<string>? Action;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private SoundTrackObjectsDisplay soundTrackObjectsDisplay { get; set; } = null!;

        [Resolved]
        private HitSoundTrackPart hitSoundTrackPart { get; set; } = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public HitSoundTrackObjectToggle(string target)
        {
            this.target = target;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Child = button = new HitSoundTrackObjectToggleButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 30,
                Height = 30,
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
                updateColour();
            };

            if (hitObject is IHasDisplayColour displayColour)
                displayColour.DisplayColour.GetBoundCopy().BindValueChanged(_ => updateColour(), true);

            hitObject.SamplesBindable.BindCollectionChanged((object? obj, NotifyCollectionChangedEventArgs e) => UpdateActiveStateAndSample(), true);
            updateColour();
        }

        private void updateColour()
        {
            Color4 colour;

            switch (hitObject)
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

            button.Colour = colour;
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
