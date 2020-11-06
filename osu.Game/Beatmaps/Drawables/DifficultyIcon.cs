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
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class DifficultyIcon : CompositeDrawable, IHasCustomTooltip
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
                    Scale = new Vector2(0.84f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.08f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                    },
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.ForDifficultyRating(beatmap.DifficultyRating) // Default value that will be re-populated once difficulty calculation completes
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

            difficultyBindable.BindValueChanged(difficulty => background.Colour = colours.ForDifficultyRating(difficulty.NewValue.DifficultyRating));
        }

        public ITooltip GetCustomTooltip() => new DifficultyIconTooltip();

        public object TooltipContent => shouldShowTooltip ? new DifficultyIconTooltipContent(beatmap, difficultyBindable) : null;

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

            private IBindable<StarDifficulty> localStarDifficulty;

            [BackgroundDependencyLoader]
            private void load()
            {
                difficultyCancellation = new CancellationTokenSource();
                localStarDifficulty = ruleset != null
                    ? difficultyCache.GetBindableDifficulty(beatmap, ruleset, mods, difficultyCancellation.Token)
                    : difficultyCache.GetBindableDifficulty(beatmap, difficultyCancellation.Token);
                localStarDifficulty.BindValueChanged(difficulty => StarDifficulty.Value = difficulty.NewValue);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                difficultyCancellation?.Cancel();
            }
        }

        private class DifficultyIconTooltipContent
        {
            public readonly BeatmapInfo Beatmap;
            public readonly IBindable<StarDifficulty> Difficulty;

            public DifficultyIconTooltipContent(BeatmapInfo beatmap, IBindable<StarDifficulty> difficulty)
            {
                Beatmap = beatmap;
                Difficulty = difficulty;
            }
        }

        private class DifficultyIconTooltip : VisibilityContainer, ITooltip
        {
            private readonly OsuSpriteText difficultyName, starRating;
            private readonly Box background;
            private readonly FillFlowContainer difficultyFlow;

            public DifficultyIconTooltip()
            {
                AutoSizeAxes = Axes.Both;
                Masking = true;
                CornerRadius = 5;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        AutoSizeDuration = 200,
                        AutoSizeEasing = Easing.OutQuint,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(10),
                        Children = new Drawable[]
                        {
                            difficultyName = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                            },
                            difficultyFlow = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    starRating = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Font = OsuFont.GetFont(size: 16, weight: FontWeight.Regular),
                                    },
                                    new SpriteIcon
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Margin = new MarginPadding { Left = 4 },
                                        Icon = FontAwesome.Solid.Star,
                                        Size = new Vector2(12),
                                    },
                                }
                            }
                        }
                    }
                };
            }

            [Resolved]
            private OsuColour colours { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                background.Colour = colours.Gray3;
            }

            private readonly IBindable<StarDifficulty> starDifficulty = new Bindable<StarDifficulty>();

            public bool SetContent(object content)
            {
                if (!(content is DifficultyIconTooltipContent iconContent))
                    return false;

                difficultyName.Text = iconContent.Beatmap.Version;

                starDifficulty.UnbindAll();
                starDifficulty.BindTo(iconContent.Difficulty);
                starDifficulty.BindValueChanged(difficulty =>
                {
                    starRating.Text = $"{difficulty.NewValue.Stars:0.##}";
                    difficultyFlow.Colour = colours.ForDifficultyRating(difficulty.NewValue.DifficultyRating, true);
                }, true);

                return true;
            }

            public void Move(Vector2 pos) => Position = pos;

            protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

            protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);
        }
    }
}
