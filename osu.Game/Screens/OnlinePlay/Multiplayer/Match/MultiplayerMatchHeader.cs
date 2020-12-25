// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Users.Drawables;
using osuTK;
using FontWeight = osu.Game.Graphics.FontWeight;
using OsuColour = osu.Game.Graphics.OsuColour;
using OsuFont = osu.Game.Graphics.OsuFont;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match
{
    public class MultiplayerMatchHeader : OnlinePlayComposite
    {
        public const float HEIGHT = 50;

        public Action OpenSettings;

        private UpdateableAvatar avatar;
        private LinkFlowContainer hostText;
        private Button openSettingsButton;

        [Resolved]
        private IAPIProvider api { get; set; }

        public MultiplayerMatchHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        avatar = new UpdateableAvatar
                        {
                            Size = new Vector2(50),
                            Masking = true,
                            CornerRadius = 10,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Font = OsuFont.GetFont(size: 30),
                                    Current = { BindTarget = RoomName }
                                },
                                hostText = new LinkFlowContainer(s => s.Font = OsuFont.GetFont(size: 20))
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                }
                            }
                        }
                    }
                },
                openSettingsButton = new PurpleTriangleButton
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Size = new Vector2(150, HEIGHT),
                    Text = "Open settings",
                    Action = () => OpenSettings?.Invoke(),
                    Alpha = 0
                }
            };

            Host.BindValueChanged(host =>
            {
                avatar.User = host.NewValue;

                hostText.Clear();

                if (host.NewValue != null)
                {
                    hostText.AddText("hosted by ");
                    hostText.AddUserLink(host.NewValue, s => s.Font = s.Font.With(weight: FontWeight.SemiBold));
                }

                openSettingsButton.Alpha = host.NewValue?.Equals(api.LocalUser.Value) == true ? 1 : 0;
            }, true);
        }
    }
}
