// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
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

            List<SetupSection> sectionsEnumerable =
            [
                new ResourcesSection(),
                new MetadataSection()
            ];

            sectionsEnumerable.AddRange(ruleset.CreateEditorSetupSections());
            sectionsEnumerable.Add(new DesignSection());

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

        public override void OnExiting(ScreenExitEvent e)
        {
            base.OnExiting(e);

            // Before exiting, trigger a focus loss.
            //
            // This is important to ensure that if the user is still editing a textbox, it will commit
            // (and potentially block the exit procedure for save).
            GetContainingFocusManager()?.TriggerFocusContention(this);
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
