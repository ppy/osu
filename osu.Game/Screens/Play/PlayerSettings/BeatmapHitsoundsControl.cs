// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class BeatmapHitsoundsControl : PlayerCheckbox
    {

        private Bindable<bool> globalHitsounds = new Bindable<bool>();


        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? beatmapHitsoundsSubscription;
        private Task? realmWriteTask;
        private bool isFollowingGlobal { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            globalHitsounds = config.GetBindable<bool>(OsuSetting.BeatmapHitsounds);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var info = realm.Realm.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID)
                ?? beatmap.Value.BeatmapInfo; // use in-memory beatmap for tests
            int val = info.UserSettings.Hitsounds;

            if (val == 0) // 0 == global
            {
                Current.Value = globalHitsounds.Value;
                isFollowingGlobal = true;
            }
            else
            {
                Current.Value = val == 1; // 1 == On; 2 == Off
                isFollowingGlobal = false;
            }


            Current.Disabled = false;
            ShowsDefaultIndicator = false;
            globalHitsounds.BindValueChanged(onGlobalChanged);
            Current.BindValueChanged(onCurrentChanged);

        }

        private void onGlobalChanged(ValueChangedEvent<bool> hitsounds)
        {
            if (isFollowingGlobal) // set current based on global hitsound setting, in case the settings overlay is open when loading beatmap and changing the global setting
                Current.Value = globalHitsounds.Value;

        }



        private void onCurrentChanged(ValueChangedEvent<bool> hitsounds)
        {
            if (!isFollowingGlobal)
                writeHitsoundsToBeatmap();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            isFollowingGlobal = false;
            return base.OnMouseDown(e);
        }

        private void writeHitsoundsToBeatmap()
        {
            // ensure the previous write has completed. ignoring performance concerns, if we don't do this, the async writes could be out of sequence.
            if (realmWriteTask?.IsCompleted == false)
            {
                Scheduler.AddOnce(writeHitsoundsToBeatmap);
                return;
            }

            realmWriteTask = realm.WriteAsync(r =>
            {
                var setInfo = r.Find<BeatmapSetInfo>(beatmap.Value.BeatmapSetInfo.ID);

                if (setInfo == null) // only the case for tests.
                {
                    beatmap.Value.BeatmapInfo.UserSettings.Hitsounds = Current.Value ? 1 : 2;
                    return;
                }

                // Apply to all difficulties in a beatmap set if they have the same audio
                // (they generally always share hitsounds).
                foreach (var b in setInfo.Beatmaps)
                {
                    BeatmapUserSettings userSettings = b.UserSettings;
                    int val = Current.Value ? 1 : 2; // 1 == On; 2 == Off

                    if (userSettings.Hitsounds != val && b.AudioEquals(beatmap.Value.BeatmapInfo))
                    {
                        userSettings.Hitsounds = val; // once value is changed (to On/Off) it will never return to global (same as osu!stable)
                    }
                }
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            beatmapHitsoundsSubscription?.Dispose();
        }
    }
}
