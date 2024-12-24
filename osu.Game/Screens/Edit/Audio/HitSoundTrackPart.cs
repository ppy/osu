// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

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
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        private Bindable<bool> active = new Bindable<bool>();
        private string target;
        private HitObject hitObject;
        private HitSoundTrackMode mode;

        private IconButton button;

        public HitSoundTrackObjectToggle(HitObject hitObject, HitSoundTrackMode mode, string target)
        {
            this.target = target;
            this.hitObject = hitObject;
            this.mode = mode;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            hitObject.SamplesBindable.BindCollectionChanged((object? obj, NotifyCollectionChangedEventArgs e) =>
            {
                switch (mode)
                {
                    case HitSoundTrackMode.Sample:
                        updateSampleActiveState();
                        break;
                    case HitSoundTrackMode.NormalBank:
                    case HitSoundTrackMode.AdditionBank:
                        updateBankActiveState();
                        break;
                }
            }, true);

            Child = button = new IconButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Icon = OsuIcon.FilledCircle,
                Enabled = { Value = true },
                Action = () =>
                {
                    switch (mode)
                    {
                        case HitSoundTrackMode.Sample:
                            toggleSample();
                            break;
                        case HitSoundTrackMode.NormalBank:
                        case HitSoundTrackMode.AdditionBank:
                            toggleBank();
                            break;
                    }
                }
            };

            active.BindValueChanged(v =>
            {
                button.FadeTo(v.NewValue ? 1f : 0.1f, 100);
            }, true);
        }

        private void toggleSample()
        {
            if (active.Value)
                hitObject.SamplesBindable.Remove(hitObject.Samples.FirstOrDefault(s => s.Name == target));
            else
                hitObject.SamplesBindable.Add(new HitSampleInfo(target, hitObject.Samples.FirstOrDefault(s => s.Name != HitSampleInfo.HIT_NORMAL)?.Bank ?? HitSampleInfo.BANK_NORMAL));

            editorBeatmap.UpdateAllHitObjects();
        }

        private void toggleBank()
        {
            switch (mode)
            {
                case HitSoundTrackMode.NormalBank:
                    // If it's already active, then it can't be turn off by manual
                    if (active.Value)
                        return;
                    var originalNormalBank = hitObject.Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL);
                    if (originalNormalBank == null)
                        return;
                    hitObject.SamplesBindable.Add(originalNormalBank.With(newBank: target));
                    hitObject.SamplesBindable.Remove(originalNormalBank);
                    break;
                case HitSoundTrackMode.AdditionBank:
                    foreach (var originSample in hitObject.Samples.Where(s => s.Name != HitSampleInfo.HIT_NORMAL))
                    {
                        Scheduler.Add(delegate ()
                        {
                            Logger.Log($"{originSample.Name} {originSample.Bank}");
                            if (active.Value)
                                hitObject.SamplesBindable.Add(originSample?.With(newEditorAutoBank: true));
                            else
                                hitObject.SamplesBindable.Add(originSample?.With(newBank: target, newEditorAutoBank: false));
                            hitObject.SamplesBindable.Remove(originSample);
                        });
                    }
                    Scheduler.Update();
                    break;
            };

            editorBeatmap.UpdateAllHitObjects();
        }

        private void updateBankActiveState()
        {
            var sample = hitObject.Samples.Where(s => s.Bank == target);
            if (sample != null)
            {
                switch (mode)
                {
                    case HitSoundTrackMode.NormalBank:
                        active.Value = sample.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)?.Bank == target;
                        break;
                    case HitSoundTrackMode.AdditionBank:
                        var additionSample = sample.FirstOrDefault(s => s.Name != HitSampleInfo.HIT_NORMAL);
                        active.Value = additionSample?.Bank == target && additionSample?.EditorAutoBank == false;
                        break;
                };
            }
            else
                active.Value = false;
        }

        private void updateSampleActiveState()
        {
            if (hitObject.Samples.FirstOrDefault(s => s.Name == target) != null)
                active.Value = true;
            else
                active.Value = false;
        }
    }

    public partial class HitSoundTrackPart : FillFlowContainer, IHasHitObjectAndParent
    {
        public HitObject HitObject { get; set; }
        public HitObject? HitObjectParent { get; set; } = null;

        public HitSoundTrackMode Mode;

        public HitSoundTrackPart(HitObject hitObject, HitSoundTrackMode mode)
        {
            HitObject = hitObject;
            Mode = mode;

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
                X = (float)v.NewValue;
            }, true);

            HitObjectParent?.StartTimeBindable.BindValueChanged(v =>
            {
                X = (float)HitObject.StartTime;
            }, true);

            Children = new[]
            {
                new HitSoundTrackObjectToggle(HitObject, Mode, Mode == HitSoundTrackMode.Sample? HitSampleInfo.HIT_WHISTLE : HitSampleInfo.BANK_NORMAL),
                new HitSoundTrackObjectToggle(HitObject, Mode, Mode == HitSoundTrackMode.Sample? HitSampleInfo.HIT_FINISH : HitSampleInfo.BANK_SOFT),
                new HitSoundTrackObjectToggle(HitObject, Mode, Mode == HitSoundTrackMode.Sample? HitSampleInfo.HIT_CLAP : HitSampleInfo.BANK_DRUM)
            };
        }
    }

    public partial class SoundTrackObjectsDisplay : TimelinePart<HitSoundTrackPart>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        private HitSoundTrackMode mode;
        public SoundTrackObjectsDisplay(HitSoundTrackMode mode)
        {
            this.mode = mode;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            editorBeatmap.HitObjectRemoved += removeHitObjectFromTrack;
            editorBeatmap.HitObjectAdded += (HitObject hitObject) =>
            {
                addHitObjectToTrack(hitObject);
            };
            editorBeatmap.HitObjectUpdated += (HitObject hitObject) =>
            {
                if (hitObject.NestedHitObjects.Count < 1)
                {
                    removeHitObjectFromTrack(hitObject);
                    addHitObjectToTrack(hitObject);
                }
            };

            List<HitSoundTrackPart> objects = [];

            editorBeatmap.HitObjects.ForEach(hitObject =>
            {
                addHitObjectToTrack(hitObject);
            });

            AddRange(objects);
        }

        private void removeHitObjectFromTrack(HitObject hitObject)
        {
            if (hitObject.NestedHitObjects.Count < 1)
            {
                objectFromHitObject<HitSoundTrackPart>(hitObject).ForEach(d => d.Expire());
                return;
            }

            foreach (HitObject nestedHitObject in hitObject.NestedHitObjects)
            {
                objectFromHitObject<HitSoundTrackPart>(hitObject).ForEach(d => d.Expire());
            }
        }

        private void addHitObjectToTrack(HitObject hitObject, HitObject? parent = null)
        {
            if (hitObject.NestedHitObjects.Count < 1)
            {
                Add(new HitSoundTrackPart(hitObject, mode) { HitObjectParent = parent });
                return;
            }

            foreach (HitObject nestedHitObject in hitObject.NestedHitObjects)
            {
                addHitObjectToTrack(nestedHitObject, hitObject);
            }
        }

        private IEnumerable<Drawable> objectFromHitObject<T>(HitObject hitObject) where T : Drawable, IHasHitObjectAndParent
        {
            return Children.Where(soundObject =>
            {
                if (soundObject is T hitSoundTrackObject)
                {
                    return hitSoundTrackObject.HitObject == hitObject || hitSoundTrackObject.HitObjectParent == hitObject;
                }
                return false;
            });
        }
    }
}
