// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneResumeOverlay : OsuManualInputManagerTestScene
    {
        private ManualOsuInputManager osuInputManager = null!;
        private CursorContainer cursor = null!;
        private ResumeOverlay resume = null!;

        private bool resumeFired;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = osuInputManager = new ManualOsuInputManager(new OsuRuleset().RulesetInfo)
            {
                Children = new Drawable[]
                {
                    cursor = new CursorContainer(),
                    resume = new OsuResumeOverlay
                    {
                        GameplayCursor = cursor
                    },
                }
            };

            resumeFired = false;
            resume.ResumeAction = () => resumeFired = true;
        });

        [Test]
        public void TestResume()
        {
            AddStep("move mouse to center", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.Centre));
            AddStep("show", () => resume.Show());

            AddStep("move mouse away", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.TopLeft));
            AddStep("click", () => osuInputManager.GameClick());
            AddAssert("not dismissed", () => !resumeFired && resume.State.Value == Visibility.Visible);

            AddStep("move mouse back", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.Centre));
            AddStep("click", () => osuInputManager.GameClick());
            AddAssert("dismissed", () => resumeFired && resume.State.Value == Visibility.Hidden);
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
