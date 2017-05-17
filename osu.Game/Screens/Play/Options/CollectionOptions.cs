// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Music;
using System.Collections.Generic;

namespace osu.Game.Screens.Play.Options
{
    public class CollectionOptions : OptionContainer
    {
        public override string Title => @"COLLECTIONS";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Add(new OsuSpriteText
            {
                Text = @"Add current song to",
            });
            Add(new CollectionsDropdown<PlaylistCollection>
            {
                RelativeSizeAxes = Axes.X,
                Items = new[] { new KeyValuePair<string, PlaylistCollection>(@"All", PlaylistCollection.All) },
            });
        }
    }
}
