// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class DifficultyIcon : CompositeDrawable, IHasCustomTooltip<DifficultyIconTooltipContent>
    {
        private readonly Container iconContainer;

        /// <summary>
        /// Size of this difficulty icon.
        /// </summary>
        public new Vector2 Size
        {
            get => iconContainer.Size;
            set => iconContainer.Size = value;
        }

        [NotNull]
        private readonly BeatmapInfo beatmap;

        [CanBeNull]
        private readonly RulesetInfo ruleset;

        [CanBeNull]
        private readonly IReadOnlyList<Mod> mods;

        private readonly bool shouldShowTooltip;

        private readonly bool performBackgroundDifficultyLookup;

        private readonly Bindable<StarDifficulty> difficultyBindable = new Bindable<StarDifficulty>();

        private Drawable background;

        /// <summary>
        /// Creates a new <see cref="DifficultyIcon"/> with a given <see cref="RulesetInfo"/> and <see cref="Mod"/> combination.
        /// </summary>
        /// <param name="beatmap">The beatmap to show the difficulty of.</param>
        /// <param name="ruleset">The ruleset to show the difficulty with.</param>
        /// <param name="mods">The mods to show the difficulty with.</param>
        /// <param name="shouldShowTooltip">Whether to display a tooltip when hovered.</param>
        public DifficultyIcon([NotNull] BeatmapInfo beatmap, [CanBeNull] RulesetInfo ruleset, [CanBeNull] IReadOnlyList<Mod> mods, bool shouldShowTooltip = true)
            : this(beatmap, shouldShowTooltip)
        {
            this.ruleset = ruleset ?? beatmap.Ruleset;
            this.mods = mods ?? Array.Empty<Mod>();
        }

        /// <summary>
        /// Creates a new <see cref="DifficultyIcon"/> that follows the currently-selected ruleset and mods.
        /// </summary>
        /// <param name="beatmap">The beatmap to show the difficulty of.</param>
        /// <param name="shouldShowTooltip">Whether to display a tooltip when hovered.</param>
        /// <param name="performBackgroundDifficultyLookup">Whether to perform difficulty lookup (including calculation if necessary).</param>
        public DifficultyIcon([NotNull] BeatmapInfo beatmap, bool shouldShowTooltip = true, bool performBackgroundDifficultyLookup = true)
        {
            this.beatmap = beatmap ?? throw new ArgumentNullException(nameof(beatmap));
            this.shouldShowTooltip = shouldShowTooltip;
            this.performBackgroundDifficultyLookup = performBackgroundDifficultyLookup;

            AutoSizeAxes = Axes.Both;

            InternalChild = iconContainer = new Container { Size = new Vector2(20f) };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            iconContainer.Children = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.06f),

                        Type = EdgeEffectType.Shadow,
                        Radius = 3,
                    },
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.ForStarDifficulty(beatmap.StarDifficulty) // Default value that will be re-populated once difficulty calculation completes
                    },
                },
                new ConstrainedIconContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    // the null coalesce here is only present to make unit tests work (ruleset dlls aren't copied correctly for testing at the moment)
                    Icon = (ruleset ?? beatmap.Ruleset)?.CreateInstance()?.CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle }
                },
            };

            if (performBackgroundDifficultyLookup)
                iconContainer.Add(new DelayedLoadUnloadWrapper(() => new DifficultyRetriever(beatmap, ruleset, mods) { StarDifficulty = { BindTarget = difficultyBindable } }, 0));
            else
                difficultyBindable.Value = new StarDifficulty(beatmap.StarDifficulty, 0);

            difficultyBindable.BindValueChanged(difficulty => background.Colour = colours.ForStarDifficulty(difficulty.NewValue.Stars));
        }

        ITooltip<DifficultyIconTooltipContent> IHasCustomTooltip<DifficultyIconTooltipContent>.GetCustomTooltip() => new DifficultyIconTooltip();

        DifficultyIconTooltipContent IHasCustomTooltip<DifficultyIconTooltipContent>.TooltipContent => shouldShowTooltip ? new DifficultyIconTooltipContent(beatmap, difficultyBindable) : null;

        private class DifficultyRetriever : Component
        {
            public readonly Bindable<StarDifficulty> StarDifficulty = new Bindable<StarDifficulty>();

            private readonly BeatmapInfo beatmap;
            private readonly RulesetInfo ruleset;
            private readonly IReadOnlyList<Mod> mods;

            private CancellationTokenSource difficultyCancellation;

            [Resolved]
            private BeatmapDifficultyCache difficultyCache { get; set; }

            public DifficultyRetriever(BeatmapInfo beatmap, RulesetInfo ruleset, IReadOnlyList<Mod> mods)
            {
                this.beatmap = beatmap;
                this.ruleset = ruleset;
                this.mods = mods;
            }

            private IBindable<StarDifficulty?> localStarDifficulty;

            [BackgroundDependencyLoader]
            private void load()
            {
                difficultyCancellation = new CancellationTokenSource();
                localStarDifficulty = ruleset != null
                    ? difficultyCache.GetBindableDifficulty(beatmap, ruleset, mods, difficultyCancellation.Token)
                    : difficultyCache.GetBindableDifficulty(beatmap, difficultyCancellation.Token);
                localStarDifficulty.BindValueChanged(d =>
                {
                    if (d.NewValue is StarDifficulty diff)
                        StarDifficulty.Value = diff;
                });
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                difficultyCancellation?.Cancel();
            }
        }
    }
}
