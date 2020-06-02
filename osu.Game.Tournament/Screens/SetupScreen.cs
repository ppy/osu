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
        private ResolutionSelector resolution;

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
                resolution = new ResolutionSelector
                {
                    Label = "推流区分辨率",
                    ButtonText = "设置高度",
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
            protected FillFlowContainer FlowContainer;

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
                    FlowContainer = new FillFlowContainer
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        AutoSizeAxes = Axes.Both,
                        Spacing = new Vector2(10, 0),
                        Children = new Drawable[]
                        {
                            button = new TriangleButton
                            {
                                Size = new Vector2(100, 40),
                                Action = () => Action?.Invoke()
                            }
                        }
                    }
                }
            };
        }

        private class ResolutionSelector : ActionableInfo
        {
            private const int minimum_window_height = 480;
            private const int maximum_window_height = 2160;

            public new Action<int> Action;

            private OsuNumberBox numberBox;

            protected override Drawable CreateComponent()
            {
                var drawable = base.CreateComponent();
                FlowContainer.Insert(-1, numberBox = new OsuNumberBox
                {
                    Text = "1080",
                    Width = 100
                });

                base.Action = () =>
                {
                    if (string.IsNullOrEmpty(numberBox.Text))
                        return;

                    // box contains text
                    if (!int.TryParse(numberBox.Text, out var number))
                    {
                        // at this point, the only reason we can arrive here is if the input number was too big to parse into an int
                        // so clamp to max allowed value
                        number = maximum_window_height;
                    }
                    else
                    {
                        number = Math.Clamp(number, minimum_window_height, maximum_window_height);
                    }

                    // in case number got clamped, reset number in numberBox
                    numberBox.Text = number.ToString();

                    Action?.Invoke(number);
                };
                return drawable;
            }
        }
    }
}