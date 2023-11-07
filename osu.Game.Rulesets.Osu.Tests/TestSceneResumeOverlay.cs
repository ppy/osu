// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play;
using osu.Game.Tests.Gameplay;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneResumeOverlay : OsuManualInputManagerTestScene
    {
        private ManualOsuInputManager osuInputManager = null!;
        private CursorContainer cursor = null!;
        private ResumeOverlay resume = null!;

        private bool resumeFired;

        private OsuConfigManager localConfig = null!;

        [Cached]
        private GameplayState gameplayState;

        public TestSceneResumeOverlay()
        {
            gameplayState = TestGameplayState.Create(new OsuRuleset());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(localConfig = new OsuConfigManager(LocalStorage));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddSliderStep("cursor size", 0.1f, 2f, 1f, v => localConfig.SetValue(OsuSetting.GameplayCursorSize, v));
            AddSliderStep("circle size", 0f, 10f, 0f, val =>
            {
                gameplayState.Beatmap.Difficulty.CircleSize = val;
                SetUp();
            });

            AddToggleStep("auto size", v => localConfig.SetValue(OsuSetting.AutoCursorSize, v));
        }

        [SetUp]
        public void SetUp() => Schedule(loadContent);

        [TestCase(1)]
        [TestCase(0.5f)]
        [TestCase(2)]
        public void TestResume(float cursorSize)
        {
            AddStep($"set cursor size to {cursorSize}", () => localConfig.SetValue(OsuSetting.GameplayCursorSize, cursorSize));

            AddStep("move mouse to center", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.Centre));
            AddStep("show", () => resume.Show());

            AddStep("move mouse away", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.TopLeft));
            AddStep("click", () => osuInputManager.GameClick());
            AddAssert("not dismissed", () => !resumeFired && resume.State.Value == Visibility.Visible);

            AddStep("move mouse just out of range", () =>
            {
                var resumeOverlay = this.ChildrenOfType<OsuResumeOverlay>().Single();
                var resumeOverlayCursor = resumeOverlay.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single();

                Vector2 offset = resumeOverlay.ToScreenSpace(new Vector2(OsuCursor.SIZE / 2)) - resumeOverlay.ToScreenSpace(Vector2.Zero);
                InputManager.MoveMouseTo(resumeOverlayCursor.ScreenSpaceDrawQuad.Centre - offset - new Vector2(1));
            });

            AddStep("click", () => osuInputManager.GameClick());
            AddAssert("not dismissed", () => !resumeFired && resume.State.Value == Visibility.Visible);

            AddStep("move mouse just within range", () =>
            {
                var resumeOverlay = this.ChildrenOfType<OsuResumeOverlay>().Single();
                var resumeOverlayCursor = resumeOverlay.ChildrenOfType<OsuResumeOverlay.OsuClickToResumeCursor>().Single();

                Vector2 offset = resumeOverlay.ToScreenSpace(new Vector2(OsuCursor.SIZE / 2)) - resumeOverlay.ToScreenSpace(Vector2.Zero);
                InputManager.MoveMouseTo(resumeOverlayCursor.ScreenSpaceDrawQuad.Centre - offset + new Vector2(1));
            });

            AddStep("click", () => osuInputManager.GameClick());
            AddAssert("dismissed", () => resumeFired && resume.State.Value == Visibility.Hidden);
        }

        private void loadContent()
        {
            Child = osuInputManager = new ManualOsuInputManager(new OsuRuleset().RulesetInfo) { Children = new Drawable[] { cursor = new CursorContainer(), resume = new OsuResumeOverlay { GameplayCursor = cursor }, } };

            resumeFired = false;
            resume.ResumeAction = () => resumeFired = true;
        }

        private partial class ManualOsuInputManager : OsuInputManager
        {
            public ManualOsuInputManager(RulesetInfo ruleset)
                : base(ruleset)
            {
            }

            public void GameClick()
            {
                KeyBindingContainer.TriggerPressed(OsuAction.LeftButton);
                KeyBindingContainer.TriggerReleased(OsuAction.LeftButton);
            }
        }
    }
}
