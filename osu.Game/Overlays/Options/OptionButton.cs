// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    class OptionButton : OsuButton, ISearchable
    {
        public string[] Keywords => new[] { Text };
        public bool LastMatch { get; set; }
    }
}
