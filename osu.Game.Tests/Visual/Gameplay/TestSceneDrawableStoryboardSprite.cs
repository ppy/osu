// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneDrawableStoryboardSprite : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached(typeof(Storyboard))]
        private TestStoryboard storyboard { get; set; } = new TestStoryboard();

        private IEnumerable<DrawableStoryboardSprite> sprites => this.ChildrenOfType<DrawableStoryboardSprite>();

        private const string lookup_name = "hitcircleoverlay";

        [Test]
        public void TestSkinSpriteDisallowedByDefault()
        {
            AddStep("disallow all lookups", () =>
            {
                storyboard.UseSkinSprites = false;
                storyboard.AlwaysProvideTexture = false;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));

            AddAssert("sprite didn't find texture", () =>
                sprites.All(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Texture == null)));
        }

        [Test]
        public void TestLookupFromStoryboard()
        {
            AddStep("allow storyboard lookup", () =>
            {
                storyboard.UseSkinSprites = false;
                storyboard.AlwaysProvideTexture = true;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));

            // Only checking for at least one sprite that succeeded, as not all skins in this test provide the hitcircleoverlay texture.
            AddAssert("sprite found texture", () =>
                sprites.Any(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Texture != null)));

            assertStoryboardSourced();
        }

        [Test]
        public void TestSkinLookupPreferredOverStoryboard()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = true;
                storyboard.AlwaysProvideTexture = true;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));

            // Only checking for at least one sprite that succeeded, as not all skins in this test provide the hitcircleoverlay texture.
            AddAssert("sprite found texture", () =>
                sprites.Any(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Texture != null)));

            assertSkinSourced();
        }

        [Test]
        public void TestAllowLookupFromSkin()
        {
            AddStep("allow skin lookup", () =>
            {
                storyboard.UseSkinSprites = true;
                storyboard.AlwaysProvideTexture = false;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));

            // Only checking for at least one sprite that succeeded, as not all skins in this test provide the hitcircleoverlay texture.
            AddAssert("sprite found texture", () =>
                sprites.Any(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Texture != null)));

            assertSkinSourced();
        }

        [Test]
        public void TestFlippedSprite()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = true;
                storyboard.AlwaysProvideTexture = true;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));
            AddStep("flip sprites", () => sprites.ForEach(s =>
            {
                s.FlipH = true;
                s.FlipV = true;
            }));
            AddAssert("origin flipped", () => sprites.All(s => s.Origin == Anchor.BottomRight));
        }

        [Test]
        public void TestZeroScale()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = true;
                storyboard.AlwaysProvideTexture = true;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));
            AddAssert("sprites present", () => sprites.All(s => s.IsPresent));
            AddStep("scale sprite", () => sprites.ForEach(s => s.VectorScale = new Vector2(0, 1)));
            AddAssert("sprites not present", () => sprites.All(s => !s.IsPresent));
        }

        [Test]
        public void TestNegativeScale()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = true;
                storyboard.AlwaysProvideTexture = true;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));
            AddStep("scale sprite", () => sprites.ForEach(s => s.VectorScale = new Vector2(-1)));
            AddAssert("origin flipped", () => sprites.All(s => s.Origin == Anchor.BottomRight));
        }

        [Test]
        public void TestNegativeScaleWithFlippedSprite()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = true;
                storyboard.AlwaysProvideTexture = true;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));
            AddStep("scale sprite", () => sprites.ForEach(s => s.VectorScale = new Vector2(-1)));
            AddAssert("origin flipped", () => sprites.All(s => s.Origin == Anchor.BottomRight));
            AddStep("flip sprites", () => sprites.ForEach(s =>
            {
                s.FlipH = true;
                s.FlipV = true;
            }));
            AddAssert("origin back", () => sprites.All(s => s.Origin == Anchor.TopLeft));
        }

        private DrawableStoryboard createSprite(string lookupName, Anchor origin, Vector2 initialPosition)
        {
            var layer = storyboard.GetLayer("Background");

            var sprite = new StoryboardSprite(lookupName, origin, initialPosition);
            sprite.AddLoop(Time.Current, 100).Alpha.Add(Easing.None, 0, 10000, 1, 1);

            layer.Elements.Clear();
            layer.Add(sprite);

            return storyboard.CreateDrawable().With(s => s.RelativeSizeAxes = Axes.Both);
        }

        private void assertStoryboardSourced()
        {
            AddAssert("sprite came from storyboard", () =>
                sprites.Any(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Size == new Vector2(200))));
        }

        private void assertSkinSourced()
        {
            AddAssert("sprite came from skin", () =>
                sprites.Any(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Size == new Vector2(128))));
        }

        private partial class TestStoryboard : Storyboard
        {
            public override DrawableStoryboard CreateDrawable(IReadOnlyList<Mod>? mods = null)
            {
                return new TestDrawableStoryboard(this, mods);
            }

            public bool AlwaysProvideTexture { get; set; }

            public override string GetStoragePathFromStoryboardPath(string path) => AlwaysProvideTexture ? path : string.Empty;

            private partial class TestDrawableStoryboard : DrawableStoryboard
            {
                private readonly bool alwaysProvideTexture;

                public TestDrawableStoryboard(TestStoryboard storyboard, IReadOnlyList<Mod>? mods)
                    : base(storyboard, mods)
                {
                    alwaysProvideTexture = storyboard.AlwaysProvideTexture;
                }

                protected override IResourceStore<byte[]> CreateResourceLookupStore() => alwaysProvideTexture
                    ? new AlwaysReturnsTextureStore()
                    : new ResourceStore<byte[]>();

                internal class AlwaysReturnsTextureStore : IResourceStore<byte[]>
                {
                    private const string test_image = "Resources/Textures/test-image.png";

                    private readonly DllResourceStore store;

                    public AlwaysReturnsTextureStore()
                    {
                        store = TestResources.GetStore();
                    }

                    public void Dispose() => store.Dispose();

                    public byte[] Get(string name) => store.Get(test_image);

                    public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken()) => store.GetAsync(test_image, cancellationToken);

                    public Stream GetStream(string name) => store.GetStream(test_image);

                    public IEnumerable<string> GetAvailableResources() => store.GetAvailableResources();
                }
            }
        }
    }
}
