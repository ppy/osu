// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multi.Components
{
    public class DrawableGameType : CircularContainer, IHasTooltip
    {
        private readonly GameType type;

        public string TooltipText => type.Name;

        public DrawableGameType(GameType type)
        {
            this.type = type;
            Masking = true;

            Children = new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex(@"545454"),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Add(type.GetIcon(colours, Height / 2));
        }
    }
}
