// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Music;
using System.Collections.Generic;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class CollectionSettings : ReplayGroup
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
