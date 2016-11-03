using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class MaintenanceOptions : OptionsSection
    {
        public MaintenanceOptions()
        {
            Header = "Maintenance";
            Children = new Drawable[]
            {
                new OptionsSubsection
                {
                    Header = "General",
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
                        new SpriteText
                        {
                            Text = "TODO: osu version here",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                        },
                    }
                }
            };
        }
    }
}