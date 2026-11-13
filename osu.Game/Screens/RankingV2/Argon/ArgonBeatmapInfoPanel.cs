// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Argon
{
    public partial class ArgonBeatmapInfoPanel : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable
    {
        public const float WIDTH = 560;
        public const float HEIGHT = 150;
        public const float SUB_WEDGE_HEIGHT = 40;

        private const float text_padding = 8 + ShearedButton.CORNER_RADIUS;
        private const float icon_size = 24;

        public static EdgeEffectParameters CreateShadowEdgeEffect() => new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 4,
            Hollow = true,
            Colour = Colour4.Black.Opacity(0.2f),
        };

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private Container content = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;
        private MarqueeContainer titleText = null!;
        private MarqueeContainer artistText = null!;
        private Container rulesetIconContainer = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private MarqueeContainer difficultyText = null!;

        private CancellationTokenSource? difficultyRetrievalCancellation;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Width = WIDTH;
            Height = HEIGHT;

            InternalChildren =
            [
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Shear = OsuGame.SHEAR,
                    Masking = true,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    EdgeEffect = CreateShadowEdgeEffect(),
                    Children =
                    [
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children =
                            [
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background3.Opacity(0.9f),
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding
                                    {
                                        Left = text_padding + HEIGHT * OsuGame.SHEAR.X,
                                        Right = 30,
                                        Bottom = SUB_WEDGE_HEIGHT + 8,
                                    },
                                    Shear = -OsuGame.SHEAR,
                                    Children = new Drawable[]
                                    {
                                        statusPill = new BeatmapSetOnlineStatusPill
                                        {
                                            Animated = false,
                                        },
                                        titleText = new MarqueeContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        artistText = new MarqueeContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                        },
                                    }
                                },
                            ]
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = SUB_WEDGE_HEIGHT,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Masking = true,
                            CornerRadius = ShearedButton.CORNER_RADIUS,
                            Children =
                            [
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background5,
                                },
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Shear = -OsuGame.SHEAR,
                                    Padding = new MarginPadding
                                    {
                                        Left = text_padding + (HEIGHT - SUB_WEDGE_HEIGHT) * OsuGame.SHEAR.X,
                                        Right = text_padding,
                                    },
                                    ColumnDimensions =
                                    [
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    ],
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            rulesetIconContainer = new Container
                                            {
                                                Size = new Vector2(icon_size),
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Margin = new MarginPadding { Right = 5, },
                                            },
                                            starRatingDisplay = new StarRatingDisplay(new StarDifficulty())
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Scale = new Vector2(1.2f),
                                                Margin = new MarginPadding { Right = 5, },
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Masking = true,
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Child = difficultyText = new MarqueeContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Anchor = Anchor.CentreLeft,
                                                    Origin = Anchor.CentreLeft,
                                                },
                                            }
                                        },
                                    },
                                }
                            ],
                        },
                    ],
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateScore(), true);
        }

        private void updateScore()
        {
            Debug.Assert(score.Value.Beatmap != null);

            statusPill.Status = score.Value.Beatmap.Status;
            titleText.CreateContent = () => new OsuSpriteText
            {
                Text = new RomanisableString(score.Value.Beatmap.Metadata.TitleUnicode, score.Value.Beatmap.Metadata.Title),
                Font = OsuFont.Style.Title.With(size: 36),
            };
            artistText.CreateContent = () => new OsuSpriteText
            {
                Text = new RomanisableString(score.Value.Beatmap.Metadata.ArtistUnicode, score.Value.Beatmap.Metadata.Artist),
                Font = OsuFont.Style.Heading2.With(size: 24),
            };

            rulesetIconContainer.Clear();
            var ruleset = score.Value.Ruleset.CreateInstance();
            rulesetIconContainer.Add(ruleset.CreateIcon().With(i => i.Size = new Vector2(icon_size)));

            difficultyText.CreateContent = () =>
            {
                var flow = new OsuTextFlowContainer(t => t.Font = OsuFont.Style.Heading2)
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Bottom = 2, },
                };
                flow.AddText(score.Value.Beatmap.DifficultyName, t => t.Font = OsuFont.Style.Heading2.With(size: 22));
                flow.AddText(" ");
                flow.AddText(BeatmapsetsStrings.ShowDetailsMappedBy(string.Empty), t => t.Font = OsuFont.Style.Caption1.With(size: 18));
                flow.AddText(score.Value.Beatmap.Metadata.Author.Username, t => t.Font = OsuFont.Style.Caption1.With(size: 18, weight: FontWeight.SemiBold));

                return flow;
            };

            difficultyRetrievalCancellation?.Cancel();
            difficultyRetrievalCancellation = new CancellationTokenSource();
            difficultyCache.GetDifficultyAsync(score.Value.Beatmap, ruleset.RulesetInfo, score.Value.Mods.Select(m => m.ToMod(ruleset)),
                               cancellationToken: difficultyRetrievalCancellation.Token)
                           .ContinueWith(t =>
                           {
                               var difficulty = t.GetResultSafely() ?? new StarDifficulty(score.Value.Beatmap.StarRating, 0);
                               Schedule(() =>
                               {
                                   starRatingDisplay.Current.Value = difficulty;

                                   var col = starRatingDisplay.ForegroundTextColour;
                                   rulesetIconContainer.Colour = col;
                                   difficultyText.Colour = col;
                               });
                           });
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        // TODO: temporary to avoid `SkinDeserialisationTest` failures.
        // Remove when the argon skin supplies a default layout of results with this component included.
        public bool IsEditable => false;

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            const double transition_duration = 500;

            FinishAnimating();

            content.FadeOut()
                   .MoveToOffset(new Vector2(-50, 0));

            using (BeginAbsoluteSequence(startTime))
            {
                content.FadeIn(transition_duration, Easing.OutQuint)
                       .MoveToOffset(new Vector2(50, 0), transition_duration, Easing.OutQuint);

                return content.LatestTransformEndTime;
            }
        }

        public void FinishAnimating()
        {
            content.FinishTransforms();
        }

        #endregion
    }
}
