// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Graphics;
using osu.Game.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Game.Skinning;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Background
{
    public class TestSceneMainMenuBackground : OsuTestScene
    {
        public TestSceneMainMenuBackground()
        {
            TestBackgroundScreen background;

            Child = new OsuScreenStack(background = new TestBackgroundScreen())
            {
                RelativeSizeAxes = Axes.Both,
            };

            AddStep("Default Mode", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Default);
            AddStep("Skin Mode (skin with no bg)", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Skin);
            AddStep("Beatmap Mode", () => background.BackgroundMode.Value = MainMenuBackgroundMode.Beatmap);
            AddStep("Change user(not supporter)", () => background.User.Value = new User
            {
                Id = -1,
                IsSupporter = false
            });
            AddStep("Change user(supporter)", () => background.User.Value = new User
            {
                Id = -2,
                IsSupporter = true
            });
        }

        private class TestBackgroundScreen : BackgroundScreenDefault
        {
            private OsuSpriteText userState;
            private OsuSpriteText modeState;
            private OsuSpriteText skinState;
            private OsuSpriteText updateNotification;
            private SettingsDropdown<SkinInfo> skinDropdown;

            public TestBackgroundScreen()
            {
                var mc = new MusicController
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre
                };

                AddInternal(mc);

                mc.Show();
            }

            [BackgroundDependencyLoader]
            private void load(SkinManager skins)
            {
                AddRangeInternal(new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Y,
                        Width = 300,
                        Margin = new MarginPadding { Top = 50, Right = 50 },
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                            },
                            skinDropdown = new SettingsDropdown<SkinInfo>
                            {
                                Bindable = new Bindable<SkinInfo>()
                            }
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Margin = new MarginPadding { Left = 100 },
                        Children = new Drawable[]
                        {
                            userState = new OsuSpriteText(),
                            modeState = new OsuSpriteText(),
                            skinState = new OsuSpriteText(),
                        }
                    },
                    updateNotification = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Margin = new MarginPadding { Bottom = 100 },
                        Text = "Background has been updated",
                        Colour = Color4.Red,
                        Font = OsuFont.GetFont(size: 40),
                        Alpha = 0,
                    }
                });

                skinDropdown.Items = skins.GetAllUsableSkins().ToArray();
                skinDropdown.Bindable.BindValueChanged(skin => skins.CurrentSkinInfo.Value = skin.NewValue);

                User.BindValueChanged(user => updateUserState(user.NewValue), true);
                Skin.BindValueChanged(_ => updateSkinState(), true);
                BackgroundMode.BindValueChanged(mode => updateModeState(mode.NewValue), true);
            }

            private void updateUserState(User user) => userState.Text = "user state: " + ((user?.IsSupporter ?? false) ? "supporter" : "unSupporter");

            private void updateModeState(MainMenuBackgroundMode mode) => modeState.Text = $"mode state: {mode}";

            private void updateSkinState() => skinState.Text = $"skin has background: {SkinHasBackground}";

            protected override void UpdateBackground()
            {
                base.UpdateBackground();
                updateNotification.FadeIn().Then().Delay(500).Then().FadeOut(500);
            }
        }
    }
}
