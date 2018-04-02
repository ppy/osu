// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Desktop.Overlays
{
    public class VersionManager : OverlayContainer
    {
        private OsuConfigManager config;
        private OsuGameBase game;
        private NotificationOverlay notificationOverlay;

        public override bool HandleKeyboardInput => false;
        public override bool HandleMouseInput => false;

        [BackgroundDependencyLoader]
        private void load(NotificationOverlay notification, OsuColour colours, TextureStore textures, OsuGameBase game, OsuConfigManager config)
        {
            notificationOverlay = notification;
            this.config = config;
            this.game = game;

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
                                    Font = @"Exo2.0-Bold",
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
                            TextSize = 12,
                            Colour = colours.Yellow,
                            Font = @"Venera",
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

#if NET_FRAMEWORK
            Add(new SquirrelUpdateManager());
#endif
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
                    notificationOverlay.Post(new UpdateCompleteNotification(version));
            }
        }

        private class UpdateCompleteNotification : SimpleNotification
        {
            public UpdateCompleteNotification(string version)
            {
                Text = $"You are now running osu!lazer {version}.\nClick to see what's new!";
                Icon = FontAwesome.fa_check_square;
                Activated = delegate
                {
                    Process.Start($"https://github.com/ppy/osu/releases/tag/v{version}");
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
            this.FadeIn(1000);
        }

        protected override void PopOut()
        {
        }
    }
}
