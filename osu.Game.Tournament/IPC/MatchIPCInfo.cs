// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;

namespace osu.Game.Tournament.IPC
{
    public class MatchIPCInfo : Component
    {
        public Bindable<BeatmapInfo> Beatmap { get; } = new Bindable<BeatmapInfo>();
        public Bindable<LegacyMods> Mods { get; } = new Bindable<LegacyMods>();
        public Bindable<TourneyState> State { get; } = new Bindable<TourneyState>();
        public BindableInt Score1 { get; } = new BindableInt();
        public BindableInt Score2 { get; } = new BindableInt();
    }
}
