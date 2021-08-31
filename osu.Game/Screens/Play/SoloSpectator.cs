// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Spectator;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Spectate;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Play
{
    [Cached(typeof(IPreviewTrackOwner))]
    public class SoloSpectator : SpectatorScreen, IPreviewTrackOwner
    {
        [NotNull]
        private readonly User targetUser;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private Container beatmapPanelContainer;
        private TriangleButton watchButton;
        private SettingsCheckbox automaticDownload;
        private BeatmapSetInfo onlineBeatmap;

        /// <summary>
        /// The player's immediate online gameplay state.
        /// This doesn't always reflect the gameplay state being watched.
        /// </summary>
        private GameplayState immediateGameplayState;

        private GetBeatmapSetRequest onlineBeatmapRequest;

        public SoloSpectator([NotNull] User targetUser)
            : base(targetUser.Id)
        {
            this.targetUser = targetUser;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OsuConfigManager config)
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
                                Action = () => scheduleStart(immediateGameplayState),
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

        protected override void OnUserStateChanged(int userId, SpectatorState spectatorState)
        {
            clearDisplay();
            showBeatmapPanel(spectatorState);
        }

        protected override void StartGameplay(int userId, GameplayState gameplayState)
        {
            immediateGameplayState = gameplayState;
            watchButton.Enabled.Value = true;

            scheduleStart(gameplayState);
        }

        protected override void EndGameplay(int userId)
        {
            scheduledStart?.Cancel();
            immediateGameplayState = null;
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

        private void scheduleStart(GameplayState gameplayState)
        {
            // This function may be called multiple times in quick succession once the screen becomes current again.
            scheduledStart?.Cancel();
            scheduledStart = Schedule(() =>
            {
                if (this.IsCurrentScreen())
                    start();
                else
                    scheduleStart(gameplayState);
            });

            void start()
            {
                Beatmap.Value = gameplayState.Beatmap;
                Ruleset.Value = gameplayState.Ruleset.RulesetInfo;

                this.Push(new SpectatorPlayerLoader(gameplayState.Score, () => new SoloSpectatorPlayer(gameplayState.Score)));
            }
        }

        private void showBeatmapPanel(SpectatorState state)
        {
            Debug.Assert(state.BeatmapID != null);

            onlineBeatmapRequest = new GetBeatmapSetRequest(state.BeatmapID.Value, BeatmapSetLookupType.BeatmapId);
            onlineBeatmapRequest.Success += res => Schedule(() =>
            {
                onlineBeatmap = res.ToBeatmapSet(rulesets);
                beatmapPanelContainer.Child = new GridBeatmapPanel(onlineBeatmap);
                checkForAutomaticDownload();
            });

            api.Queue(onlineBeatmapRequest);
        }

        private void checkForAutomaticDownload()
        {
            if (onlineBeatmap == null)
                return;

            if (!automaticDownload.Current.Value)
                return;

            if (beatmaps.IsAvailableLocally(onlineBeatmap))
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
