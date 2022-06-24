// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

        public bool ShowTooltip
        {
            get => difficultyIcon.ShowTooltip;
            set => difficultyIcon.ShowTooltip = value;
        }

        private readonly IBeatmapInfo beatmapInfo;

        private readonly DifficultyIcon difficultyIcon;

        /// <summary>
        /// Creates a new <see cref="CalculatingDifficultyIcon"/> that follows the currently-selected ruleset and mods.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to show the difficulty of.</param>
        public CalculatingDifficultyIcon(IBeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo ?? throw new ArgumentNullException(nameof(beatmapInfo));

            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                difficultyIcon = new DifficultyIcon(beatmapInfo),
                new DelayedLoadUnloadWrapper(createDifficultyRetriever, 0)
            };
        }

        private Drawable createDifficultyRetriever() => new DifficultyRetriever(beatmapInfo) { StarDifficulty = { BindTarget = difficultyIcon.Current } };
    }
}
