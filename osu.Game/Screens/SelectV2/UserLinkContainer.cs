// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Users;

namespace osu.Game.Screens.SelectV2
{
    public partial class UserLinkContainer : OsuHoverContainer, IHasContextMenu
    {
        private readonly TruncatingSpriteText text;

        public float MaxWidth
        {
            set => text.MaxWidth = value;
        }

        public FontUsage Font
        {
            set => text.Font = value;
        }

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        private IUser? user;

        public IUser? User
        {
            get => user;
            set
            {
                user = value;
                text.Text = user?.Username ?? string.Empty;
            }
        }

        public UserLinkContainer()
        {
            AutoSizeAxes = Axes.Both;

            Child = text = new TruncatingSpriteText
            {
                Shadow = true,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours, ISongSelect? songSelect)
        {
            IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
            Action = () => songSelect?.Search(user!.Username);
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(ContextMenuStrings.ViewProfile, MenuItemType.Standard, () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, user!))),
            new OsuMenuItem(ContextMenuStrings.SearchOnline, MenuItemType.Standard, () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, user!))),
        };
    }
}
