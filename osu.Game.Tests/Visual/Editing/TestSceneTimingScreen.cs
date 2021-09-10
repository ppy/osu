// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneTimingScreen : EditorClockTestScene
    {
        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        private readonly EditorBeatmap editorBeatmap;

        protected override bool ScrollUsingMouseWheel => false;

        public TestSceneTimingScreen()
        {
            editorBeatmap = new EditorBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(editorBeatmap.PlayableBeatmap);
            Beatmap.Disabled = true;

            Child = new TimingScreen
            {
                State = { Value = Visibility.Visible },
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            Beatmap.Disabled = false;
            base.Dispose(isDisposing);
        }
    }
}
