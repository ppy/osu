// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Spectate;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play
{
    [Cached(typeof(IPreviewTrackOwner))]
    public partial class SoloSpectatorScreen : SpectatorScreen, IPreviewTrackOwner
    {
        [NotNull]
        private readonly APIUser targetUser;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; }

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private Container beatmapPanelContainer;
        private RoundedButton watchButton;
        private SettingsCheckbox automaticDownload;

        /// <summary>
        /// The player's immediate online gameplay state.
        /// This doesn't always reflect the gameplay state being watched.
        /// </summary>
        private SpectatorGameplayState immediateSpectatorGameplayState;

        private GetBeatmapSetRequest onlineBeatmapRequest;

        private APIBeatmapSet beatmapSet;

        public SoloSpectatorScreen([NotNull] APIUser targetUser)
            : base(targetUser.Id)
        {
            this.targetUser = targetUser;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            InternalChild = new Container
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
                        Colour = colourProvider.Background5,
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
                                Text = "Spectator Mode",
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
                                    new UserGridPanel(targetUser)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Height = 145,
                                        Width = 290,
                                    },
                                    new SpriteIcon
                                    {
                                        Size = new Vector2(40),
                                        Icon = FontAwesome.Solid.ArrowRight,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    beatmapPanelContainer = new Container
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                }
                            },
                            automaticDownload = new SettingsCheckbox
                            {
                                LabelText = "Automatically download beatmaps",
                                Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            watchButton = new PurpleRoundedButton
                            {
                                Text = "Start Watching",
                                Width = 250,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Action = () => scheduleStart(immediateSpectatorGameplayState),
                                Enabled = { Value = false }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            automaticDownload.Current.BindValueChanged(_ => checkForAutomaticDownload());
        }

        protected override void OnNewPlayingUserState(int userId, SpectatorState spectatorState)
        {
            clearDisplay();
            showBeatmapPanel(spectatorState);
        }

        protected override void StartGameplay(int userId, SpectatorGameplayState spectatorGameplayState)
        {
            immediateSpectatorGameplayState = spectatorGameplayState;
            watchButton.Enabled.Value = true;

            scheduleStart(spectatorGameplayState);
        }

        protected override void QuitGameplay(int userId)
        {
            scheduledStart?.Cancel();
            immediateSpectatorGameplayState = null;
            watchButton.Enabled.Value = false;

            clearDisplay();
        }

        private void clearDisplay()
        {
            watchButton.Enabled.Value = false;
            onlineBeatmapRequest?.Cancel();
            beatmapPanelContainer.Clear();
            previewTrackManager.StopAnyPlaying(this);
        }

        private ScheduledDelegate scheduledStart;

        private void scheduleStart(SpectatorGameplayState spectatorGameplayState)
        {
            // This function may be called multiple times in quick succession once the screen becomes current again.
            scheduledStart?.Cancel();
            scheduledStart = Schedule(() =>
            {
                if (this.IsCurrentScreen())
                    start();
                else
                    scheduleStart(spectatorGameplayState);
            });

            void start()
            {
                Beatmap.Value = spectatorGameplayState.Beatmap;
                Ruleset.Value = spectatorGameplayState.Ruleset.RulesetInfo;

                this.Push(new SpectatorPlayerLoader(spectatorGameplayState.Score, () => new SoloSpectatorPlayer(spectatorGameplayState.Score)));
            }
        }

        private void showBeatmapPanel(SpectatorState state)
        {
            Debug.Assert(state.BeatmapID != null);

            onlineBeatmapRequest = new GetBeatmapSetRequest(state.BeatmapID.Value, BeatmapSetLookupType.BeatmapId);
            onlineBeatmapRequest.Success += beatmapSet => Schedule(() =>
            {
                this.beatmapSet = beatmapSet;
                beatmapPanelContainer.Child = new BeatmapCardNormal(this.beatmapSet, allowExpansion: false);
                checkForAutomaticDownload();
            });

            api.Queue(onlineBeatmapRequest);
        }

        private void checkForAutomaticDownload()
        {
            if (beatmapSet == null)
                return;

            if (!automaticDownload.Current.Value)
                return;

            if (beatmaps.IsAvailableLocally(new BeatmapSetInfo { OnlineID = beatmapSet.OnlineID }))
                return;

            beatmapDownloader.Download(beatmapSet);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            previewTrackManager.StopAnyPlaying(this);
            return base.OnExiting(e);
        }
    }
}
