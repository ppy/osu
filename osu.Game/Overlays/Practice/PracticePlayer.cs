// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    public class PracticePlayer : Player
    {
        public PracticeOverlay PracticeOverlay = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(PracticeOverlay = new PracticeOverlay());
        }
    }
}
