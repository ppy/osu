// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelBeatmapStandalone : PanelBase
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.6f;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private IBindable<StarDifficulty?>? starDifficultyBindable;
        private CancellationTokenSource? starDifficultyCancellationSource;

        private BeatmapSetPanelBackground background = null!;

        private OsuSpriteText titleText = null!;
        private OsuSpriteText artistText = null!;
        private UpdateBeatmapSetButton updateButton = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;

        private ConstrainedIconContainer difficultyIcon = null!;
        private FillFlowContainer difficultyLine = null!;
        private StarRatingDisplay difficultyStarRating = null!;
        private TopLocalRank difficultyRank = null!;
        private OsuSpriteText difficultyKeyCountText = null!;
        private OsuSpriteText difficultyName = null!;
        private OsuSpriteText difficultyAuthor = null!;

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
                Size = new Vector2(20),
                Margin = new MarginPadding { Horizontal = 5f },
                Colour = colourProvider.Background5,
            };

            Background = background = new BeatmapSetPanelBackground
            {
                RelativeSizeAxes = Axes.Both,
            };

            Content.Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding { Top = 7.5f, Left = 15, Bottom = 5 },
                Children = new Drawable[]
                {
                    titleText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22, italics: true),
                        Shadow = true,
                    },
                    artistText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 17, italics: true),
                        Shadow = true,
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                        Margin = new MarginPadding { Top = 5f },
                        Children = new Drawable[]
                        {
                            updateButton = new UpdateBeatmapSetButton
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Right = 5f, Top = -2f },
                            },
                            statusPill = new BeatmapSetOnlineStatusPill
                            {
                                AutoSizeAxes = Axes.Both,
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                TextSize = 11,
                                TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                Margin = new MarginPadding { Right = 5f },
                            },
                            difficultyLine = new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    difficultyStarRating = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                                    {
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft,
                                        Scale = new Vector2(8f / 9f),
                                        Margin = new MarginPadding { Right = 5f },
                                    },
                                    difficultyRank = new TopLocalRank
                                    {
                                        Scale = new Vector2(8f / 11),
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Right = 5f },
                                    },
                                    difficultyKeyCountText = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Alpha = 0,
                                        Margin = new MarginPadding { Bottom = 2f },
                                    },
                                    difficultyName = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                        Origin = Anchor.BottomLeft,
                                        Anchor = Anchor.BottomLeft,
                                        Margin = new MarginPadding { Right = 5f, Bottom = 2f },
                                    },
                                    difficultyAuthor = new OsuSpriteText
                                    {
                                        Colour = colourProvider.Content2,
                                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                        Origin = Anchor.BottomLeft,
                                        Anchor = Anchor.BottomLeft,
                                        Margin = new MarginPadding { Right = 5f, Bottom = 2f },
                                    }
                                }
                            },
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

            difficultyRank.Beatmap = beatmap;
            difficultyName.Text = beatmap.DifficultyName;
            difficultyAuthor.Text = BeatmapsetsStrings.ShowDetailsMappedBy(beatmap.Metadata.Author.Username);
            difficultyLine.Show();

            computeStarRating();
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            background.Beatmap = null;
            updateButton.BeatmapSet = null;
            difficultyRank.Beatmap = null;
            starDifficultyBindable = null;
        }

        private void computeStarRating()
        {
            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            if (Item == null)
                return;

            var beatmap = (BeatmapInfo)Item.Model;

            starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmap, starDifficultyCancellationSource.Token);
            starDifficultyBindable.BindValueChanged(_ => updateDisplay(), true);
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

                difficultyKeyCountText.Alpha = 1;
                difficultyKeyCountText.Text = $"[{keyCount}K] ";
            }
            else
                difficultyKeyCountText.Alpha = 0;
        }

        private void updateDisplay()
        {
            const float duration = 500;

            var starDifficulty = starDifficultyBindable?.Value ?? default;

            AccentColour = colours.ForStarDifficulty(starDifficulty.Stars);
            difficultyIcon.FadeColour(starDifficulty.Stars > 6.5f ? colours.Orange1 : colourProvider.Background5, duration, Easing.OutQuint);
            difficultyStarRating.Current.Value = starDifficulty;
        }
    }
}
