using System;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class MaintenanceOptions : OptionsSection
    {
        protected override string Header => "Maintenance";
        public override FontAwesome Icon => FontAwesome.fa_wrench;
    
        public MaintenanceOptions()
        {
            content.Spacing = new Vector2(0, 5);
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
            };
        }
    }
}