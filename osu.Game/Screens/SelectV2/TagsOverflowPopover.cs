// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.SelectV2
{
    public partial class TagsOverflowPopover : OsuPopover
    {
        private readonly string[] tags;
        private readonly SongSelect? songSelect;

        public TagsOverflowPopover(string[] tags, SongSelect? songSelect)
        {
            this.tags = tags;
            this.songSelect = songSelect;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LinkFlowContainer textFlow;

            Child = textFlow = new LinkFlowContainer(t =>
            {
                t.Font = t.Font.With(size: 14.4f, weight: FontWeight.Regular);
            })
            {
                Width = 200,
                AutoSizeAxes = Axes.Y,
            };

            foreach (string tag in tags)
            {
                textFlow.AddLink(tag, () => songSelect?.Search(tag));
                textFlow.AddText(" ");
            }
        }
    }
}
