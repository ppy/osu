// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class AudioScreen : EditorScreenWithTimeline
    {
        public AudioScreen()
            : base(EditorScreenMode.Audio)
        {
        }

        private HitObjectComposer? composer;
        private Ruleset ruleset = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            ruleset = parent.Get<IBindable<WorkingBeatmap>>().Value.BeatmapInfo.Ruleset.CreateInstance();
            composer = ruleset.CreateHitObjectComposer();

            // make the composer available to the timeline and other components in this screen.
            if (composer != null)
                dependencies.CacheAs(composer);

            return dependencies;
        }

        protected override Drawable CreateTimelineContent()
        {
            if (composer == null)
                return base.CreateTimelineContent();

            return new EditorSkinProvidingContainer(EditorBeatmap).WithChild(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TimelineBlueprintContainer(composer),
            });
        }

        protected override Drawable CreateMainContent()
        {
            if (composer == null)
                return new ScreenWhiteBox.UnderConstructionMessage("Audio mode");

            return new EditorSkinProvidingContainer(EditorBeatmap).WithChild(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Width = 1,
                        Height = 1,
                        Child = composer,
                    },
                    new HitSoundTrackTable(),
                }
            });
        }
    }
}
