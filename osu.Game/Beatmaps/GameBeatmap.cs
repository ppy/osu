// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using osu.Framework.Audio;
using osu.Framework.Configuration;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// A <see cref="Bindable{WorkingBeatmap}"/> for the <see cref="OsuGame"/> beatmap.
    /// This should be used sparingly in-favour of <see cref="IGameBeatmap"/>.
    /// </summary>
    public class GameBeatmap : NonNullableBindable<WorkingBeatmap>, IGameBeatmap
    {
        private readonly AudioManager audioManager;

        private WorkingBeatmap lastBeatmap;

        public GameBeatmap(WorkingBeatmap defaultValue, AudioManager audioManager)
            : base(defaultValue)
        {
            this.audioManager = audioManager;

            ValueChanged += registerAudioTrack;
        }

        private void registerAudioTrack(WorkingBeatmap beatmap)
        {
            var trackLoaded = lastBeatmap?.TrackLoaded ?? false;

            // compare to last beatmap as sometimes the two may share a track representation (optimisation, see WorkingBeatmap.TransferTo)
            if (!trackLoaded || lastBeatmap?.Track != beatmap.Track)
            {
                if (trackLoaded)
                {
                    Debug.Assert(lastBeatmap != null);
                    Debug.Assert(lastBeatmap.Track != null);

                    lastBeatmap.RecycleTrack();
                }

                audioManager.Track.AddItem(beatmap.Track);
            }

            lastBeatmap = beatmap;
        }

        public GameBeatmap GetBoundCopy()
        {
            var copy = new GameBeatmap(Default, audioManager);
            copy.BindTo(this);
            return copy;
        }
    }
}
