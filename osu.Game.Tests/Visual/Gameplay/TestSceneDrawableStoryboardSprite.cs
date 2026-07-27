// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Gameplay;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneDrawableStoryboardSprite : SkinnableTestScene
    {
        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();

        [Cached(typeof(Storyboard))]
        private TestStoryboard storyboard { get; set; } = new TestStoryboard();

        [Cached]
        private GameplayState gameplayState = TestGameplayState.Create(new OsuRuleset());

        private IEnumerable<DrawableStoryboardSprite> sprites => this.ChildrenOfType<DrawableStoryboardSprite>();

        private const string lookup_name = "hitcircleoverlay";

        [Test]
        public void TestSkinSpriteDisallowedByDefault()
        {
            AddStep("disallow all lookups", () =>
            {
                storyboard.UseSkinSprites = false;
                storyboard.ProvideResources = false;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));

            AddAssert("sprite didn't find texture", () =>
                sprites.All(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Texture == null)));
        }

        [Test]
        public void TestSpriteFadeOverflowBehaviour()
        {
            ManualClock manualClock = new ManualClock();

            AddStep("allow storyboard lookup", () =>
            {
                storyboard.UseSkinSprites = false;
                storyboard.ProvideResources = true;
            });

            AddStep("create sprite", () => SetContents(_ =>
            {
                var layer = storyboard.GetLayer("Background");

                var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, lookup_name, Anchor.TopLeft, new Vector2(256, 192));
                sprite.Commands.AddAlpha(Easing.None, 0, 2000, 0, 2);

                layer.Elements.Clear();
                layer.Add(sprite);

                return new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        storyboard.CreateDrawable().With(d => d.Clock = new FramedClock(manualClock))
                    }
                };
            }));

            AddStep("seek to 1000 ms", () => manualClock.CurrentTime = 900);
            AddUntilStep("sprite reached high opacity once", () => sprites.All(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Alpha > 0.8f)));
            AddStep("seek to 2000 ms", () => manualClock.CurrentTime = 1100);
            AddUntilStep("sprite reset to low opacity", () => sprites.All(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Alpha < 0.2f)));
            AddStep("seek to 2000 ms", () => manualClock.CurrentTime = 1900);
            AddUntilStep("sprite reached high opacity twice", () => sprites.All(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Alpha > 0.8f)));
        }

        [Test]
        public void TestLookupFromStoryboard()
        {
            AddStep("allow storyboard lookup", () =>
            {
                storyboard.UseSkinSprites = false;
                storyboard.ProvideResources = true;
            });

            AddStep("create sprites", () => SetContents(_ => createSprite(lookup_name, Anchor.TopLeft, Vector2.Zero)));

            // Only checking for at least one sprite that succeeded, as not all skins in this test provide the hitcircleoverlay texture.
            AddAssert("sprite found texture", () =>
                sprites.Any(sprite => sprite.ChildrenOfType<Sprite>().All(s => s.Texture != null)));

            assertStoryboardSourced();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestVideo(bool scaleTransformProvided)
        {
            AddStep("allow storyboard lookup", () =>
            {
                storyboard.ProvideResources = true;
            });

            AddStep("create video", () => SetContents(_ =>
            {
                var layer = storyboard.GetLayer("Video");

                var sprite = new StoryboardVideo(StoryboardElementSource.Beatmap, "Videos/test-video.mp4", Time.Current);

                if (scaleTransformProvided)
                {
                    sprite.Commands.AddScale(Easing.None, Time.Current, Time.Current + 1000, 1, 2);
                    sprite.Commands.AddScale(Easing.None, Time.Current + 1000, Time.Current + 2000, 2, 1);
                }

                layer.Elements.Clear();
                layer.Add(sprite);

                return new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        storyboard.CreateDrawable()
                    }
                };
            }));
        }

        [Test]
        public void TestSkinLookupPreferredOverStoryboard()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = true;
                storyboard.ProvideResources = true;
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
                storyboard.ProvideResources = false;
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
                storyboard.ProvideResources = true;
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
                storyboard.ProvideResources = true;
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
                storyboard.ProvideResources = true;
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
                storyboard.ProvideResources = true;
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

        [Test]
        public void TestPassingFailingTriggers()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = false;
                storyboard.ProvideResources = true;
            });

            AddStep("create sprites", () => SetContents(_ =>
            {
                var layer = storyboard.GetLayer("Background");

                var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, lookup_name, Anchor.TopLeft, Vector2.Zero);
                var loop = sprite.AddLoopingGroup(Time.Current, 100);
                loop.AddAlpha(Easing.None, 0, 10000, 1, 1);
                var passTrigger = sprite.AddTriggerGroup(@"Passing", 0, 0, 0);
                passTrigger.AddFlipV(Easing.None, 0, 0, false, false);
                var failTrigger = sprite.AddTriggerGroup(@"Failing", 0, 0, 0);
                failTrigger.AddFlipV(Easing.None, 0, 0, true, true);

                layer.Elements.Clear();
                layer.Add(sprite);

                return new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        storyboard.CreateDrawable()
                    }
                };
            }));

            AddStep("failing", () => gameplayState.HealthProcessor.Health.Value = 0.3);
            AddAssert("all sprites flipped", () => this.ChildrenOfType<DrawableStoryboardSprite>().All(s => s.FlipV));

            AddStep("passing", () => gameplayState.HealthProcessor.Health.Value = 0.8);
            AddAssert("all sprites unflipped", () => this.ChildrenOfType<DrawableStoryboardSprite>().All(s => !s.FlipV));
        }

        [Test]
        public void TestHitObjectHitTrigger()
        {
            AddStep("allow all lookups", () =>
            {
                storyboard.UseSkinSprites = false;
                storyboard.ProvideResources = true;
            });

            AddStep("create sprites", () => SetContents(_ =>
            {
                var layer = storyboard.GetLayer("Background");

                var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, lookup_name, Anchor.TopLeft, Vector2.Zero);
                var loop = sprite.AddLoopingGroup(Time.Current, 100);
                loop.AddAlpha(Easing.None, 0, 10000, 1, 1);
                var hitObjectHitTrigger = sprite.AddTriggerGroup(@"HitObjectHit", 0, 2000, 0);
                hitObjectHitTrigger.AddColour(Easing.None, 0, 2000, Colour4.Red, Colour4.White);

                layer.Elements.Clear();
                layer.Add(sprite);

                return new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        storyboard.CreateDrawable()
                    }
                };
            }));

            AddStep("new scorable hit", () => ((Bindable<JudgementResult>)gameplayState.LastJudgementResult).Value = new JudgementResult(new HitObject(), new Judgement())
            {
                Type = HitResult.Great
            });
            AddAssert("colour changed", () => this.ChildrenOfType<DrawableStoryboardSprite>().All(s => s.Colour != Colour4.White));
            AddUntilStep("colour eventually reverts", () => this.ChildrenOfType<DrawableStoryboardSprite>().All(s => s.Colour == Colour4.White));

            AddStep("new non-scorable hit", () => ((Bindable<JudgementResult>)gameplayState.LastJudgementResult).Value = new JudgementResult(new HitObject(), new Judgement())
            {
                Type = HitResult.IgnoreMiss
            });
            AddAssert("colour not changed", () => this.ChildrenOfType<DrawableStoryboardSprite>().All(s => s.Colour == Colour4.White));

            AddStep("new scorable miss", () => ((Bindable<JudgementResult>)gameplayState.LastJudgementResult).Value = new JudgementResult(new HitObject(), new Judgement())
            {
                Type = HitResult.Miss
            });
            AddAssert("colour not changed", () => this.ChildrenOfType<DrawableStoryboardSprite>().All(s => s.Colour == Colour4.White));
        }

        private Drawable createSprite(string lookupName, Anchor origin, Vector2 initialPosition)
        {
            var layer = storyboard.GetLayer("Background");

            var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, lookupName, origin, initialPosition);
            var loop = sprite.AddLoopingGroup(Time.Current, 100);
            loop.AddAlpha(Easing.None, 0, 10000, 1, 1);

            layer.Elements.Clear();
            layer.Add(sprite);

            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    storyboard.CreateDrawable()
                }
            };
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

            public bool ProvideResources { get; set; }

            public override string GetStoragePathFromStoryboardPath(string path) => ProvideResources ? path : string.Empty;

            private partial class TestDrawableStoryboard : DrawableStoryboard
            {
                private readonly bool provideResources;

                public TestDrawableStoryboard(TestStoryboard storyboard, IReadOnlyList<Mod>? mods)
                    : base(storyboard, mods)
                {
                    provideResources = storyboard.ProvideResources;
                }

                protected override IResourceStore<byte[]> CreateResourceLookupStore() => provideResources
                    ? new ResourcesTextureStore()
                    : new ResourceStore<byte[]>();

                internal class ResourcesTextureStore : IResourceStore<byte[]>
                {
                    private readonly DllResourceStore store;

                    public ResourcesTextureStore()
                    {
                        store = TestResources.GetStore();
                    }

                    public void Dispose() => store.Dispose();

                    public byte[] Get(string name) => store.Get(map(name));

                    public Task<byte[]> GetAsync(string name, CancellationToken cancellationToken = new CancellationToken()) => store.GetAsync(map(name), cancellationToken);

                    public Stream GetStream(string name) => store.GetStream(map(name));

                    private string map(string name)
                    {
                        switch (name)
                        {
                            case lookup_name:
                                return "Resources/Textures/test-image.png";

                            default:
                                return $"Resources/{name}";
                        }
                    }

                    public IEnumerable<string> GetAvailableResources() => store.GetAvailableResources();
                }
            }
        }
    }
}
