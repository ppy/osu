// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Users
{
    public class Medal
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
        public string ImageUrl => $@"https://s.ppy.sh/images/medals-client/{InternalName}@2x.png";
        public string Description { get; set; }
    }
}
