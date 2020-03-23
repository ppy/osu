// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens
{
    public class SetupScreen : TournamentScreen, IProvideVideo
    {
        private FillFlowContainer fillFlow;

        private LoginOverlay loginOverlay;
        private ActionableInfo resolution;

        [Resolved]
        private MatchIPCInfo ipc { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

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
            reload();
        }

        [Resolved]
        private Framework.Game game { get; set; }

        private void reload()
        {
            var fileBasedIpc = ipc as FileBasedIPC;

            fillFlow.Children = new Drawable[]
            {
                new ActionableInfo
                {
                    Label = "当前的IPC源",
                    ButtonText = "刷新",
                    Action = () =>
                    {
                        fileBasedIpc?.LocateStableStorage();
                        reload();
                    },
                    Value = fileBasedIpc?.Storage?.GetFullPath(string.Empty) ?? "未找到",
                    Failing = fileBasedIpc?.Storage == null,
                    Description = "将使用osu！stable安装目录作为IPC的数据源。 如果找不到源，请确保在osu!stable的最新cutting-edge中创建了一个空的ipc.txt，并将其注册为默认的osu！安装目录。"
                },
                new ActionableInfo
                {
                    Label = "当前用户",
                    ButtonText = "更换用户",
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
                    Description = "要访问API并获取元数据, 你需要先登录."
                },
                new LabelledDropdown<RulesetInfo>
                {
                    Label = "游戏模式",
                    Description = "决定显示哪些统计数据以及为玩家检索哪些排名",
                    Items = rulesets.AvailableRulesets,
                    Current = LadderInfo.Ruleset,
                },
                resolution = new ActionableInfo
                {
                    Label = "推流区分辨率",
                    ButtonText = "设置为1080p",
                    Action = () =>
                    {
                        windowSize.Value = new Size((int)(1920 / TournamentSceneManager.STREAM_AREA_WIDTH * TournamentSceneManager.REQUIRED_WIDTH), 1080);
                    }
                },
            };
        }

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

        private class ActionableInfo : LabelledDrawable<Drawable>
        {
            private OsuButton button;

            public ActionableInfo()
                : base(true)
            {
            }

            public string ButtonText
            {
                set => button.Text = value;
            }

            public string Value
            {
                set => valueText.Text = value;
            }

            public bool Failing
            {
                set => valueText.Colour = value ? Color4.Red : Color4.White;
            }

            public Action Action;

            private TournamentSpriteText valueText;

            protected override Drawable CreateComponent() => new Container
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    valueText = new TournamentSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    button = new TriangleButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(100, 30),
                        Action = () => Action?.Invoke()
                    },
                }
            };
        }
    }
}