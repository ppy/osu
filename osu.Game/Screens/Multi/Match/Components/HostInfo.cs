// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class HostInfo : CompositeDrawable
    {
        public readonly IBindable<User> Host = new Bindable<User>();

        private readonly LinkFlowContainer linkContainer;
        private readonly UpdateableAvatar avatar;

        public HostInfo()
        {
            AutoSizeAxes = Axes.X;
            Height = 50;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    avatar = new UpdateableAvatar { Size = new Vector2(50) },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Child = linkContainer = new LinkFlowContainer { AutoSizeAxes = Axes.Both }
                    }
                }
            };

            Host.BindValueChanged(host => updateHost(host.NewValue));
        }

        private void updateHost(User host)
        {
            avatar.User = host;

            if (host != null)
            {
                linkContainer.AddText("hosted by");
                linkContainer.NewLine();
                linkContainer.AddUserLink(host, s => s.Font = s.Font.With(Typeface.Exo, weight: FontWeight.Bold, italics: true));
            }
        }
    }
}
