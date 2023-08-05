// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneEditDefaultSkin : OsuGameTestScene
    {
        private SkinManager skinManager => Game.Dependencies.Get<SkinManager>();
        private SkinEditorOverlay skinEditor => Game.Dependencies.Get<SkinEditorOverlay>();

        [Test]
        public void TestEditDefaultSkin()
        {
            AddAssert("is default skin", () => skinManager.CurrentSkinInfo.Value.ID == SkinInfo.ARGON_SKIN);

            AddStep("open settings", () => { Game.Settings.Show(); });

            // Until step requires as settings has a delayed load.
            AddUntilStep("export button disabled", () => Game.Settings.ChildrenOfType<SkinSection.ExportSkinButton>().SingleOrDefault()?.Enabled.Value == false);

            // Will create a mutable skin.
            AddStep("open skin editor", () => skinEditor.Show());

            // Until step required as the skin editor may take time to load (and an extra scheduled frame for the mutable part).
            AddUntilStep("is modified default skin", () => skinManager.CurrentSkinInfo.Value.ID != SkinInfo.ARGON_SKIN);
            AddAssert("is not protected", () => skinManager.CurrentSkinInfo.Value.PerformRead(s => !s.Protected));

            AddUntilStep("export button enabled", () => Game.Settings.ChildrenOfType<SkinSection.ExportSkinButton>().SingleOrDefault()?.Enabled.Value == true);
        }
    }
}
