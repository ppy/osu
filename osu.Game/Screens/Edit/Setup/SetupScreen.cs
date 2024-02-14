// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Setup
{
    public partial class SetupScreen : EditorScreen
    {
        [Cached]
        private SectionsContainer<SetupSection> sections { get; } = new SetupScreenSectionsContainer();

        [Cached]
        private SetupScreenHeader header = new SetupScreenHeader();

        public SetupScreen()
            : base(EditorScreenMode.SongSetup)
        {
        }

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap beatmap, OverlayColourProvider colourProvider)
        {
            var ruleset = beatmap.BeatmapInfo.Ruleset.CreateInstance();

            var sectionsEnumerable = new List<SetupSection>
            {
                new ResourcesSection(),
                new MetadataSection(),
                ruleset.CreateEditorDifficultySection() ?? new DifficultySection(),
                new ColoursSection(),
                new DesignSection(),
            };

            var rulesetSpecificSection = ruleset.CreateEditorSetupSection();
            if (rulesetSpecificSection != null)
                sectionsEnumerable.Add(rulesetSpecificSection);

            Add(new Box
            {
                Colour = colourProvider.Background3,
                RelativeSizeAxes = Axes.Both,
            });

            Add(sections.With(s =>
            {
                s.RelativeSizeAxes = Axes.Both;
                s.ChildrenEnumerable = sectionsEnumerable;
                s.FixedHeader = header;
            }));
        }

        private partial class SetupScreenSectionsContainer : SectionsContainer<SetupSection>
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
