// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public partial class CollectionButton : OsuAnimatedButton, IHasPopover
    {
        private readonly Box background;

        private readonly BeatmapInfo beatmapInfo;

        public CollectionButton(BeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;

            Size = new Vector2(50, 30);

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue
                },
                new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(13),
                    Icon = FontAwesome.Solid.Book,
                },
            };

            TooltipText = "collections";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Green;

            Action = this.ShowPopover;
        }

        // use Content for tracking input as some buttons might be temporarily hidden with DisappearToBottom, and they become hidden by moving Content away from screen.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Content.ReceivePositionalInputAt(screenSpacePos);

        public Popover GetPopover() => new CollectionPopover(beatmapInfo);
    }
}
