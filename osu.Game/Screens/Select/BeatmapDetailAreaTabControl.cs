// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetailAreaTabControl : Container
    {
        public static readonly float HEIGHT = 24;
        private OsuTabControlCheckBox modsCheckbox;
        private OsuTabControl<BeatmapDetailTab> tabs;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            modsCheckbox.AccentColour = tabs.AccentColour = colour.YellowLight;
        }

        public BeatmapDetailAreaTabControl()
        {
            Height = HEIGHT;

            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Colour = Color4.White.Opacity(0.2f),
                },
                tabs = new OsuTabControl<BeatmapDetailTab>
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                },
                modsCheckbox = new OsuTabControlCheckBox
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Text = @"Mods",
                },
            };

            tabs.ItemChanged += (sender, e) =>
            {
                
            };

            modsCheckbox.Action += (sender, e) =>
            {
                
            };
        }
    }

    public enum BeatmapDetailTab
    {
        Details,
        Local,
        Country,
        Global,
        Friends
    }
}
