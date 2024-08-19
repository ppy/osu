// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets.Catch.Skinning.Legacy;
using osu.Game.Screens.Edit;
using osu.Game.Skinning;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [HeadlessTest]
    public partial class TestSceneCatchSkinMigration : PlayerTestScene
    {
        private SkinEditor skinEditor = null!;

        protected override bool Autoplay => true;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached]
        public readonly EditorClipboard Clipboard = new EditorClipboard();

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        private SkinComponentsContainer targetContainer => Player.ChildrenOfType<SkinComponentsContainer>().First();

        private SkinComponentsContainer rulesetHUDTarget => Player.ChildrenOfType<SkinComponentsContainer>()
                                                                  .Single(c => c.Lookup.Target == SkinComponentsContainerLookup.TargetArea.MainHUDComponents && c.Lookup.Ruleset != null);

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset skin", () => skins.CurrentSkinInfo.SetDefault());
            AddUntilStep("wait for hud load", () => targetContainer.ComponentsLoaded);

            AddStep("reload skin editor", () =>
            {
                if (skinEditor.IsNotNull())
                    skinEditor.Expire();
                Player.ScaleTo(0.4f);
                LoadComponentAsync(skinEditor = new SkinEditor(Player), Add);
            });
            AddUntilStep("wait for loaded", () => skinEditor.IsLoaded);
        }

        [Test]
        public void TestMigrationLegacyCatch()
        {
            AddStep("import old classic skin", () => skins.CurrentSkinInfo.Value = importSkinFromArchives(@"classic-layout-version-0.osk").SkinInfo);
            AddAssert("layout loaded", () => skins.CurrentSkin.Value.LayoutInfos, () => NUnit.Framework.Contains.Key(SkinComponentsContainerLookup.TargetArea.MainHUDComponents));
            AddUntilStep("wait for load", () => rulesetHUDTarget.ComponentsLoaded);

            AddAssert("catch specific combo counter in ruleset target", () => rulesetHUDTarget.Components.OfType<LegacyCatchComboCounter>(), () => Has.One.Items);
            AddAssert("correct anchor/origin", () =>
            {
                var catchComboCounter = rulesetHUDTarget.Components.OfType<LegacyCatchComboCounter>().Single();
                return catchComboCounter.Anchor == Anchor.CentreLeft && catchComboCounter.Origin == Anchor.Centre;
            });
        }

        private Skin importSkinFromArchives(string filename)
        {
            var imported = skins.Import(new ImportTask(TestResources.OpenResource($@"Archives/{filename}"), filename)).GetResultSafely();
            return imported.PerformRead(skinInfo => skins.GetSkin(skinInfo));
        }

        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();
    }
}
