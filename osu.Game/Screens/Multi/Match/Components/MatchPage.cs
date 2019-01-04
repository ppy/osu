// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;

namespace osu.Game.Screens.Multi.Match.Components
{
    public abstract class MatchPage
    {
        public abstract string Name { get; }

        public readonly BindableBool Enabled = new BindableBool(true);

        public override string ToString() => Name;
        public override int GetHashCode() => GetType().GetHashCode();
        public override bool Equals(object obj) => GetType() == obj?.GetType();
    }

    public class SettingsMatchPage : MatchPage
    {
        public override string Name => "Settings";
    }

    public class RoomMatchPage : MatchPage
    {
        public override string Name => "Room";
    }
}
