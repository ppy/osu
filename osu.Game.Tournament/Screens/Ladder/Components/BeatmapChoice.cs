// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class BeatmapChoice
    {
        public TeamColour Team;
        public ChoiceType Type;
        public int BeatmapID;
    }

    public enum TeamColour
    {
        Red,
        Blue
    }

    public enum ChoiceType
    {
        Pick,
        Ban,
    }
}
