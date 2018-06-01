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

        public event Action Stopped;
        public event Action Started;

        public PreviewTrack(Track track, OverlayContainer owner)
        {
            Track = track;
            Owner = owner;
        }

        public void Start()
        {
            Track.Restart();
            Started?.Invoke();
        }

        public void Stop()
        {
            Track.Stop();
            Stopped?.Invoke();
        }
    }
}
