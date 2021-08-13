// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Setup
{
    public class SetupScreen : TournamentScreen, IProvideVideo
    {
        private FillFlowContainer fillFlow;

        private LoginOverlay loginOverlay;
        private ResolutionSelector resolution;

        [Resolved]
        private MatchIPCInfo ipc { get; set; }

        [Resolved]
        private StableInfo stableInfo { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved(canBeNull: true)]
        private TournamentSceneManager sceneManager { get; set; }

        private Bindable<Size> windowSize;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            windowSize = frameworkConfig.GetBindable<Size>(FrameworkSetting.WindowedSize);

            InternalChild = fillFlow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10),
            };

            api.LocalUser.BindValueChanged(_ => Schedule(reload));
            stableInfo.OnStableInfoSaved += () => Schedule(reload);
            reload();
        }

        private void reload()
        {
            var fileBasedIpc = ipc as FileBasedIPC;
            fillFlow.Children = new Drawable[]
            {
                new ActionableInfo
                {
                    Label = "Current IPC source",
                    ButtonText = "Change source",
                    Action = () => sceneManager?.SetScreen(new StablePathSelectScreen()),
                    Value = fileBasedIpc?.IPCStorage?.GetFullPath(string.Empty) ?? "Not found",
                    Failing = fileBasedIpc?.IPCStorage == null,
                    Description = "The osu!stable installation which is currently being used as a data source. If a source is not found, make sure you have created an empty ipc.txt in your stable cutting-edge installation."
                },
                new ActionableInfo
                {
                    Label = "Current user",
                    ButtonText = "Change sign-in",
                    Action = () =>
                    {
                        api.Logout();

                        if (loginOverlay == null)
                        {
                            AddInternal(loginOverlay = new LoginOverlay
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                            });
                        }

                        loginOverlay.State.Value = Visibility.Visible;
                    },
                    Value = api?.LocalUser.Value.Username,
                    Failing = api?.IsLoggedIn != true,
                    Description = "In order to access the API and display metadata, signing in is required."
                },
                new LabelledDropdown<RulesetInfo>
                {
                    Label = "Ruleset",
                    Description = "Decides what stats are displayed and which ranks are retrieved for players.",
                    Items = rulesets.AvailableRulesets,
                    Current = LadderInfo.Ruleset,
                },
                new TournamentSwitcher
                {
                    Label = "Current tournament",
                    Description = "Changes the background videos and bracket to match the selected tournament. This requires a restart to apply changes.",
                },
                resolution = new ResolutionSelector
                {
                    Label = "Stream area resolution",
                    ButtonText = "Set height",
                    Action = height =>
                    {
                        windowSize.Value = new Size((int)(height * aspect_ratio / TournamentSceneManager.STREAM_AREA_WIDTH * TournamentSceneManager.REQUIRED_WIDTH), height);
                    }
                },
            };
        }

        private const float aspect_ratio = 16f / 9f;

        protected override void Update()
        {
            base.Update();

            resolution.Value = $"{ScreenSpaceDrawQuad.Width:N0}x{ScreenSpaceDrawQuad.Height:N0}";
        }

        public class LabelledDropdown<T> : LabelledComponent<OsuDropdown<T>, T>
        {
            public LabelledDropdown()
                : base(true)
            {
            }

            public IEnumerable<T> Items
            {
                get => Component.Items;
                set => Component.Items = value;
            }

            protected override OsuDropdown<T> CreateComponent() => new OsuDropdown<T>
            {
                RelativeSizeAxes = Axes.X,
                Width = 0.5f,
            };
        }
    }
}
