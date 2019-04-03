// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Desktop.Overlays
{
    public class VersionManager : OverlayContainer
    {
        private OsuConfigManager config;
        private OsuGameBase game;
        private NotificationOverlay notificationOverlay;
        private GameHost host;

        [BackgroundDependencyLoader]
        private void load(NotificationOverlay notification, OsuColour colours, TextureStore textures, OsuGameBase game, OsuConfigManager config, GameHost host)
        {
            notificationOverlay = notification;
            this.config = config;
            this.game = game;
            this.host = host;

            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Alpha = 0;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new Vector2(5),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold),
                                    Text = game.Name
                                },
                                new OsuSpriteText
                                {
                                    Colour = DebugUtils.IsDebug ? colours.Red : Color4.White,
                                    Text = game.Version
                                },
                            }
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = OsuFont.Numeric.With(size: 12),
                            Colour = colours.Yellow,
                            Text = @"Development Build"
                        },
                        new Sprite
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Texture = textures.Get(@"Menu/dev-build-footer"),
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var version = game.Version;
            var lastVersion = config.Get<string>(OsuSetting.Version);
            if (game.IsDeployedBuild && version != lastVersion)
            {
                config.Set(OsuSetting.Version, version);

                // only show a notification if we've previously saved a version to the config file (ie. not the first run).
                if (!string.IsNullOrEmpty(lastVersion))
                    notificationOverlay.Post(new UpdateCompleteNotification(version, host.OpenUrlExternally));
            }
        }

        private class UpdateCompleteNotification : SimpleNotification
        {
            public UpdateCompleteNotification(string version, Action<string> openUrl = null)
            {
                Text = $"You are now running osu!lazer {version}.\nClick to see what's new!";
                Icon = FontAwesome.Solid.CheckSquare;
                Activated = delegate
                {
                    openUrl?.Invoke($"https://osu.ppy.sh/home/changelog/lazer/{version}");
                    return true;
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconBackgound.Colour = colours.BlueDark;
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(1400, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(500, Easing.OutQuint);
        }
    }
}
