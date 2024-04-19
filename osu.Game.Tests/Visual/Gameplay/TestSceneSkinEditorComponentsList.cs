// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinEditorComponentsList : SkinnableTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Test]
        public void TestToggleEditor()
        {
            var skinComponentsContainer = new SkinnableContainer(new SkinnableContainerLookup(SkinnableContainerLookup.TargetArea.SongSelect));

            AddStep("show available components", () => SetContents(_ => new SkinComponentToolbox(skinComponentsContainer, null)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Width = 0.6f,
            }));
        }

        [Test]
        public void TestNonPlaceableComponent()
        {
            // Lift TestRuleset and NonPlaceableComponent into an isolated assembly containing both types at a top level.
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = asmBuilder.DefineDynamicModule(asmBuilder.GetName().Name!);

            moduleBuilder.DefineType($"{nameof(NonPlaceableComponent)}", TypeAttributes.Public | TypeAttributes.Class, typeof(NonPlaceableComponent)).CreateType();
            TypeBuilder rulesetType = moduleBuilder.DefineType("MockRuleset", TypeAttributes.Public | TypeAttributes.Class, typeof(TestRuleset));
            Ruleset ruleset = (Ruleset)Activator.CreateInstance(rulesetType.CreateType())!;

            AddStep("show available components", () => SetContents(_ =>
                new SkinComponentToolbox(new SkinnableContainer(new SkinnableContainerLookup(SkinnableContainerLookup.TargetArea.SongSelect)), ruleset.RulesetInfo)
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Width = 0.6f,
                }));

            AddAssert("component not visible", this.ChildrenOfType<SkinComponentToolbox.ToolboxComponentButton>, () => Is.Empty);
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        // Must be public to be lifted into a secondary assembly.
        public class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();
            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => throw new NotImplementedException();
            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();
            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();
            public override string Description => string.Empty;
            public override string ShortName => string.Empty;
        }

        // Must be public to be lifted into a secondary assembly.
        public partial class NonPlaceableComponent : Drawable, ISerialisableDrawable
        {
            public bool UsesFixedAnchor { get; set; }
            public bool IsPlaceable => false;
        }
    }
}
