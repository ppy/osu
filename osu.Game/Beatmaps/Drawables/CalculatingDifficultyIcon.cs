#nullable enable
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// A difficulty icon which automatically calculates difficulty in the background.
    /// </summary>
    public class CalculatingDifficultyIcon : CompositeDrawable
    {
        /// <summary>
        /// Size of this difficulty icon.
        /// </summary>
        public new Vector2 Size
        {
            get => difficultyIcon.Size;
            set => difficultyIcon.Size = value;
        }

        private readonly IRulesetInfo? ruleset;

        private readonly IReadOnlyList<Mod>? mods;

        private readonly IBeatmapInfo beatmapInfo;

        private readonly DifficultyIcon difficultyIcon;

        /// <summary>
        /// Creates a new <see cref="CalculatingDifficultyIcon"/> with a given <see cref="RulesetInfo"/> and <see cref="Mod"/> combination.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to show the difficulty of.</param>
        /// <param name="ruleset">The ruleset to show the difficulty with.</param>
        /// <param name="mods">The mods to show the difficulty with.</param>
        /// <param name="shouldShowTooltip">Whether to display a tooltip when hovered.</param>
        /// <param name="performBackgroundDifficultyLookup">Whether to perform difficulty lookup (including calculation if necessary).</param>
        public CalculatingDifficultyIcon(IBeatmapInfo beatmapInfo, IRulesetInfo? ruleset, IReadOnlyList<Mod>? mods, bool shouldShowTooltip = true,
                                         bool performBackgroundDifficultyLookup = true)
            : this(beatmapInfo, shouldShowTooltip, performBackgroundDifficultyLookup)
        {
            this.ruleset = ruleset ?? beatmapInfo.Ruleset;
            this.mods = mods ?? Array.Empty<Mod>();
        }

        /// <summary>
        /// Creates a new <see cref="CalculatingDifficultyIcon"/> that follows the currently-selected ruleset and mods.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to show the difficulty of.</param>
        /// <param name="shouldShowTooltip">Whether to display a tooltip when hovered.</param>
        /// <param name="performBackgroundDifficultyLookup">Whether to perform difficulty lookup (including calculation if necessary).</param>
        public CalculatingDifficultyIcon(IBeatmapInfo beatmapInfo, bool shouldShowTooltip = true, bool performBackgroundDifficultyLookup = true)
        {
            this.beatmapInfo = beatmapInfo ?? throw new ArgumentNullException(nameof(beatmapInfo));

            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                difficultyIcon = new DifficultyIcon(beatmapInfo, beatmapInfo.Ruleset),
                new DelayedLoadUnloadWrapper(createDifficultyRetriever, 0)
            };
        }

        private Drawable createDifficultyRetriever()
        {
            if (ruleset != null && mods != null)
                return new DifficultyRetriever(beatmapInfo, ruleset, mods) { StarDifficulty = { BindTarget = difficultyIcon.Current } };

            return new DifficultyRetriever(beatmapInfo) { StarDifficulty = { BindTarget = difficultyIcon.Current } };
        }
    }
}
