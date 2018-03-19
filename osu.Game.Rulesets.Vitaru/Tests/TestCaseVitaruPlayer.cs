using osu.Game.Tests.Visual;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Vitaru.Objects.Characters;

namespace osu.Game.Rulesets.Vitaru.Tests
{
    [TestFixture]
    internal class TestCaseVitaruPlayer : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Children = new Drawable[]
            {
                new VitaruPlayer(this, Characters.ReimuHakurei)
                {
                    Alpha = 1,
                    AlwaysPresent = true,
                }
            };
        }
    }
}
