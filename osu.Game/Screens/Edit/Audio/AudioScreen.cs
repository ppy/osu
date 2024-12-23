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
        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        public AudioScreen()
            : base(EditorScreenMode.Audio)
        {
        }

        protected override Drawable CreateTimelineContent()
        {
            Ruleset ruleset = workingBeatmap.Value.BeatmapInfo.Ruleset.CreateInstance();
            HitObjectComposer? composer = ruleset?.CreateHitObjectComposer();

            if (composer == null || ruleset == null)
                return base.CreateTimelineContent();

            return new EditorSkinProvidingContainer(EditorBeatmap).WithChild(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Alpha = 0,
                        Child = composer,
                    },
                    new TimelineBlueprintContainer(composer),
                }
            });
        }
        protected override Drawable CreateMainContent()
        {
            return new HitSoundTable();
        }
    }
}
