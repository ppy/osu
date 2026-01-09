// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Audio.Effects;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;
using osu.Game.Seasonal;

namespace osu.Game.Overlays
{
    /// <summary>
    /// Handles playback of the global music track.
    /// </summary>
    public partial class MusicController : CompositeDrawable
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        /// <summary>
        /// Point in time after which the current track will be restarted on triggering a "previous track" action.
        /// </summary>
        private const double restart_cutoff_point = 5000;

        /// <summary>
        /// Whether the user has requested the track to be paused. Use <see cref="IsPlaying"/> to determine whether the track is still playing.
        /// </summary>
        public bool UserPauseRequested { get; private set; }

        /// <summary>
        /// Whether user control of the global track should be allowed.
        /// </summary>
        public readonly BindableBool AllowTrackControl = new BindableBool(true);

        public readonly BindableBool Shuffle = new BindableBool(true);

        /// <summary>
        /// Fired when the global <see cref="WorkingBeatmap"/> has changed.
        /// Includes direction information for display purposes.
        /// </summary>
        public event Action<WorkingBeatmap, TrackChangeDirection>? TrackChanged;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        public DrawableTrack CurrentTrack { get; private set; } = new DrawableTrack(new TrackVirtual(1000));

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IBindable<double> sampleVolume = null!;

        private readonly BindableDouble audioDuckVolume = new BindableDouble(1);

        private AudioFilter audioDuckFilter = null!;

        private readonly Bindable<RandomSelectAlgorithm> randomSelectAlgorithm = new Bindable<RandomSelectAlgorithm>();

