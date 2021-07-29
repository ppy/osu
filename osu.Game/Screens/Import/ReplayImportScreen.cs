// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.Import
{
    [Cached(typeof(IPreviewTrackOwner))]
    public class ReplayImportScreen : OsuScreen, IPreviewTrackOwner
    {
        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        private BeatmapSetInfo onlineBeatmap;
        private Container container;
        private Container beatmapPanelContainer;
        private SettingsCheckbox automaticDownload;
        private ScorePanel scorePanel;
        private PurpleTriangleButton watchButton;

        private readonly APIBeatmap beatmap;
        private readonly ArchiveReader archive;
        private readonly Score score;

        public ReplayImportScreen(Score score, APIBeatmap beatmap, ArchiveReader archive)
        {
            this.score = score;
            this.beatmap = beatmap;
            this.archive = archive;
        }

        [BackgroundDependencyLoader]
        private void load(OsuGame game, ScoreManager scoreManager, OsuColour colours, OsuConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                scorePanel = new ScorePanel(score.ScoreInfo)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    Position = new Vector2(-0.25f, 0)
                },
                container = new Container
                {
                    Masking = true,
                    CornerRadius = 20,
                    AutoSizeAxes = Axes.Both,
                    AutoSizeDuration = 500,
                    AutoSizeEasing = Easing.OutQuint,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeafoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding(20),
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(15),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Beatmap info",
                                    Font = OsuFont.Default.With(size: 30),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Spacing = new Vector2(15),
                                    Children = new Drawable[]
                                    {
                                        beatmapPanelContainer = new Container
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        }
                                    }
                                },
                                automaticDownload = new SettingsCheckbox
                                {
                                    LabelText = "Automatically download beatmaps",
                                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadWhenSpectating),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                },
                                watchButton = new PurpleTriangleButton
                                {
                                    Text = "Start Watching",
                                    Width = 250,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Action = () =>
                                    {
                                        game.PresentScore(scoreManager.Import(archive).Result, ScorePresentType.Gameplay);
                                    },
                                    Enabled = { Value = false }
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scorePanel.StateChanged += _ => container.MoveToX(scorePanel.Size.X / 2, 500, Easing.OutQuint);
            automaticDownload.Current.BindValueChanged(_ => checkForAutomaticDownload());
            score.ScoreInfo.Beatmap = beatmap.ToBeatmap(rulesets);

            var onlineBeatmapRequest = new GetBeatmapSetRequest(beatmap.OnlineBeatmapSetID);

            onlineBeatmapRequest.Success += res => Schedule(() =>
            {
                onlineBeatmap = res.ToBeatmapSet(rulesets);
                beatmapPanelContainer.Child = new GridBeatmapPanel(onlineBeatmap);
            });

            api.Queue(onlineBeatmapRequest);
        }

        protected override void Update()
        {
            base.Update();
            checkForAutomaticDownload();
        }

        private void checkForAutomaticDownload()
        {
            if (onlineBeatmap == null)
                return;

            if (beatmaps.IsAvailableLocally(onlineBeatmap))
            {
                watchButton.Enabled.Value = true;
                return;
            }

            if (!automaticDownload.Current.Value)
                return;

            beatmaps.Download(onlineBeatmap);
        }

        public override bool OnExiting(IScreen next)
        {
            previewTrackManager.StopAnyPlaying(this);
            return base.OnExiting(next);
        }
    }
}
