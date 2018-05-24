// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Track;

namespace osu.Game.Audio
{
    public class PreviewTrack
    {
        public readonly Track Track;
        private readonly Action<PreviewTrack> onStart;
        private readonly Action onStop;

        public event Action Stopped;
        public event Action Started;

        public PreviewTrack(Track track, Action<PreviewTrack> onStart, Action onStop)
        {
            Track = track;
            this.onStart = onStart;
            this.onStop = onStop;
        }

        public void Start()
        {
            onStart?.Invoke(this);
            Track.Restart();
            Started?.Invoke();
        }

        public void Stop()
        {
            onStop?.Invoke();
            Track.Stop();
            Stopped?.Invoke();
        }
    }
}
