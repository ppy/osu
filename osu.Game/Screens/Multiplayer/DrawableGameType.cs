// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Screens.Multiplayer
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
                    Colour = OsuColour.FromHex(@"545454"),
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
