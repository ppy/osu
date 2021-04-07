// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneDrawableStoryboardSprite : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached]
        private Storyboard storyboard { get; set; } = new Storyboard();

        [Test]
        public void TestSkinSpriteDisallowedByDefault()
        {
            const string lookup_name = "hitcircleoverlay";

            AddStep("allow skin lookup", () => storyboard.UseSkinSprites = false);

            AddStep("create sprites", () => SetContents(
                () => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));

            assertSpritesFromSkin(false);
        }

        [Test]
        public void TestAllowLookupFromSkin()
        {
            const string lookup_name = "hitcircleoverlay";

            AddStep("allow skin lookup", () => storyboard.UseSkinSprites = true);

            AddStep("create sprites", () => SetContents(
                () => createSprite(lookup_name, Anchor.Centre, Vector2.Zero)));

            assertSpritesFromSkin(true);
        }

        private DrawableStoryboardSprite createSprite(string lookupName, Anchor origin, Vector2 initialPosition)
            => new DrawableStoryboardSprite(
                new StoryboardSprite(lookupName, origin, initialPosition)
            ).With(s =>
            {
                s.LifetimeStart = double.MinValue;
                s.LifetimeEnd = double.MaxValue;
            });

        private void assertSpritesFromSkin(bool fromSkin) =>
            AddAssert($"sprites are {(fromSkin ? "from skin" : "from storyboard")}",
                () => this.ChildrenOfType<DrawableStoryboardSprite>()
                          .All(sprite => sprite.ChildrenOfType<SkinnableSprite>().Any() == fromSkin));
    }
}
