// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Encapsulates a <see cref="WorkingBeatmap"/> while providing an overridden <see cref="Beatmap{TObject}"/>.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    public class EditorWorkingBeatmap<TObject> : IWorkingBeatmap
        where TObject : HitObject
    {
        private readonly Beatmap<TObject> playableBeatmap;
        private readonly WorkingBeatmap workingBeatmap;

        public EditorWorkingBeatmap(Beatmap<TObject> playableBeatmap, WorkingBeatmap workingBeatmap)
        {
            this.playableBeatmap = playableBeatmap;
            this.workingBeatmap = workingBeatmap;
        }

        public IBeatmap Beatmap => workingBeatmap.Beatmap;

        public Texture Background => workingBeatmap.Background;

        public VideoSprite Video => workingBeatmap.Video;

        public Track Track => workingBeatmap.Track;

        public Waveform Waveform => workingBeatmap.Waveform;

        public Storyboard Storyboard => workingBeatmap.Storyboard;

        public ISkin Skin => workingBeatmap.Skin;

        public IBeatmap GetPlayableBeatmap(RulesetInfo ruleset, IReadOnlyList<Mod> mods) => playableBeatmap;
    }
}
