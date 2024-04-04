// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public partial class TestSceneManiaSkinEditor : PlayerTestScene
    {
        private SkinEditor skinEditor = null!;

        protected override bool Autoplay => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached]
        public readonly EditorClipboard Clipboard = new EditorClipboard();

        private SkinComponentsContainer targetContainer => Player.ChildrenOfType<SkinComponentsContainer>().First();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("clear", () => Clear(true));

            AddUntilStep("wait for hud load", () => targetContainer.ComponentsLoaded);

            AddStep("reload skin editor", () =>
            {
                Player.ScaleTo(0.4f);
                LoadComponentAsync(skinEditor = new SkinEditor(Player), Add);
            });
            AddUntilStep("wait for loaded", () => skinEditor.IsLoaded);
        }

        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();
    }
}
