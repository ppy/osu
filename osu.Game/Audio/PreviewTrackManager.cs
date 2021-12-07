// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game.Beatmaps;

namespace osu.Game.Audio
{
    public class PreviewTrackManager : Component
    {
        private readonly IAdjustableAudioComponent mainTrackAdjustments;

        private readonly BindableDouble muteBindable = new BindableDouble();

        private ITrackStore trackStore;

        protected TrackManagerPreviewTrack CurrentTrack;

        public PreviewTrackManager(IAdjustableAudioComponent mainTrackAdjustments)
        {
            this.mainTrackAdjustments = mainTrackAdjustments;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audioManager)
        {
            trackStore = audioManager.GetTrackStore(new OnlineStore());
        }

        /// <summary>
        /// Retrieves a <see cref="PreviewTrack"/> for a <see cref="IBeatmapSetInfo"/>.
        /// </summary>
        /// <param name="beatmapSetInfo">The <see cref="IBeatmapSetInfo"/> to retrieve the preview track for.</param>
        /// <returns>The playable <see cref="PreviewTrack"/>.</returns>
        public PreviewTrack Get(IBeatmapSetInfo beatmapSetInfo)
        {
            var track = CreatePreviewTrack(beatmapSetInfo, trackStore);

            track.Started += () => Schedule(() =>
            {
                CurrentTrack?.Stop();
                CurrentTrack = track;
                mainTrackAdjustments.AddAdjustment(AdjustableProperty.Volume, muteBindable);
            });

            track.Stopped += () => Schedule(() =>
            {
                if (CurrentTrack != track)
                    return;

                CurrentTrack = null;
                mainTrackAdjustments.RemoveAdjustment(AdjustableProperty.Volume, muteBindable);
            });

            return track;
        }

        /// <summary>
        /// Stops any currently playing <see cref="PreviewTrack"/>.
        /// </summary>
        /// <remarks>
        /// Only the immediate owner (an object that implements <see cref="IPreviewTrackOwner"/>) of the playing <see cref="PreviewTrack"/>
        /// can globally stop the currently playing <see cref="PreviewTrack"/>. The object holding a reference to the <see cref="PreviewTrack"/>
        /// can always stop the <see cref="PreviewTrack"/> themselves through <see cref="PreviewTrack.Stop()"/>.
        /// </remarks>
        /// <param name="source">The <see cref="IPreviewTrackOwner"/> which may be the owner of the <see cref="PreviewTrack"/>.</param>
        public void StopAnyPlaying(IPreviewTrackOwner source)
        {
            if (CurrentTrack == null || (CurrentTrack.Owner != null && CurrentTrack.Owner != source))
                return;

            CurrentTrack.Stop();
            // CurrentTrack should not be set to null here as it will result in incorrect handling in the track.Stopped callback above.
        }

        /// <summary>
        /// Creates the <see cref="TrackManagerPreviewTrack"/>.
        /// </summary>
        protected virtual TrackManagerPreviewTrack CreatePreviewTrack(IBeatmapSetInfo beatmapSetInfo, ITrackStore trackStore) =>
            new TrackManagerPreviewTrack(beatmapSetInfo, trackStore);

        public class TrackManagerPreviewTrack : PreviewTrack
        {
            [Resolved(canBeNull: true)]
            public IPreviewTrackOwner Owner { get; private set; }

            private readonly IBeatmapSetInfo beatmapSetInfo;
            private readonly ITrackStore trackManager;

            public TrackManagerPreviewTrack(IBeatmapSetInfo beatmapSetInfo, ITrackStore trackManager)
            {
                this.beatmapSetInfo = beatmapSetInfo;
                this.trackManager = trackManager;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                if (Owner == null)
                    Logger.Log($"A {nameof(PreviewTrack)} was created without a containing {nameof(IPreviewTrackOwner)}. An owner should be added for correct behaviour.");
            }

            protected override Track GetTrack() => trackManager.Get($"https://b.ppy.sh/preview/{beatmapSetInfo.OnlineID}.mp3");
        }
    }
}
