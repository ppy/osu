// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelBeatmapStandalone : Panel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.6f;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private ISongSelect? songSelect { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private IBindable<StarDifficulty>? starDifficultyBindable;
        private CancellationTokenSource? starDifficultyCancellationSource;

        private PanelSetBackground background = null!;

        private OsuSpriteText titleText = null!;
        private OsuSpriteText artistText = null!;
        private PanelUpdateBeatmapButton updateButton = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;

        private ConstrainedIconContainer difficultyIcon = null!;
        private FillFlowContainer difficultyLine = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private StarCounter starCounter = null!;
        private PanelLocalRankDisplay localRank = null!;
        private OsuSpriteText keyCountText = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText authorText = null!;

        public PanelBeatmapStandalone()
        {
            PanelXOffset = 20;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Height = HEIGHT;

            Icon = difficultyIcon = new ConstrainedIconContainer
            {
                Size = new Vector2(16),
                Margin = new MarginPadding { Horizontal = 5f },
                Colour = colourProvider.Background5,
            };

            Background = background = new PanelSetBackground
            {
                RelativeSizeAxes = Axes.Both,
            };

            Content.Child = new FillFlowContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Padding = new MarginPadding { Left = 10f },
                Direction = FillDirection.Vertical,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    titleText = new OsuSpriteText
                    {
                        Font = OsuFont.Style.Heading2.With(typeface: Typeface.TorusAlternate, weight: FontWeight.Bold),
                    },
                    artistText = new OsuSpriteText
                    {
                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                        Padding = new MarginPadding { Top = -2 },
                    },
                    difficultyLine = new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = 4},
                        Children = new Drawable[]
                        {
                            statusPill = new BeatmapSetOnlineStatusPill
                            {
                                Animated = false,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                TextSize = OsuFont.Style.Caption2.Size,
                                Margin = new MarginPadding { Right = 5f },
                            },
                            updateButton = new PanelUpdateBeatmapButton
                            {
                                Scale = new Vector2(0.7f),
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Right = 5f, Top = -2f },
                            },
                            keyCountText = new OsuSpriteText
                            {
                                Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Alpha = 0,
                            },
                            difficultyText = new OsuSpriteText
                            {
                                Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Margin = new MarginPadding { Right = 3f },
                            },
                            authorText = new OsuSpriteText
                            {
                                Colour = colourProvider.Content2,
                                Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(3),
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            localRank = new PanelLocalRankDisplay
                            {
                                Scale = new Vector2(0.65f),
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                            },
                            starRatingDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Small, animated: true)
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Scale = new Vector2(0.875f),
                            },
                            starCounter = new StarCounter
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Scale = new Vector2(0.4f)
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ =>
            {
                computeStarRating();
                updateKeyCount();
            });

            mods.BindValueChanged(_ =>
            {
                computeStarRating();
                updateKeyCount();
            }, true);

            Selected.BindValueChanged(s => Expanded.Value = s.NewValue, true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            var beatmap = (BeatmapInfo)Item.Model;
            var beatmapSet = beatmap.BeatmapSet!;

            // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
            background.Beatmap = beatmaps.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID));

            titleText.Text = new RomanisableString(beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title);
            artistText.Text = new RomanisableString(beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist);
            updateButton.BeatmapSet = beatmapSet;
            statusPill.Status = beatmapSet.Status;

            difficultyIcon.Icon = beatmap.Ruleset.CreateInstance().CreateIcon();
            difficultyIcon.Show();

            localRank.Beatmap = beatmap;
            difficultyText.Text = beatmap.DifficultyName;
            authorText.Text = BeatmapsetsStrings.ShowDetailsMappedBy(beatmap.Metadata.Author.Username);
            difficultyLine.Show();

            computeStarRating();
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            background.Beatmap = null;
            updateButton.BeatmapSet = null;
            localRank.Beatmap = null;
            starDifficultyBindable = null;

            starDifficultyCancellationSource?.Cancel();
        }

        private void computeStarRating()
        {
            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            if (Item == null)
                return;

            var beatmap = (BeatmapInfo)Item.Model;

            starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmap, starDifficultyCancellationSource.Token, SongSelect.SELECTION_DEBOUNCE);
            starDifficultyBindable.BindValueChanged(_ => updateDisplay(), true);
        }

        protected override void Update()
        {
            base.Update();

            if (Item?.IsVisible != true)
            {
                starDifficultyCancellationSource?.Cancel();
                starDifficultyCancellationSource = null;
            }

            // Dirty hack to make sure we don't take up spacing in parent fill flow when not displaying a rank.
            // I can't find a better way to do this.
            starRatingDisplay.Margin = new MarginPadding { Left = 1 / starRatingDisplay.Scale.X * (localRank.HasRank ? 0 : -3) };
        }

        private void updateKeyCount()
        {
            if (Item == null)
                return;

            var beatmap = (BeatmapInfo)Item.Model;

            if (ruleset.Value.OnlineID == 3)
            {
                // Account for mania differences locally for now.
                // Eventually this should be handled in a more modular way, allowing rulesets to add more information to the panel.
                ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.Value.CreateInstance();
                int keyCount = legacyRuleset.GetKeyCount(beatmap, mods.Value);

                keyCountText.Alpha = 1;
                keyCountText.Text = $"[{keyCount}K] ";
            }
            else
                keyCountText.Alpha = 0;
        }

        private void updateDisplay()
        {
            const float duration = 500;

            var starDifficulty = starDifficultyBindable?.Value ?? default;

            starRatingDisplay.Current.Value = starDifficulty;
            starCounter.Current = (float)starDifficulty.Stars;

            difficultyIcon.FadeColour(starDifficulty.Stars > OsuColour.STAR_DIFFICULTY_DEFINED_COLOUR_CUTOFF ? colours.Orange1 : colourProvider.Background5, duration, Easing.OutQuint);

            var starRatingColour = colours.ForStarDifficulty(starDifficulty.Stars);
            starCounter.FadeColour(starRatingColour, duration, Easing.OutQuint);
            AccentColour = colours.ForStarDifficulty(starDifficulty.Stars);
        }

        public override MenuItem[] ContextMenuItems
        {
            get
            {
                if (Item == null)
                    return Array.Empty<MenuItem>();

                List<MenuItem> items = new List<MenuItem>();

                if (songSelect != null)
                    items.AddRange(songSelect.GetForwardActions((BeatmapInfo)Item.Model));

                return items.ToArray();
            }
        }
    }
}
