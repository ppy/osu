// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Setup
{
    public class SetupScreen : EditorRoundedScreen
    {
        [Cached]
        private SectionsContainer<SetupSection> sections = new SectionsContainer<SetupSection>();

        [Cached]
        private SetupScreenHeader header = new SetupScreenHeader();

        public SetupScreen()
            : base(EditorScreenMode.SongSetup)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(new Drawable[]
            {
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
            });
        }
    }
}
