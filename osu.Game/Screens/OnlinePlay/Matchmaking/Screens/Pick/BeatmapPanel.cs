// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Screens.Pick
{
    public partial class BeatmapPanel : CompositeDrawable
    {
        public static readonly Vector2 SIZE = new Vector2(300, 70);

        public readonly Container OverlayLayer = new Container { RelativeSizeAxes = Axes.Both };

        public APIBeatmap? Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap?.OnlineID == value?.OnlineID)
                    return;

                beatmap = value;

                if (IsLoaded)
                    updateContent();
            }
        }

        private APIBeatmap? beatmap;

        private Container content = null!;
        private UpdateableOnlineBeatmapSetCover cover = null!;

        public BeatmapPanel(APIBeatmap? beatmap = null)
        {
            this.beatmap = beatmap;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 6;

            InternalChildren = new Drawable[]
            {
                cover = new UpdateableOnlineBeatmapSetCover(BeatmapSetCoverType.Card, timeBeforeLoad: 0, timeBeforeUnload: 10000)
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientHorizontal(
                        colourProvider.Background4.Opacity(0.7f),
                        colourProvider.Background4.Opacity(0.4f)
                    )
                },
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                OverlayLayer,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateContent();
            FinishTransforms(true);
        }

        private void updateContent()
        {
            foreach (var child in content.Children)
                child.FadeOut(300).Expire();

            cover.OnlineInfo = beatmap?.BeatmapSet;

            if (beatmap != null)
            {
                var panelContent = new BeatmapPanelContent(beatmap)
                {
                    RelativeSizeAxes = Axes.Both,
                };

                content.Add(panelContent);

                panelContent.FadeInFromZero(300);
            }
        }

        private partial class BeatmapPanelContent : CompositeDrawable
        {
            private readonly APIBeatmap beatmap;

            public BeatmapPanelContent(APIBeatmap beatmap)
            {
                this.beatmap = beatmap;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding { Horizontal = 12 },
                    Children = new Drawable[]
                    {
                        new TruncatingSpriteText
                        {
                            Text = new RomanisableString(beatmap.Metadata.TitleUnicode, beatmap.Metadata.TitleUnicode),
                            Font = OsuFont.Default.With(size: 19, weight: FontWeight.SemiBold),
                            RelativeSizeAxes = Axes.X,
                        },
                        new TextFlowContainer(s =>
                        {
                            s.Font = OsuFont.GetFont(size: 16, weight: FontWeight.SemiBold);
                        }).With(d =>
                        {
                            d.RelativeSizeAxes = Axes.X;
                            d.AutoSizeAxes = Axes.Y;
                            d.AddText("by ");
                            d.AddText(new RomanisableString(beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist));
                        }),
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Top = 6 },
                            Spacing = new Vector2(4),
                            Children = new Drawable[]
                            {
                                new StarRatingDisplay(new StarDifficulty(beatmap.StarRating, 0), StarRatingDisplaySize.Small)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                new TruncatingSpriteText
                                {
                                    Text = beatmap.DifficultyName,
                                    Font = OsuFont.Default.With(size: 16, weight: FontWeight.SemiBold),
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                            }
                        },
                    },
                };
            }
        }
    }
}
