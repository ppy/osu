// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderEditorInfo
    {
        public readonly BindableBool EditingEnabled = new BindableBool();
        public readonly Bindable<MatchPairing> Selected = new Bindable<MatchPairing>();
    }
}
