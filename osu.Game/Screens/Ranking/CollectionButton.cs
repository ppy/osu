// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class CollectionButton : GrayButton, IHasPopover
    {
        private readonly BeatmapInfo beatmapInfo;

        public CollectionButton(BeatmapInfo beatmapInfo)
            : base(FontAwesome.Solid.Book)
        {
            this.beatmapInfo = beatmapInfo;

            Size = new Vector2(75, 30);

            TooltipText = "collections";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Background.Colour = colours.Green;

            Action = this.ShowPopover;
        }

        // use Content for tracking input as some buttons might be temporarily hidden with DisappearToBottom, and they become hidden by moving Content away from screen.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Content.ReceivePositionalInputAt(screenSpacePos);

        public Popover GetPopover() => new CollectionPopover(beatmapInfo);
    }
}
