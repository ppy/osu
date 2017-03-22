// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Textures;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Modes.Mods;
using System;
using System.Collections.Generic;

namespace osu.Game.Beatmaps
{
    public abstract class WorkingBeatmap : IDisposable
    {
        public readonly BeatmapInfo BeatmapInfo;

        public readonly BeatmapSetInfo BeatmapSetInfo;

        /// <summary>
        /// A play mode that is preferred for this beatmap. PlayMode will become this mode where conversion is feasible,
        /// or otherwise to the beatmap's default.
        /// </summary>
        public PlayMode? PreferredPlayMode;

        public PlayMode PlayMode => Beatmap?.BeatmapInfo?.Mode > PlayMode.Osu ? Beatmap.BeatmapInfo.Mode : PreferredPlayMode ?? PlayMode.Osu;

        public readonly Bindable<IEnumerable<Mod>> Mods = new Bindable<IEnumerable<Mod>>();

        public readonly bool WithStoryboard;

        protected WorkingBeatmap(BeatmapInfo beatmapInfo, BeatmapSetInfo beatmapSetInfo, bool withStoryboard = false)
        {
            BeatmapInfo = beatmapInfo;
            BeatmapSetInfo = beatmapSetInfo;
            WithStoryboard = withStoryboard;
        }
        
        public abstract Beatmap Beatmap { get; }
        public abstract Texture Background { get; }
        public abstract Track Track { get; }

        public abstract void TransferTo(WorkingBeatmap other);
        public abstract void Dispose();

        public virtual bool TrackLoaded => Track != null;
    }
}
