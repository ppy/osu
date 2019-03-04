// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderEditorInfo
    {
        public readonly Bindable<MatchPairing> Selected = new Bindable<MatchPairing>();
    }
}
