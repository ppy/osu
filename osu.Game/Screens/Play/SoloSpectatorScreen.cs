// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
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
        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private BeatmapModelDownloader beatmapDownloader { get; set; } = null!;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private Container beatmapPanelContainer = null!;
        private RoundedButton watchButton = null!;
        private SettingsCheckbox automaticDownload = null!;

        private readonly APIUser targetUser;

        /// <summary>
        /// The player's immediate online gameplay state.
        /// This doesn't always reflect the gameplay state being watched.
        /// </summary>
        private SpectatorGameplayState? immediateSpectatorGameplayState;

        private GetBeatmapSetRequest? onlineBeatmapRequest;

        private APIBeatmapSet? beatmapSet;

        private OsuScreenStack playerScreenStack = null!;

        private Container screenStackContainer = null!;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        public SoloSpectatorScreen(APIUser targetUser)
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

            AddInternal(screenStackContainer = new Container
            {
                CornerRadius = 10,
                Masking = true,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(0.95f),
                Children = new Drawable[]
                {
                    playerScreenStack = new OsuScreenStack()
                }
            });
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

            playerScreenStack.CurrentScreen?.Exit();
        }

        private void clearDisplay()
        {
            watchButton.Enabled.Value = false;
            onlineBeatmapRequest?.Cancel();
            beatmapPanelContainer.Clear();
            previewTrackManager.StopAnyPlaying(this);
        }

        private ScheduledDelegate? scheduledStart;

        private void scheduleStart(SpectatorGameplayState? spectatorGameplayState)
        {
            Debug.Assert(spectatorGameplayState != null);

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

                screenStackContainer.FadeInFromZero(500);
                screenStackContainer
                    .ScaleTo(0.9f)
                    .ScaleTo(0.95f, 500, Easing.OutQuint);
                playerScreenStack.Push(new SpectatorPlayerLoader(spectatorGameplayState.Score, () => new SoloSpectatorPlayer(spectatorGameplayState.Score)));

                // LoadComponentAsync(new SpectatorPlayerLoader(spectatorGameplayState.Score, () => new SoloSpectatorPlayer(spectatorGameplayState.Score)), loader =>
                // {
                //     playerScreenStack.FadeInFromZero(500);
                //     playerScreenStack
                //         .ScaleTo(0.9f)
                //         .ScaleTo(0.95f, 500, Easing.OutQuint);
                //
                //     playerScreenStack.PushSynchronously(loader);
                // });
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
