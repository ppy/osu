// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneResumeOverlay : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(OsuResumeOverlay),
        };

        public TestSceneResumeOverlay()
        {
            ManualOsuInputManager osuInputManager;
            CursorContainer cursor;
            ResumeOverlay resume;

            bool resumeFired = false;

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

            resume.ResumeAction = () => resumeFired = true;

            AddStep("move mouse to center", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.Centre));
            AddStep("show", () => resume.Show());

            AddStep("move mouse away", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.TopLeft));
            AddStep("click", () => osuInputManager.GameClick());
            AddAssert("not dismissed", () => !resumeFired && resume.State.Value == Visibility.Visible);

            AddStep("move mouse back", () => InputManager.MoveMouseTo(ScreenSpaceDrawQuad.Centre));
            AddStep("click", () => osuInputManager.GameClick());
            AddAssert("dismissed", () => resumeFired && resume.State.Value == Visibility.Hidden);
        }

        private class ManualOsuInputManager : OsuInputManager
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
