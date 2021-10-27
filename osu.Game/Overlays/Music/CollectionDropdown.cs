// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Game.Collections;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Music
{
    /// <summary>
    /// A <see cref="CollectionFilterDropdown"/> for use in the <see cref="NowPlayingOverlay"/>.
    /// </summary>
    public class CollectionDropdown : CollectionFilterDropdown
    {
        protected override bool ShowManageCollectionsItem => false;

        protected override CollectionDropdownHeader CreateCollectionHeader() => new CollectionsHeader();

        protected override CollectionDropdownMenu CreateCollectionMenu() => new CollectionsMenu();

        private class CollectionsMenu : CollectionDropdownMenu
        {
            public CollectionsMenu()
            {
                Masking = true;
                CornerRadius = 5;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Gray4;
                SelectionColour = colours.Gray5;
                HoverColour = colours.Gray6;
            }
        }

        private class CollectionsHeader : CollectionDropdownHeader
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                BackgroundColour = colours.Gray4;
                BackgroundColourHover = colours.Gray6;
            }

            public CollectionsHeader()
            {
                CornerRadius = 5;
                Height = 30;
                Icon.Size = new Vector2(14);
                Icon.Margin = new MarginPadding(0);
                Foreground.Padding = new MarginPadding { Top = 4, Bottom = 4, Left = 10, Right = 10 };
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(0.3f),
                    Radius = 3,
                    Offset = new Vector2(0f, 1f),
                };
            }
        }
    }
}
