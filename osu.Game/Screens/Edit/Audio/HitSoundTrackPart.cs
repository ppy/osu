// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Audio
{
    public interface IHasHitObjectAndParent
    {
        HitObject HitObject { get; set; }
        HitObject? HitObjectParent { get; set; }
    };

    public enum HitSoundTrackMode
    {
        Sample,
        NormalBank,
        AdditionBank,
    }

    public partial class HitSoundTrackObjectToggle : Container
    {
        private IconButton button;
        private BindableList<HitSampleInfo> bindableSamples = null!;
        private IList<HitSampleInfo> samples = null!;
        private string target;
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
            bindableSamples = hitSoundTrackPart.HitObject.SamplesBindable;

            bindableSamples.BindCollectionChanged((object? obj, NotifyCollectionChangedEventArgs e) =>
            {
                samples = hitSoundTrackPart.HitObject.Samples;

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
            }, true);
        }

        protected void Toggle()
        {
            editorBeatmap.BeginChange();

            switch (soundTrackObjectsDisplay.Mode)
            {
                case HitSoundTrackMode.Sample:
                    if (active.Value)
                        bindableSamples.RemoveAll(sample => sample.Name == target);
                    else
                        bindableSamples.Add(new HitSampleInfo(target));
                    break;
                case HitSoundTrackMode.NormalBank:
                    // If it's already active, then it can't be turn off by manual
                    if (active.Value)
                        return;
                    var originalNormalBank = samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL);
                    if (originalNormalBank == null)
                        return;
                    bindableSamples.Add(originalNormalBank.With(newBank: target));
                    bindableSamples.Remove(originalNormalBank);
                    break;
                case HitSoundTrackMode.AdditionBank:

                    foreach (var originalSample in samples.Where(s => s.Name != HitSampleInfo.HIT_NORMAL))
                    {
                        Scheduler.Add(() =>
                        {
                            if (active.Value)
                                bindableSamples.Add(originalSample.With(newEditorAutoBank: true));
                            else
                                bindableSamples.Add(originalSample.With(newBank: target, newEditorAutoBank: false));
                            bindableSamples.Remove(originalSample);
                        });
                    }

                    Scheduler.Update();
                    break;
            }

            editorBeatmap.Update(hitSoundTrackPart.HitObject);
            editorBeatmap.EndChange();
        }
    }


    [Cached]
    public partial class HitSoundTrackPart : FillFlowContainer
    {
        public HitObject HitObject;

        [Resolved]
        private SoundTrackObjectsDisplay soundTrackObjectsDisplay { get; set; } = null!;

        public HitSoundTrackPart(HitObject hitObject)
        {
            HitObject = hitObject;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;
            Direction = FillDirection.Vertical;
            Origin = Anchor.TopCentre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            HitObject.StartTimeBindable.BindValueChanged(v =>
            {
                X = (float)GetStartTime();
            }, true);

            string buildTarget(string sample, string bank) => soundTrackObjectsDisplay.Mode == HitSoundTrackMode.Sample ? sample : bank;

            Children = new[]
            {
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_NORMAL)),
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_FINISH, HitSampleInfo.BANK_SOFT)),
                new HitSoundTrackObjectToggle(buildTarget(HitSampleInfo.HIT_CLAP, HitSampleInfo.BANK_DRUM)),
            };
        }

        protected virtual double GetStartTime()
        {
            return HitObject.StartTime;
        }
    }

    public partial class NodeHitSoundTrackPart : HitSoundTrackPart
    {
        public IHasRepeats HasRepeat;
        public int NodeIndex;

        public NodeHitSoundTrackPart(HitObject hitObject, IHasRepeats hasRepeat, int nodeIndex) : base(hitObject)
        {
            HasRepeat = hasRepeat;
            NodeIndex = nodeIndex;
        }

        protected override double GetStartTime()
        {
            return HitObject.StartTime + HasRepeat.Duration * NodeIndex / HasRepeat.SpanCount();
        }
    }

    [Cached]
    public partial class SoundTrackObjectsDisplay : TimelinePart<HitSoundTrackPart>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public readonly HitSoundTrackMode Mode;

        public SoundTrackObjectsDisplay(HitSoundTrackMode mode)
        {
            Mode = mode;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            editorBeatmap.HitObjectRemoved += removeHitObjectFromTrack;
            editorBeatmap.HitObjectAdded += addHitObjectToTrack;
            editorBeatmap.HitObjectUpdated += (HitObject hitObject) =>
            {
                if (hitObject.NestedHitObjects.Count < 1)
                {
                    removeHitObjectFromTrack(hitObject);
                    addHitObjectToTrack(hitObject);
                }
            };

            List<HitSoundTrackPart> objects = [];

            editorBeatmap.HitObjects.ForEach(addHitObjectToTrack);

            AddRange(objects);
        }

        private void removeHitObjectFromTrack(HitObject hitObject)
        {
            Children.Where(v =>
            {
                if (v is HitSoundTrackPart hitSoundTrackPart)
                    return hitSoundTrackPart.HitObject == hitObject;
                return false;
            }).ForEach(part => part.Expire());
        }

        private void addHitObjectToTrack(HitObject hitObject)
        {
            if (hitObject is IHasRepeats repeatedHitObject)
            {
                for (int i = 0; i < repeatedHitObject.NodeSamples.Count; i++)
                {
                    Add(new NodeHitSoundTrackPart(hitObject, repeatedHitObject, i));
                }
            }
            else
            {
                Add(new HitSoundTrackPart(hitObject));
            }
        }
    }
}
