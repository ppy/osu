// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneGameplayCursorSizeChange : PlayerTestScene
    {
        private const float initial_cursor_size = 1f;
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        [Resolved]
        private SkinManager? skins { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (skins != null) skins.CurrentSkinInfo.Value = skins.DefaultClassicSkin.SkinInfo;
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("Set gameplay cursor size: 1", () => LocalConfig.SetValue(OsuSetting.GameplayCursorSize, initial_cursor_size));
            AddStep("resume player", () => Player.GameplayClockContainer.Start());
            AddUntilStep("clock running", () => Player.GameplayClockContainer.IsRunning);
        }

        [Test]
        public void TestPausedChangeCursorSize()
        {
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddStep("move cursor to top left", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopLeft));
            AddStep("move cursor to center", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));
            AddStep("move cursor to top right", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.TopRight));
            AddStep("press escape", () => InputManager.Key(Key.Escape));

            AddSliderStep("cursor size", 0.1f, 2f, 1f, v => LocalConfig.SetValue(OsuSetting.GameplayCursorSize, v));
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new TestPlayer(true, false);
    }
}
