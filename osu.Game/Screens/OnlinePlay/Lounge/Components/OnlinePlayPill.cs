// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class OnlinePlayPill : OnlinePlayComposite
    {
        protected PillContainer pill;
        protected OsuTextFlowContainer textFlow;

        public OnlinePlayPill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = pill = new PillContainer
            {
                Child = textFlow = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12))
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft
                }
            };
        }
    }
}
