// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.Match.Components
{
    public class OverlinedChatDisplay : OverlinedDisplay
    {
        public OverlinedChatDisplay()
            : base("Chat")
        {
            Content.Add(new MatchChatDisplay
            {
                RelativeSizeAxes = Axes.Both
            });
        }
    }
}
