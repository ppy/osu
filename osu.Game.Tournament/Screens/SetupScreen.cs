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
        private ActionableInfoWithNumberBox resolution;

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
                    Label = "Current IPC source",
                    ButtonText = "Refresh",
                    Action = () =>
                    {
                        fileBasedIpc?.LocateStableStorage();
                        reload();
                    },
                    Value = fileBasedIpc?.Storage?.GetFullPath(string.Empty) ?? "Not found",
                    Failing = fileBasedIpc?.Storage == null,
                    Description = "The osu!stable installation which is currently being used as a data source. If a source is not found, make sure you have created an empty ipc.txt in your stable cutting-edge installation, and that it is registered as the default osu! install."
                },
                new ActionableInfo
                {
                    Label = "Current User",
                    ButtonText = "Change Login",
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
                    Description = "In order to access the API and display metadata, a login is required."
                },
                new LabelledDropdown<RulesetInfo>
                {
                    Label = "Ruleset",
                    Description = "Decides what stats are displayed and which ranks are retrieved for players",
                    Items = rulesets.AvailableRulesets,
                    Current = LadderInfo.Ruleset,
                },
                resolution = new ActionableInfoWithNumberBox
                {
                    Label = "Stream area resolution",
                    ButtonText = "Set size",
                    Action = i =>
                    {
                        i = Math.Clamp(i, 480, 2160);
                        windowSize.Value = new Size((int)(i * aspect_ratio / TournamentSceneManager.STREAM_AREA_WIDTH * TournamentSceneManager.REQUIRED_WIDTH), i);
                        resolution.NumberValue = i;
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
            protected OsuButton Button;

            public ActionableInfo()
                : base(true)
            {
            }

            public string ButtonText
            {
                set => Button.Text = value;
            }

            public string Value
            {
                set => ValueText.Text = value;
            }

            public bool Failing
            {
                set => ValueText.Colour = value ? Color4.Red : Color4.White;
            }

            public Action Action;

            protected TournamentSpriteText ValueText;

            protected override Drawable CreateComponent() => new Container
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    ValueText = new TournamentSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    Button = new TriangleButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(100, 30),
                        Action = () => Action?.Invoke()
                    },
                }
            };
        }

        private class ActionableInfoWithNumberBox : ActionableInfo
        {
            public new Action<int> Action;

            private OsuNumberBox numberBox;

            public int NumberValue
            {
                get
                {
                    int.TryParse(numberBox.Text, out var val);
                    return val;
                }
                set => numberBox.Text = value.ToString();
            }

            protected override Drawable CreateComponent() => new Container
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Children = new Drawable[]
                {
                    ValueText = new TournamentSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    numberBox = new OsuNumberBox
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Width = 100,
                        Margin = new MarginPadding
                        {
                            Right = 110
                        }
                    },
                    Button = new TriangleButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(100, 30),
                        Action = () =>
                        {
                            if (numberBox.Text.Length > 0) Action?.Invoke(NumberValue);
                        }
                    },
                }
            };
        }
    }
}
