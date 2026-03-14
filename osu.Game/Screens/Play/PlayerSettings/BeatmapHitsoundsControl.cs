// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

            HitsoundsSetting hitsoundsState = beatmap.Value.BeatmapInfo.UserSettings.Hitsounds;

            if (hitsoundsState == HitsoundsSetting.UseGlobalSetting)
            {
                Current.Value = globalHitsounds.Value;
                isFollowingGlobal = true;
            }
            else
            {
                Current.Value = hitsoundsState == HitsoundsSetting.HitsoundsOn;
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
                var beatmapInfo = r.Find<BeatmapInfo>(beatmap.Value.BeatmapInfo.ID);

                if (beatmapInfo == null) // only the case for tests.
                    return;

                beatmapInfo.UserSettings.Hitsounds = Current.Value ? HitsoundsSetting.HitsoundsOn : HitsoundsSetting.HitsoundsOff;
                // once value is changed (to On/Off) it will never return to global (same as osu!stable)
            });
        }
    }
}
