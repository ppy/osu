// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Screens.Edit.Submission;
using osu.Game.Screens.Footer;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneBeatmapSubmissionOverlay : OsuTestScene
    {
        private ScreenFooter footer = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("add overlay", () =>
            {
                var receptor = new ScreenFooter.BackReceptor();
                footer = new ScreenFooter(receptor);

                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new[]
                    {
                        (typeof(ScreenFooter), (object)footer),
                        (typeof(BeatmapSubmissionSettings), new BeatmapSubmissionSettings()),
                    },
                    Children = new Drawable[]
                    {
                        receptor,
                        new BeatmapSubmissionOverlay
                        {
                            State = { Value = Visibility.Visible, },
                        },
                        footer,
                    }
                };
            });
        }
    }
}
