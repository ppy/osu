// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public partial class TestSceneIntroTriangles : IntroTestScene
    {
        protected override bool IntroReliesOnTrack => true;
        protected override IntroScreen CreateScreen() => new IntroTriangles();

        [Cached(typeof(IRulesetStore))]
        private TestRulesetStore rulesets = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            rulesets = new TestRulesetStore(parent.Get<RealmAccess>(), parent.Get<Storage>());
            return base.CreateChildDependencies(parent);
        }

        [Test]
        public void TestPlayIntroWithExcessiveRulesets()
        {
            AddStep("enable excessive rulesets", () => rulesets.ShowExcessiveRulesets = true);
            base.TestPlayIntro();
            AddStep("disable excessive rulesets", () => rulesets.ShowExcessiveRulesets = false);
        }

        internal class TestRulesetStore : RealmRulesetStore
        {
            public bool ShowExcessiveRulesets;

            public override IEnumerable<RulesetInfo> AvailableRulesets
            {
                get
                {
                    if (ShowExcessiveRulesets)
                    {
                        return base.AvailableRulesets
                                   .Concat(base.AvailableRulesets)
                                   .Concat(base.AvailableRulesets)
                                   .Concat(base.AvailableRulesets);
                    }

                    return base.AvailableRulesets;
                }
            }

            public TestRulesetStore(RealmAccess realm, Storage? storage = null)
                : base(realm, storage)
            {
            }
        }
    }
}
