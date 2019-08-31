// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Beatmaps
{
    public interface IWorkingBeatmap
    {
        /// <summary>
        /// Retrieves the <see cref="IBeatmap"/> which this <see cref="WorkingBeatmap"/> represents.
        /// </summary>
        IBeatmap Beatmap { get; }

        /// <summary>
        /// Retrieves the background for this <see cref="WorkingBeatmap"/>.
        /// </summary>
        Texture Background { get; }

        /// <summary>
        /// Retrieves the video background file for this <see cref="WorkingBeatmap"/>.
        /// </summary>
        VideoSprite Video { get; }

        /// <summary>
        /// Retrieves the audio track for this <see cref="WorkingBeatmap"/>.
        /// </summary>
        Track Track { get; }

        /// <summary>
        /// Retrieves the <see cref="Waveform"/> for the <see cref="Track"/> of this <see cref="WorkingBeatmap"/>.
        /// </summary>
        Waveform Waveform { get; }

        /// <summary>
        /// Retrieves the <see cref="Storyboard"/> which this <see cref="WorkingBeatmap"/> provides.
        /// </summary>
        Storyboard Storyboard { get; }

        /// <summary>
        /// Retrieves the <see cref="Skin"/> which this <see cref="WorkingBeatmap"/> provides.
        /// </summary>
        ISkin Skin { get; }

        /// <summary>
        /// Constructs a playable <see cref="IBeatmap"/> from <see cref="Beatmap"/> using the applicable converters for a specific <see cref="RulesetInfo"/>.
        /// <para>
        /// The returned <see cref="IBeatmap"/> is in a playable state - all <see cref="HitObject"/> and <see cref="BeatmapDifficulty"/> <see cref="Mod"/>s
        /// have been applied, and <see cref="HitObject"/>s have been fully constructed.
        /// </para>
        /// </summary>
        /// <param name="ruleset">The <see cref="RulesetInfo"/> to create a playable <see cref="IBeatmap"/> for.</param>
        /// <param name="mods">The <see cref="Mod"/>s to apply to the <see cref="IBeatmap"/>.</param>
        /// <returns>The converted <see cref="IBeatmap"/>.</returns>
        /// <exception cref="BeatmapInvalidForRulesetException">If <see cref="Beatmap"/> could not be converted to <paramref name="ruleset"/>.</exception>
        IBeatmap GetPlayableBeatmap(RulesetInfo ruleset, IReadOnlyList<Mod> mods);
    }
}
