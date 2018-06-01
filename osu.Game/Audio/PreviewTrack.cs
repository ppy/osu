// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Audio
{
    public class PreviewTrack
    {
        public readonly Track Track;
        public readonly OverlayContainer Owner;

        private readonly Action<PreviewTrack> onStart;
        private readonly Action onStop;

        public event Action Stopped;
        public event Action Started;

        public PreviewTrack(Track track, Action<PreviewTrack> onStart, Action onStop, OverlayContainer owner)
        {
            Track = track;
            this.onStart = onStart;
            this.onStop = onStop;
            Owner = owner;
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
