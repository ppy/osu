// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Skinning.Select;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLegacyBackButton : SkinnableTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SetContents(s =>
            {
                if (s is not LegacySkin)
                    return Empty();

                return new Container
                {
                    Size = new Vector2(300),
                    Child = new SkinProvidingContainer(skins.DefaultClassicSkin)
                    {
                        Child = new SkinProvidingContainer(s)
                        {
                            Child = new LegacyBackButton
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                            }
                        },
                    }
                };
            });
        });
    }
}
