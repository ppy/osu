// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneStoryboardCommands : OsuTestScene
    {
        [Cached(typeof(Storyboard))]
        private TestStoryboard storyboard { get; set; } = new TestStoryboard
        {
            UseSkinSprites = false,
            AlwaysProvideTexture = true,
        };

        private readonly ManualClock manualClock = new ManualClock { Rate = 1, IsRunning = true };
        private int clockDirection;

        private const string lookup_name = "hitcircleoverlay";
        private const double clock_limit = 2500;

        protected override Container<Drawable> Content => content;

        private Container content = null!;
        private SpriteText timelineText = null!;
        private Box timelineMarker = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            base.Content.Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                timelineText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Margin = new MarginPadding { Bottom = 60 },
                },
                timelineMarker = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomCentre,
                    RelativePositionAxes = Axes.X,
                    Size = new Vector2(2, 50),
                },
            };
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("start clock", () => clockDirection = 1);
            AddStep("pause clock", () => clockDirection = 0);
            AddStep("set clock = 0", () => manualClock.CurrentTime = 0);
        }

        [Test]
        public void TestNormalCommandPlayback()
        {
            AddStep("create storyboard", () => Child = createStoryboard(s =>
            {
                s.Commands.AddY(Easing.OutBounce, 500, 900, 100, 240);
                s.Commands.AddY(Easing.OutQuint, 1100, 1500, 240, 100);
            }));

            assert(0, 100);
            assert(500, 100);
            assert(1000, 240);
            assert(1500, 100);
            assert(clock_limit, 100);
            assert(1500, 100);
            assert(1000, 240);
            assert(500, 100);
            assert(0, 100);

            void assert(double time, double y)
            {
                AddStep($"set clock = {time}", () => manualClock.CurrentTime = time);
                AddAssert($"sprite y = {y} at t = {time}", () => this.ChildrenOfType<DrawableStoryboardSprite>().Single().Y == y);
            }
        }

        [Test]
        public void TestLoopingCommandsPlayback()
        {
            AddStep("create storyboard", () => Child = createStoryboard(s =>
            {
                var loop = s.AddLoopingGroup(250, 1);
                loop.AddY(Easing.OutBounce, 0, 400, 100, 240);
                loop.AddY(Easing.OutQuint, 600, 1000, 240, 100);
            }));

            assert(0, 100);
            assert(250, 100);
            assert(850, 240);
            assert(1250, 100);
            assert(1850, 240);
            assert(2250, 100);
            assert(clock_limit, 100);
            assert(2250, 100);
            assert(1850, 240);
            assert(1250, 100);
            assert(850, 240);
            assert(250, 100);
            assert(0, 100);

            void assert(double time, double y)
            {
                AddStep($"set clock = {time}", () => manualClock.CurrentTime = time);
                AddAssert($"sprite y = {y} at t = {time}", () => this.ChildrenOfType<DrawableStoryboardSprite>().Single().Y == y);
            }
        }

        [Test]
        public void TestLoopManyTimes()
        {
            AddStep("create storyboard", () => Child = createStoryboard(s =>
            {
                var loop = s.AddLoopingGroup(500, 10000);
                loop.AddY(Easing.OutBounce, 0, 60, 100, 240);
                loop.AddY(Easing.OutQuint, 80, 120, 240, 100);
            }));
        }

        [Test]
        public void TestParameterTemporaryEffect()
        {
            AddStep("create storyboard", () => Child = createStoryboard(s =>
            {
                s.Commands.AddFlipV(Easing.None, 1000, 1500, true, false);
            }));

            AddAssert("sprite not flipped at t = 0", () => !this.ChildrenOfType<DrawableStoryboardSprite>().Single().FlipV);

            AddStep("set clock = 1250", () => manualClock.CurrentTime = 1250);
            AddAssert("sprite flipped at t = 1250", () => this.ChildrenOfType<DrawableStoryboardSprite>().Single().FlipV);

            AddStep("set clock = 2000", () => manualClock.CurrentTime = 2000);
            AddAssert("sprite not flipped at t = 2000", () => !this.ChildrenOfType<DrawableStoryboardSprite>().Single().FlipV);

            AddStep("resume clock", () => clockDirection = 1);
        }

        [Test]
        public void TestParameterPermanentEffect()
        {
            AddStep("create storyboard", () => Child = createStoryboard(s =>
            {
                s.Commands.AddFlipV(Easing.None, 1000, 1000, true, true);
            }));

            AddAssert("sprite flipped at t = 0", () => this.ChildrenOfType<DrawableStoryboardSprite>().Single().FlipV);

            AddStep("set clock = 1250", () => manualClock.CurrentTime = 1250);
            AddAssert("sprite flipped at t = 1250", () => this.ChildrenOfType<DrawableStoryboardSprite>().Single().FlipV);

            AddStep("set clock = 2000", () => manualClock.CurrentTime = 2000);
            AddAssert("sprite flipped at t = 2000", () => this.ChildrenOfType<DrawableStoryboardSprite>().Single().FlipV);

            AddStep("resume clock", () => clockDirection = 1);
        }

        protected override void Update()
        {
            base.Update();

            if (manualClock.CurrentTime > clock_limit || manualClock.CurrentTime < 0)
                clockDirection = -clockDirection;

            manualClock.CurrentTime += Time.Elapsed * clockDirection;
            timelineText.Text = $"Time: {manualClock.CurrentTime:0}ms";
            timelineMarker.X = (float)(manualClock.CurrentTime / clock_limit);
        }

        private DrawableStoryboard createStoryboard(Action<StoryboardSprite>? addCommands = null)
        {
            var layer = storyboard.GetLayer("Background");

            var sprite = new StoryboardSprite(lookup_name, Anchor.Centre, new Vector2(320, 240));
            sprite.Commands.AddScale(Easing.None, 0, clock_limit, 0.5f, 0.5f);
            sprite.Commands.AddAlpha(Easing.None, 0, clock_limit, 1, 1);
            addCommands?.Invoke(sprite);

            layer.Elements.Clear();
            layer.Add(sprite);

            return storyboard.CreateDrawable().With(c => c.Clock = new FramedClock(manualClock));
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
