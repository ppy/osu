// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2
{
    public partial class MetadataLinkContainer : OsuHoverContainer, IHasContextMenu
    {
        private readonly TruncatingSpriteText text;

        public float MaxWidth
        {
            set => text.MaxWidth = value;
        }

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        public FontUsage Font
        {
            set => text.Font = value;
        }

        public MetadataLinkContainer()
        {
            AutoSizeAxes = Axes.Both;

            Child = text = new TruncatingSpriteText();
        }

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, ISongSelect? songSelect, LocalisationManager localisations)
        {
            IdleColour ??= colourProvider.Light2;
            Action = () => songSelect?.Search(localisations.GetLocalisedString(text.Text));
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem(ContextMenuStrings.SearchOnline, MenuItemType.Standard, () => linkHandler?.HandleLink(new LinkDetails(LinkAction.SearchBeatmapSet, text.Text)))
        };
    }
}
