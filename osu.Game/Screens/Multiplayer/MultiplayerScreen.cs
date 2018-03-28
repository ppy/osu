// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Screens.Multiplayer
{
    public abstract class MultiplayerScreen : OsuScreen
    {
        public abstract string Title { get; }
        public abstract string Name { get; }

        public override string ToString() => Name;
    }
}
