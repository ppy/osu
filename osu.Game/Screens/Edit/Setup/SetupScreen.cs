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
                sections = new SetupScreenSectionsContainer
                {
                    FixedHeader = header,
                    RelativeSizeAxes = Axes.Both,
                    Children = new SetupSection[]
                    {
                        new ResourcesSection(),
                        new MetadataSection(),
                        new DifficultySection(),
                        new ColoursSection(),
                        new DesignSection(),
                    }
                },
            });
        }

        private class SetupScreenSectionsContainer : SectionsContainer<SetupSection>
        {
            protected override UserTrackingScrollContainer CreateScrollContainer()
            {
                var scrollContainer = base.CreateScrollContainer();

                // Workaround for masking issues (see https://github.com/ppy/osu-framework/issues/1675#issuecomment-910023157)
                // Note that this actually causes the full scroll range to be reduced by 2px at the bottom, but it's not really noticeable.
                scrollContainer.Margin = new MarginPadding { Top = 2 };

                return scrollContainer;
            }
        }
    }
}
