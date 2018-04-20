// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Music;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class CollectionSettings : PlayerSettingsGroup
    {
        protected override string Title => @"collections";

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = @"Add current song to",
                },
                new CollectionsDropdown<PlaylistCollection>
                {
                    RelativeSizeAxes = Axes.X,
                    Items = new[] { new KeyValuePair<string, PlaylistCollection>(@"All", PlaylistCollection.All) },
                },
            };
        }
    }
}
