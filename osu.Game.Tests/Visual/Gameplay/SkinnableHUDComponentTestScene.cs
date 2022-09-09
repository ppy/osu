// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;

namespace osu.Game.Tests.Visual.Gameplay
{
    public abstract class SkinnableHUDComponentTestScene : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SetContents(skin =>
            {
                var implementation = skin != null
                    ? CreateLegacyImplementation()
                    : CreateDefaultImplementation();

                implementation.Anchor = Anchor.Centre;
                implementation.Origin = Anchor.Centre;
                return implementation;
            });
        });

        protected abstract Drawable CreateDefaultImplementation();
        protected abstract Drawable CreateLegacyImplementation();
    }
}
