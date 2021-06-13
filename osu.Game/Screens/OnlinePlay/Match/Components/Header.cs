// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Match.Components
{
    public class Header : OnlinePlayComposite
    {
        public const float HEIGHT = 50;

        private UpdateableAvatar avatar;
        private LinkFlowContainer hostText;

        public Header()
        {
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new FillFlowContainer
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
            }, true);
        }
    }
}
