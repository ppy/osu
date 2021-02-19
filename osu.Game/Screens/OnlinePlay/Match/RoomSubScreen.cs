// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Match
{
    [Cached(typeof(IPreviewTrackOwner))]
    public abstract class RoomSubScreen : OnlinePlaySubScreen, IPreviewTrackOwner
    {
        [Cached(typeof(IBindable<PlaylistItem>))]
        protected readonly Bindable<PlaylistItem> SelectedItem = new Bindable<PlaylistItem>();

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        private readonly ModSelectOverlay userModsSelectOverlay;

        /// <summary>
        /// A container that provides controls for selection of user mods.
        /// This will be shown/hidden automatically when applicable.
        /// </summary>
        protected Drawable UserModsSection;

        private Sample sampleStart;

        /// <summary>
        /// Any mods applied by/to the local user.
        /// </summary>
        protected readonly Bindable<IReadOnlyList<Mod>> UserMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        [Resolved]
        private MusicController music { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved(canBeNull: true)]
        protected OnlinePlayScreen ParentScreen { get; private set; }

        private IBindable<WeakReference<BeatmapSetInfo>> managerUpdated;

        [Cached]
        protected OnlinePlayBeatmapAvailablilityTracker BeatmapAvailablilityTracker { get; }

        protected IBindable<BeatmapAvailability> BeatmapAvailability => BeatmapAvailablilityTracker.Availability;

        protected RoomSubScreen()
        {
            AddRangeInternal(new Drawable[]
            {
                BeatmapAvailablilityTracker = new OnlinePlayBeatmapAvailablilityTracker
                {
                    SelectedItem = { BindTarget = SelectedItem }
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Depth = float.MinValue,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.5f,
                    Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING },
                    Child = userModsSelectOverlay = new UserModSelectOverlay
                    {
                        SelectedMods = { BindTarget = UserMods },
                        IsValidMod = _ => false
                    }
                },
            });
        }

        protected override void ClearInternal(bool disposeChildren = true) =>
            throw new InvalidOperationException($"{nameof(RoomSubScreen)}'s children should not be cleared as it will remove required components");

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleStart = audio.Samples.Get(@"SongSelect/confirm-selection");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItem.BindValueChanged(_ => Scheduler.AddOnce(selectedItemChanged));

            managerUpdated = beatmapManager.ItemUpdated.GetBoundCopy();
            managerUpdated.BindValueChanged(beatmapUpdated);

            UserMods.BindValueChanged(_ => Scheduler.AddOnce(UpdateMods));
        }

        public override bool OnBackButton()
        {
            if (userModsSelectOverlay.State.Value == Visibility.Visible)
            {
                userModsSelectOverlay.Hide();
                return true;
            }

            return base.OnBackButton();
        }

        protected void ShowUserModSelect() => userModsSelectOverlay.Show();

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);
            beginHandlingTrack();
        }

        public override void OnSuspending(IScreen next)
        {
            endHandlingTrack();
            base.OnSuspending(next);
        }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);
            beginHandlingTrack();
            Scheduler.AddOnce(UpdateMods);
        }

        public override bool OnExiting(IScreen next)
        {
            RoomManager?.PartRoom();
            Mods.Value = Array.Empty<Mod>();

            endHandlingTrack();

            return base.OnExiting(next);
        }

        protected void StartPlay(Func<Player> player)
        {
            sampleStart?.Play();
            ParentScreen?.Push(new PlayerLoader(player));
        }

        private void selectedItemChanged()
        {
            updateWorkingBeatmap();

            var selected = SelectedItem.Value;

            if (selected == null)
                return;

            // Remove any user mods that are no longer allowed.
            UserMods.Value = UserMods.Value
                                     .Where(m => selected.AllowedMods.Any(a => m.GetType() == a.GetType()))
                                     .ToList();

            UpdateMods();

            Ruleset.Value = selected.Ruleset.Value;

            if (!selected.AllowedMods.Any())
            {
                UserModsSection?.Hide();
                userModsSelectOverlay.Hide();
                userModsSelectOverlay.IsValidMod = _ => false;
            }
            else
            {
                UserModsSection?.Show();
                userModsSelectOverlay.IsValidMod = m => selected.AllowedMods.Any(a => a.GetType() == m.GetType());
            }
        }

        private void beatmapUpdated(ValueChangedEvent<WeakReference<BeatmapSetInfo>> weakSet) => Schedule(updateWorkingBeatmap);

        private void updateWorkingBeatmap()
        {
            var beatmap = SelectedItem.Value?.Beatmap.Value;

            // Retrieve the corresponding local beatmap, since we can't directly use the playlist's beatmap info
            var localBeatmap = beatmap == null ? null : beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == beatmap.OnlineBeatmapID);

            Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
        }

        protected virtual void UpdateMods()
        {
            if (SelectedItem.Value == null)
                return;

            Mods.Value = UserMods.Value.Concat(SelectedItem.Value.RequiredMods).ToList();
        }

        private void beginHandlingTrack()
        {
            Beatmap.BindValueChanged(applyLoopingToTrack, true);
        }

        private void endHandlingTrack()
        {
            Beatmap.ValueChanged -= applyLoopingToTrack;
            cancelTrackLooping();
        }

        private void applyLoopingToTrack(ValueChangedEvent<WorkingBeatmap> _ = null)
        {
            if (!this.IsCurrentScreen())
                return;

            var track = Beatmap.Value?.Track;

            if (track != null)
            {
                Beatmap.Value.PrepareTrackForPreviewLooping();
                music?.EnsurePlayingSomething();
            }
        }

        private void cancelTrackLooping()
        {
            var track = Beatmap?.Value?.Track;

            if (track != null)
                track.Looping = false;
        }

        private class UserModSelectOverlay : LocalPlayerModSelectOverlay
        {
        }
    }
}
