// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
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
        private bool forward;

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

        [SetUp]
        public void SetUp()
        {
            manualClock.CurrentTime = 0;
            forward = true;
        }

        [Test]
        public void TestLoop()
        {
            AddStep("create sprite", () => Child = createSprite(s =>
            {
                var loop = s.AddLoopingGroup(500, 10);
                loop.AddY(Easing.OutBounce, 0, 600, 100, 240);
                loop.AddY(Easing.OutQuint, 800, 1200, 240, 100);
            }));
        }

        protected override void Update()
        {
            base.Update();

            if (manualClock.CurrentTime > clock_limit || manualClock.CurrentTime < 0)
                forward = !forward;

            manualClock.CurrentTime += Time.Elapsed * (forward ? 1 : -1);
            timelineText.Text = $"Time: {manualClock.CurrentTime:0}ms";
            timelineMarker.X = (float)(manualClock.CurrentTime / clock_limit);
        }

        private DrawableStoryboard createSprite(Action<StoryboardSprite>? addCommands = null)
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
