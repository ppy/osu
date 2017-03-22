// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Overlays.Options.Sections
{
    public class MaintenanceSection : OptionsSection
    {
        public override string Header => "Maintenance";
        public override FontAwesome Icon => FontAwesome.fa_wrench;

        public MaintenanceSection()
        {
            FlowContent.Spacing = new Vector2(0, 5);
            Children = new Drawable[]
            {
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Delete all unranked maps",
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Repair folder permissions",
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Mark all maps as played",
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Run osu! updater",
                },
            };
        }
    }
}