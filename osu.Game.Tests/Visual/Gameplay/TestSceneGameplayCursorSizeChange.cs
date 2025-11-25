// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneGameplayCursorSizeChange : OsuPlayerTestScene
    {
        [Resolved]
        private SkinManager? skins { get; set; }

        protected new PausePlayer Player => (PausePlayer)base.Player;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (skins != null) skins.CurrentSkinInfo.Value = skins.DefaultClassicSkin.SkinInfo;
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("resume player", () => Player.GameplayClockContainer.Start());
            AddUntilStep("clock running", () => Player.GameplayClockContainer.IsRunning);
        }

        [Test]
        public void TestChangeCursorSize()
        {
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddStep("move cursor to top left", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopLeft));
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddStep("move cursor to top right", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopRight));
            AddStep("press escape", () => InputManager.Key(Key.Escape));

            for (float cursorSize = 0.4f; cursorSize <= 1.6f + 0.001f; cursorSize += 0.4f)
            {
                AddWaitStep("wait 2 seconds", 2);
                float newCursorSize = cursorSize;
                AddStep($"gameplay cursor size: {newCursorSize:F1}", () => LocalConfig.SetValue(OsuSetting.GameplayCursorSize, newCursorSize));
            }
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new PausePlayer();

        protected partial class PausePlayer : TestPlayer
        {
            public double LastPauseTime { get; private set; }
            public double LastResumeTime { get; private set; }

            public override void OnEntering(ScreenTransitionEvent e)
            {
                base.OnEntering(e);
                GameplayClockContainer.Stop();
            }

            private bool? isRunning;

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (GameplayClockContainer.IsRunning != isRunning)
                {
                    isRunning = GameplayClockContainer.IsRunning;

                    if (isRunning.Value)
                        LastResumeTime = GameplayClockContainer.CurrentTime;
                    else
                        LastPauseTime = GameplayClockContainer.CurrentTime;
                }
            }
        }
    }
}