        private readonly LinkedList<Live<BeatmapSetInfo>> randomHistory = new LinkedList<Live<BeatmapSetInfo>>();
        private LinkedListNode<Live<BeatmapSetInfo>>? currentRandomHistoryPosition;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager configManager)
        {
            AddInternal(audioDuckFilter = new AudioFilter(audio.TrackMixer));
            audio.Tracks.AddAdjustment(AdjustableProperty.Volume, audioDuckVolume);
            sampleVolume = audio.Samples.AggregateVolume.GetBoundCopy();

            configManager.BindWith(OsuSetting.RandomSelectAlgorithm, randomSelectAlgorithm);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(b =>
            {
                if (b.NewValue != null)
                    changeBeatmap(b.NewValue);
            }, true);
            mods.BindValueChanged(_ => ResetTrackAdjustments(), true);
        }

        /// <summary>
        /// Forcefully reload the current <see cref="WorkingBeatmap"/>'s track from disk.
        /// </summary>
        public void ReloadCurrentTrack()
        {
            if (current == null)
                return;

            changeTrack();
            TrackChanged?.Invoke(current, TrackChangeDirection.None);
        }

        /// <summary>
        /// Returns whether the beatmap track is playing.
        /// </summary>
        public bool IsPlaying => CurrentTrack.IsRunning;

        /// <summary>
        /// Returns whether the beatmap track is loaded.
        /// </summary>
        public bool TrackLoaded => CurrentTrack.TrackLoaded;

        private ScheduledDelegate? seekDelegate;

        public void SeekTo(double position)
        {
            seekDelegate?.Cancel();
            seekDelegate = Schedule(() =>
            {
                if (!AllowTrackControl.Value)
                    return;

                CurrentTrack.Seek(position);
            });
        }

        /// <summary>
        /// Ensures music is playing, no matter what, unless the user has explicitly paused.
        /// This means that if the current beatmap has a virtual track (see <see cref="TrackVirtual"/>) a new beatmap will be selected.
        /// </summary>
        public void EnsurePlayingSomething()
        {
            if (UserPauseRequested) return;

            if (CurrentTrack.IsDummyDevice || beatmap.Value.BeatmapSetInfo.DeletePending)
            {
                if (beatmap.Disabled || !AllowTrackControl.Value)
                    return;

                Logger.Log($"{nameof(MusicController)} skipping next track to {nameof(EnsurePlayingSomething)}");
                NextTrack(allowProtectedTracks: true);
            }
            else if (!IsPlaying)
            {
                Logger.Log($"{nameof(MusicController)} starting playback to {nameof(EnsurePlayingSomething)}");
                Play();
            }
        }

        /// <summary>
        /// Start playing the current track (if not already playing).
        /// </summary>
        /// <param name="restart">Whether to restart the track from the beginning.</param>
        /// <param name="requestedByUser">
        /// Whether the request to play was issued by the user rather than internally.
        /// Specifying <c>true</c> will ensure that other methods like <see cref="EnsurePlayingSomething"/>
        /// will resume music playback going forward.
        /// </param>
        /// <returns>Whether the operation was successful.</returns>
        public bool Play(bool restart = false, bool requestedByUser = false)
        {
            if (requestedByUser && !AllowTrackControl.Value)
                return false;

            if (requestedByUser)
                UserPauseRequested = false;

            if (restart)
                CurrentTrack.RestartAsync();
            else if (!IsPlaying)
                CurrentTrack.StartAsync();

            return true;
        }

        /// <summary>
        /// Stop playing the current track and pause at the current position.
        /// </summary>
        /// <param name="requestedByUser">
        /// Whether the request to stop was issued by the user rather than internally.
        /// Specifying <c>true</c> will ensure that other methods like <see cref="EnsurePlayingSomething"/>
        /// will not resume music playback until the next explicit call to <see cref="Play"/>.
        /// </param>
        public void Stop(bool requestedByUser = false)
        {
            if (requestedByUser && !AllowTrackControl.Value)
                return;

            UserPauseRequested |= requestedByUser;
            if (CurrentTrack.IsRunning)
                CurrentTrack.StopAsync();
        }

        /// <summary>
        /// Toggle pause / play.
        /// </summary>
        /// <returns>Whether the operation was successful.</returns>
        public bool TogglePause()
        {
            if (!AllowTrackControl.Value)
                return false;

            if (CurrentTrack.IsRunning)
                Stop(true);
            else
                Play(requestedByUser: true);

            return true;
        }

        /// <summary>
        /// Play the previous track or restart the current track if it's current time below <see cref="restart_cutoff_point"/>.
        /// </summary>
        /// <param name="onSuccess">Invoked when the operation has been performed successfully.</param>
        /// <param name="allowProtectedTracks">Whether to include <see cref="BeatmapSetInfo.Protected"/> beatmap sets when navigating.</param>
        public void PreviousTrack(Action<PreviousTrackResult>? onSuccess = null, bool allowProtectedTracks = false) => Schedule(() =>
        {
            PreviousTrackResult res = prev(allowProtectedTracks);
            if (res != PreviousTrackResult.None)
                onSuccess?.Invoke(res);
        });

        /// <summary>
        /// Play the previous track or restart the current track if it's current time below <see cref="restart_cutoff_point"/>.
        /// </summary>
        /// <param name="allowProtectedTracks">Whether to include <see cref="BeatmapSetInfo.Protected"/> beatmap sets when navigating.</param>
        /// <returns>The <see cref="PreviousTrackResult"/> that indicate the decided action.</returns>
        private PreviousTrackResult prev(bool allowProtectedTracks)
        {
            if (beatmap.Disabled || !AllowTrackControl.Value)
                return PreviousTrackResult.None;

            double currentTrackPosition = CurrentTrack.CurrentTime;

            if (currentTrackPosition >= restart_cutoff_point)
            {
                SeekTo(0);
                return PreviousTrackResult.Restart;
            }

            queuedDirection = TrackChangeDirection.Prev;

            Live<BeatmapSetInfo>? playableSet;

            if (Shuffle.Value)
                playableSet = getNextRandom(-1, allowProtectedTracks);
            else
            {
                playableSet = getBeatmapSets(allowProtectedTracks).TakeWhile(i => !i.Value.Equals(current?.BeatmapSetInfo)).LastOrDefault()
                              ?? getBeatmapSets(allowProtectedTracks).LastOrDefault();
            }

            if (playableSet != null)
            {
                changeBeatmap(beatmaps.GetWorkingBeatmap(playableSet.Value.Beatmaps.First()));
                restartTrack();
                return PreviousTrackResult.Previous;
            }

            return PreviousTrackResult.None;
        }

        /// <summary>
        /// Play the next random or playlist track.
        /// </summary>
        /// <param name="onSuccess">Invoked when the operation has been performed successfully.</param>
        /// <param name="allowProtectedTracks">Whether to include <see cref="BeatmapSetInfo.Protected"/> beatmap sets when navigating.</param>
        /// <returns>A <see cref="ScheduledDelegate"/> of the operation.</returns>
        public void NextTrack(Action? onSuccess = null, bool allowProtectedTracks = false) => Schedule(() =>
        {
            bool res = next(allowProtectedTracks);
            if (res)
                onSuccess?.Invoke();
        });

        private readonly List<DuckParameters> duckOperations = new List<DuckParameters>();

        /// <summary>
        /// Applies ducking, attenuating the volume and/or low-pass cutoff of the currently playing track to make headroom for effects (or just to apply an effect).
        /// </summary>
        /// <returns>A <see cref="IDisposable"/> which will restore the duck operation when disposed.</returns>
        public IDisposable Duck(DuckParameters? parameters = null)
        {
            // Don't duck if samples have no volume, it sounds weird.
            if (sampleVolume.Value == 0)
                return new InvokeOnDisposal(() => { });

            parameters ??= new DuckParameters();

            duckOperations.Add(parameters);

            DuckParameters volumeOperation = duckOperations.MinBy(p => p.DuckVolumeTo)!;
            DuckParameters lowPassOperation = duckOperations.MinBy(p => p.DuckCutoffTo)!;

            audioDuckFilter.CutoffTo(lowPassOperation.DuckCutoffTo, lowPassOperation.DuckDuration, lowPassOperation.DuckEasing);
            this.TransformBindableTo(audioDuckVolume, volumeOperation.DuckVolumeTo, volumeOperation.DuckDuration, volumeOperation.DuckEasing);

            return new InvokeOnDisposal(restoreDucking);

            void restoreDucking() => Schedule(() =>
            {
                if (!duckOperations.Remove(parameters))
                    return;

                DuckParameters? restoreVolumeOperation = duckOperations.MinBy(p => p.DuckVolumeTo);
                DuckParameters? restoreLowPassOperation = duckOperations.MinBy(p => p.DuckCutoffTo);

                // If another duck operation is in the list, restore ducking to its level, else reset back to defaults.
                audioDuckFilter.CutoffTo(restoreLowPassOperation?.DuckCutoffTo ?? AudioFilter.MAX_LOWPASS_CUTOFF, parameters.RestoreDuration, parameters.RestoreEasing);
                this.TransformBindableTo(audioDuckVolume, restoreVolumeOperation?.DuckVolumeTo ?? 1, parameters.RestoreDuration, parameters.RestoreEasing);
            });
        }

        /// <summary>
        /// A convenience method that ducks the currently playing track, then after a delay, restores automatically.
        /// </summary>
        /// <param name="delayUntilRestore">A delay in milliseconds which defines how long to delay restoration after ducking completes.</param>
        /// <param name="parameters">Parameters defining the ducking operation.</param>
        public void DuckMomentarily(double delayUntilRestore, DuckParameters? parameters = null)
        {
            // Don't duck if samples have no volume, it sounds weird.
            if (sampleVolume.Value == 0)
                return;

            parameters ??= new DuckParameters();

            IDisposable duckOperation = Duck(parameters);

            Scheduler.AddDelayed(() => duckOperation.Dispose(), delayUntilRestore);
        }

        private bool next(bool allowProtectedTracks)
        {
            if (beatmap.Disabled || !AllowTrackControl.Value)
                return false;

            queuedDirection = TrackChangeDirection.Next;

            Live<BeatmapSetInfo>? playableSet;

            if (Shuffle.Value)
                playableSet = getNextRandom(1, allowProtectedTracks);
            else
            {
                playableSet = getBeatmapSets(allowProtectedTracks).SkipWhile(i => !i.Value.Equals(current?.BeatmapSetInfo)).ElementAtOrDefault(1)
                              ?? getBeatmapSets(allowProtectedTracks).FirstOrDefault();
            }

            var playableBeatmap = playableSet?.Value.Beatmaps.FirstOrDefault();

            if (playableBeatmap != null)
            {
                changeBeatmap(beatmaps.GetWorkingBeatmap(playableBeatmap));
                restartTrack();
                return true;
            }

            return false;
        }

        private Live<BeatmapSetInfo>? getNextRandom(int direction, bool allowProtectedTracks)
        {
            Live<BeatmapSetInfo> result;

            var possibleSets = getBeatmapSets(allowProtectedTracks).ToList();

            if (possibleSets.Count == 0)
                return null;

            // if there is only one possible set left, play it, even if it is the same as the current track.
            // looping is preferable over playing nothing.
            if (possibleSets.Count == 1)
                return possibleSets.Single();

            // now that we actually know there is a choice, do not allow the current track to be played again.
            possibleSets.RemoveAll(s => s.Value.Equals(current?.BeatmapSetInfo));

            if (currentRandomHistoryPosition != null)
            {
                if (direction < 0 && currentRandomHistoryPosition.Previous != null)
                {
                    currentRandomHistoryPosition = currentRandomHistoryPosition.Previous;
                    return currentRandomHistoryPosition.Value;
                }

                if (direction > 0 && currentRandomHistoryPosition.Next != null)
                {
                    currentRandomHistoryPosition = currentRandomHistoryPosition.Next;
                    return currentRandomHistoryPosition.Value;
                }
            }

            // if the early-return above didn't cover it, it means that we have no history to fall back on
            // and need to actually choose something random.

            switch (randomSelectAlgorithm.Value)
            {
                case RandomSelectAlgorithm.Random:
                    result = possibleSets[RNG.Next(possibleSets.Count)];
                    break;

                case RandomSelectAlgorithm.RandomPermutation:
                    var notYetPlayedSets = possibleSets.Except(randomHistory).ToList();

                    if (notYetPlayedSets.Count == 0)
                    {
                        possibleSets.RemoveAll(s => s.Value.Equals(current?.BeatmapSetInfo));
                        notYetPlayedSets = possibleSets;
                        randomHistory.Clear();
                    }

                    result = notYetPlayedSets[RNG.Next(notYetPlayedSets.Count)];

                    Debug.Assert(randomHistory.Count == 0
                                 || (currentRandomHistoryPosition == randomHistory.First && direction < 0)
                                 || (currentRandomHistoryPosition == randomHistory.Last && direction > 0));

                    // notably, this depends solely on `direction` specifically, because when there are less than 2 items in `randomHistory`,
                    // we have `randomHistory.First == randomHistory.Last` (either `null` if no items, or the single item).
                    // the assert above should make that safe to depend on.
                    if (direction > 0)
                        currentRandomHistoryPosition = randomHistory.AddLast(result);
                    else if (direction < 0)
                        currentRandomHistoryPosition = randomHistory.AddFirst(result);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(randomSelectAlgorithm), randomSelectAlgorithm.Value, "Unsupported random select algorithm");
            }

            return result;
        }

        private void restartTrack()
        {
            // if not scheduled, the previously track will be stopped one frame later (see ScheduleAfterChildren logic in GameBase).
            // we probably want to move this to a central method for switching to a new working beatmap in the future.
            Schedule(() => CurrentTrack.RestartAsync());
        }

        private WorkingBeatmap? current;

        private TrackChangeDirection? queuedDirection;

        private IEnumerable<Live<BeatmapSetInfo>> getBeatmapSets(bool allowProtectedTracks) =>
            realm.Realm.All<BeatmapSetInfo>().Where(s => !s.DeletePending)
                 .AsEnumerable()
                 .Select(s => new RealmLive<BeatmapSetInfo>(s, realm))
                 .Where(i => (allowProtectedTracks || !i.Value.Protected)
                             && (SeasonalUIConfig.ENABLED || i.Value.Hash != IntroChristmas.CHRISTMAS_BEATMAP_SET_HASH));

        private void changeBeatmap(WorkingBeatmap newWorking)
        {
            // This method can potentially be triggered multiple times as it is eagerly fired in next() / prev() to ensure correct execution order
            // (changeBeatmap must be called before consumers receive the bindable changed event, which is not the case when the local beatmap bindable is updated directly).
            if (newWorking == current)
                return;

            var lastWorking = current;

            TrackChangeDirection direction = TrackChangeDirection.None;

            bool audioEquals = newWorking.BeatmapInfo?.AudioEquals(current?.BeatmapInfo) == true;

            if (current != null)
            {
                if (audioEquals)
                    direction = TrackChangeDirection.None;
                else if (queuedDirection.HasValue)
                {
                    direction = queuedDirection.Value;
                    queuedDirection = null;
                }
                else
                {
                    // figure out the best direction based on order in playlist.
                    int last = getBeatmapSets(allowProtectedTracks: false).TakeWhile(b => !b.Value.Equals(current.BeatmapSetInfo)).Count();
                    int next = getBeatmapSets(allowProtectedTracks: false).TakeWhile(b => !b.Value.Equals(newWorking.BeatmapSetInfo)).Count();

                    direction = last > next ? TrackChangeDirection.Prev : TrackChangeDirection.Next;
                }
            }

            current = newWorking;

            if (lastWorking == null || !lastWorking.TryTransferTrack(current))
                changeTrack();

            TrackChanged?.Invoke(current, direction);

            ResetTrackAdjustments();

            queuedDirection = null;

            // this will be a noop if coming from the beatmapChanged event.
            // the exception is local operations like next/prev, where we want to complete loading the track before sending out a change.
            if (beatmap.Value != current && beatmap is Bindable<WorkingBeatmap> working)
                working.Value = current;
        }

        private void changeTrack()
        {
            var queuedTrack = getQueuedTrack();

            var lastTrack = CurrentTrack;
            lastTrack.Completed -= onTrackCompleted;

            CurrentTrack = queuedTrack;

            // At this point we may potentially be in an async context from tests. This is extremely dangerous but we have to make do for now.
            // CurrentTrack is immediately updated above for situations where a immediate knowledge about the new track is required,
            // but the mutation of the hierarchy is scheduled to avoid exceptions.
            Schedule(() =>
            {
                lastTrack.VolumeTo(0, 500, Easing.Out).Expire();

                if (queuedTrack == CurrentTrack)
                {
                    AddInternal(queuedTrack);
                    queuedTrack.VolumeTo(0).Then().VolumeTo(1, 300, Easing.Out);
                }
                else
                {
                    // If the track has changed since the call to changeTrack, it is safe to dispose the
                    // queued track rather than consume it.
                    queuedTrack.Dispose();
                }
            });
        }

        private DrawableTrack getQueuedTrack()
        {
            // Important to keep this in its own method to avoid inadvertently capturing unnecessary variables in the callback.
            // Can lead to leaks.
            var queuedTrack = new DrawableTrack(current!.LoadTrack());
            queuedTrack.Completed += onTrackCompleted;
            return queuedTrack;
        }

        private void onTrackCompleted()
        {
            if (!CurrentTrack.Looping && !beatmap.Disabled && AllowTrackControl.Value)
                NextTrack(allowProtectedTracks: true);
        }

        private bool applyModTrackAdjustments;

        /// <summary>
        /// Whether mod track adjustments are allowed to be applied.
        /// </summary>
        public bool ApplyModTrackAdjustments
        {
            get => applyModTrackAdjustments;
            set
            {
                if (applyModTrackAdjustments == value)
                    return;

                applyModTrackAdjustments = value;
                ResetTrackAdjustments();
            }
        }

        private AudioAdjustments? modTrackAdjustments;

        /// <summary>
        /// Resets the adjustments currently applied on <see cref="CurrentTrack"/> and applies the mod adjustments if <see cref="ApplyModTrackAdjustments"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Does not reset any adjustments applied directly to the beatmap track.
        /// </remarks>
        public void ResetTrackAdjustments()
        {
            // todo: we probably want a helper method rather than this.
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Balance);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Frequency);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Tempo);
            CurrentTrack.RemoveAllAdjustments(AdjustableProperty.Volume);

            if (applyModTrackAdjustments)
            {
                CurrentTrack.BindAdjustments(modTrackAdjustments = new AudioAdjustments());

                foreach (var mod in mods.Value.OfType<IApplicableToTrack>())
                    mod.ApplyToTrack(modTrackAdjustments);
            }
        }
    }

    public class DuckParameters
    {
        /// <summary>
        /// The duration of the ducking transition in milliseconds.
        /// Defaults to 100 ms.
        /// </summary>
        public double DuckDuration = 100;

        /// <summary>
        /// The final volume which should be reached during ducking, when 0 is silent and 1 is original volume.
        /// Defaults to 25%.
        /// </summary>
        public double DuckVolumeTo = 0.25;

        /// <summary>
        /// The low-pass cutoff frequency which should be reached during ducking. If not required, set to <see cref="AudioFilter.MAX_LOWPASS_CUTOFF"/>.
        /// Defaults to 300 Hz.
        /// </summary>
        public int DuckCutoffTo = 300;

        /// <summary>
        /// The easing curve to be applied during ducking.
        /// Defaults to <see cref="Easing.Out"/>.
        /// </summary>
        public Easing DuckEasing = Easing.Out;

        /// <summary>
        /// The duration of the restoration transition in milliseconds.
        /// Defaults to 500 ms.
        /// </summary>
        public double RestoreDuration = 500;

        /// <summary>
        /// The easing curve to be applied during restoration.
        /// Defaults to <see cref="Easing.In"/>.
        /// </summary>
        public Easing RestoreEasing = Easing.In;
    }

    public enum TrackChangeDirection
    {
        None,
        Next,
        Prev
    }

    public enum PreviousTrackResult
    {
        None,
        Restart,
        Previous
    }
}
