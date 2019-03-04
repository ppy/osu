// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
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
        public Bindable<string> ChatChannel { get; } = new Bindable<string>();
        public BindableInt Score1 { get; } = new BindableInt();
        public BindableInt Score2 { get; } = new BindableInt();
    }
}
