// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class HostInfo : CompositeDrawable
    {
        public HostInfo(Room room)
        {
            AutoSizeAxes = Axes.X;
            Height = 50;

            LinkFlowContainer linkContainer;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    new UpdateableAvatar
                    {
                        Size = new Vector2(50),
                        User = room.Host.Value
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            linkContainer = new LinkFlowContainer { AutoSizeAxes = Axes.Both }
                        }
                    }
                }
            };

            linkContainer.AddText("hosted by");
            linkContainer.NewLine();
            linkContainer.AddLink(room.Host.Value.Username,null, LinkAction.OpenUserProfile, room.Host.Value.Id.ToString(), "Open profile", s => s.Font = "Exo2.0-BoldItalic");
        }
    }
}
