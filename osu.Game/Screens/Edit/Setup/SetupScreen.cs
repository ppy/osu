// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Setup
{
    public class SetupScreen : EditorScreen
    {
        [Resolved]
        private OsuColour colours { get; set; }

        [Cached]
        protected readonly OverlayColourProvider ColourProvider;

        public SetupScreen()
            : base(EditorScreenMode.SongSetup)
        {
            ColourProvider = new OverlayColourProvider(OverlayColourScheme.Green);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(50),
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 10,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeafoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new SectionsContainer<SetupSection>
                        {
                            FixedHeader = new SetupScreenHeader(),
                            RelativeSizeAxes = Axes.Both,
                            Children = new SetupSection[]
                            {
                                new ResourcesSection(),
                                new MetadataSection(),
                                new DifficultySection(),
                            }
                        },
                    }
                }
            };
        }
    }

    internal class SetupScreenHeader : OverlayHeader
    {
        protected override OverlayTitle CreateTitle() => new SetupScreenTitle();

        private class SetupScreenTitle : OverlayTitle
        {
            public SetupScreenTitle()
            {
                Title = "beatmap setup";
                Description = "change general settings of your beatmap";
                IconTexture = "Icons/Hexacons/social";
            }
        }
    }
}
