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
        public const int HORIZONTAL_PADDING = 100;

        [Resolved]
        private OsuColour colours { get; set; }

        [Cached]
        protected readonly OverlayColourProvider ColourProvider;

        [Cached]
        private SectionsContainer<SetupSection> sections = new SectionsContainer<SetupSection>();

        [Cached]
        private SetupScreenHeader header = new SetupScreenHeader();

        public SetupScreen()
            : base(EditorScreenMode.SongSetup)
        {
            ColourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);
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
                            Colour = ColourProvider.Dark4,
                            RelativeSizeAxes = Axes.Both,
                        },
                        sections = new SectionsContainer<SetupSection>
                        {
                            FixedHeader = header,
                            RelativeSizeAxes = Axes.Both,
                            Children = new SetupSection[]
                            {
                                new ResourcesSection(),
                                new MetadataSection(),
                                new DifficultySection(),
                                new ColoursSection()
                            }
                        },
                    }
                }
            };
        }
    }
}
