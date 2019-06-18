// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    public class LadderEditorInfo
    {
        public readonly Bindable<TournamentMatch> Selected = new Bindable<TournamentMatch>();
    }
}
