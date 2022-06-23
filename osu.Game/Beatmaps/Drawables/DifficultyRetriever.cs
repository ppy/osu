// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Beatmaps.Drawables
{
    /// <summary>
    /// A component solely responsible for calculating difficulty in the background.
    /// Intended for use with <see cref="DelayedLoadWrapper"/> to only run processing when usage is on-screen.
    /// </summary>
    public class DifficultyRetriever : Component
    {
        /// <summary>
        /// The bindable star difficulty.
        /// </summary>
        public IBindable<StarDifficulty> StarDifficulty => starDifficulty;

        private readonly Bindable<StarDifficulty> starDifficulty = new Bindable<StarDifficulty>();

        private readonly IBeatmapInfo beatmapInfo;

        private readonly IRulesetInfo? ruleset;
        private readonly IReadOnlyList<Mod>? mods;

        private readonly CancellationTokenSource difficultyCancellation = new CancellationTokenSource();

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        /// <summary>
        /// Construct a difficulty retriever that tracks the current ruleset / mod selection.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to use for calculation.</param>
        public DifficultyRetriever(IBeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;
        }

        /// <summary>
        /// Construct a difficulty retriever that is calculated only once for the specified ruleset / mod combination.
        /// This will not track global ruleset and mod changes.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to use for calculation.</param>
        /// <param name="ruleset">The ruleset to use for calculation.</param>
        /// <param name="mods">The mods to use for calculation.</param>
        public DifficultyRetriever(IBeatmapInfo beatmapInfo, IRulesetInfo ruleset, IReadOnlyList<Mod> mods)
        {
            this.beatmapInfo = beatmapInfo;
            this.ruleset = ruleset;
            this.mods = mods;
        }

        private IBindable<StarDifficulty?> localStarDifficulty = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            localStarDifficulty = ruleset != null
                ? difficultyCache.GetBindableDifficulty(beatmapInfo, ruleset, mods, difficultyCancellation.Token)
                : difficultyCache.GetBindableDifficulty(beatmapInfo, difficultyCancellation.Token);

            localStarDifficulty.BindValueChanged(d =>
            {
                if (d.NewValue is StarDifficulty diff)
                    starDifficulty.Value = diff;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            difficultyCancellation.Cancel();
        }
    }
}
