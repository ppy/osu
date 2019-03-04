// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Tournament.Screens
{
    public class TournamentScreen : CompositeDrawable
    {
        [Resolved]
        protected LadderInfo LadderInfo { get; private set; }

        public TournamentScreen()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Hide()
        {
            this.FadeOut(200);
        }

        public override void Show()
        {
            this.FadeIn(200);
        }
    }
}
