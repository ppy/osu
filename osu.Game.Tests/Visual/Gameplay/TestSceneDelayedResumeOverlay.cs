// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Gameplay;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneDelayedResumeOverlay : OsuTestScene
    {
        private ResumeOverlay resume = null!;
        private bool resumeFired;

        [Cached]
        private GameplayState gameplayState;

        public TestSceneDelayedResumeOverlay()
        {
            gameplayState = TestGameplayState.Create(new OsuRuleset());
        }

        [SetUp]
        public void SetUp() => Schedule(loadContent);

        [Test]
        public void TestResume()
        {
            AddStep("show", () => resume.Show());
            AddUntilStep("dismissed", () => resumeFired && resume.State.Value == Visibility.Hidden);
        }

        private void loadContent()
        {
            Child = resume = new DelayedResumeOverlay();

            resumeFired = false;
            resume.ResumeAction = () => resumeFired = true;
        }
    }
}
