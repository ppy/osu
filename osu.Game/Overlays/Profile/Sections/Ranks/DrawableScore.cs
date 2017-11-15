// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Leaderboards;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using OpenTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public abstract class DrawableScore : Container
    {
        private const int fade_duration = 200;

        protected readonly FillFlowContainer<OsuSpriteText> Stats;
        private readonly FillFlowContainer metadata;
        private readonly ScoreModsContainer modsContainer;
        protected readonly Score Score;
        private readonly Box underscoreLine;
        private readonly Box coloredBackground;
        private readonly Container background;

        protected DrawableScore(Score score)
        {
            Score = score;

            RelativeSizeAxes = Axes.X;
            Height = 60;
            Children = new Drawable[]
            {
                background = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 3,
                    Alpha = 0,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(0f, 1f),
                        Radius = 1f,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    Child = coloredBackground = new Box { RelativeSizeAxes = Axes.Both }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = 0.97f,
                    Children = new Drawable[]
                    {
                        underscoreLine = new Box
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 1,
                        },
                        new DrawableRank(score.Rank)
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 60,
                            FillMode = FillMode.Fit,
                        },
                        Stats = new FillFlowContainer<OsuSpriteText>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Direction = FillDirection.Vertical,
                        },
                        metadata = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Margin = new MarginPadding { Left = 70 },
                            Direction = FillDirection.Vertical,
                            Child = new OsuSpriteText
                            {
                                Text = score.Date.LocalDateTime.ToShortDateString(),
                                TextSize = 11,
                                Colour = OsuColour.Gray(0xAA),
                                Depth = -1,
                            },
                        },
                        modsContainer = new ScoreModsContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Width = 60,
                            Margin = new MarginPadding { Right = 160 }
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colour, LocalisationEngine locale, BeatmapSetOverlay beatmapSetOverlay)
        {
            coloredBackground.Colour = underscoreLine.Colour = colour.Gray4;

            Stats.Add(new OsuSpriteText
            {
                Text = $"accuracy: {Score.Accuracy:P2}",
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Colour = colour.GrayA,
                TextSize = 11,
                Font = "Exo2.0-RegularItalic",
                Depth = -1,
            });

            metadata.Add(new MetadataContainer(Score.Beatmap.Metadata.Title, Score.Beatmap.Metadata.Artist)
            {
                AutoSizeAxes = Axes.Both,
                Action = () =>
                {
                    if (Score.Beatmap.OnlineBeatmapSetID.HasValue) beatmapSetOverlay?.ShowBeatmapSet(Score.Beatmap.OnlineBeatmapSetID.Value);
                },
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference(
                                $"{Score.Beatmap.Metadata.TitleUnicode ?? Score.Beatmap.Metadata.Title} [{Score.Beatmap.Version}] ",
                                $"{Score.Beatmap.Metadata.Title ?? Score.Beatmap.Metadata.TitleUnicode} [{Score.Beatmap.Version}] "
                            ),
                            TextSize = 15,
                            Font = "Exo2.0-SemiBoldItalic",
                        },
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference(Score.Beatmap.Metadata.ArtistUnicode, Score.Beatmap.Metadata.Artist),
                            TextSize = 12,
                            Padding = new MarginPadding { Top = 3 },
                            Font = "Exo2.0-RegularItalic",
                        },
                    },
                },
            });

            foreach (Mod mod in Score.Mods)
                modsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.5f) });
        }

        protected override bool OnClick(InputState state) => true;

        protected override bool OnHover(InputState state)
        {
            background.FadeIn(fade_duration, Easing.OutQuint);
            underscoreLine.FadeOut(fade_duration, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            background.FadeOut(fade_duration, Easing.OutQuint);
            underscoreLine.FadeIn(fade_duration, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        private class MetadataContainer : OsuHoverContainer, IHasTooltip
        {
            public string TooltipText { get; set; }

            public MetadataContainer(string title, string artist)
            {
                TooltipText = $"{artist} - {title}";
            }
        }
    }
}
