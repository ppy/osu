// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Storyboards;
using osu.Game.Storyboards.Drawables;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneStoryboardElements : OsuTestScene
    {
        private DrawableStoryboard drawableStoryboard = null!;

        private void createStoryboard(Func<IStoryboardElement> createElement)
        {
            AddStep("create storyboard", () =>
            {
                TestStoryboard storyboard = new TestStoryboard
                {
                    BeatmapInfo = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo
                };

                var videoLayer = storyboard.GetLayer("Video");
                var backgroundLayer = storyboard.GetLayer("Background");

                var element = createElement();

                if (element is StoryboardVideo video)
                {
                    videoLayer.Add(video);
                }
                else if (element is StoryboardSprite sprite)
                {
                    sprite.AddLoop(0, 100).Alpha.Add(Easing.None, 0, 10000, 1, 1);
                    backgroundLayer.Add(sprite);
                }

                Child = drawableStoryboard = new TestDrawableStoryboard(storyboard)
                {
                    Clock = new FramedClock()
                };
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("Change origin to centre", () =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements)
                        d.Origin = Anchor.Centre;
                }
            });

            AddStep("Change origin to bottom right", () =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements)
                        d.Origin = Anchor.BottomRight;
                }
            });

            AddStep("Change origin to top left", () =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements)
                        d.Origin = Anchor.TopLeft;
                }
            });

            AddToggleStep("Toggle flipH", val =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements.OfType<DrawableStoryboardAnimation>())
                        d.FlipH = val;
                    foreach (var d in layer.Elements.OfType<DrawableStoryboardSprite>())
                        d.FlipH = val;
                }
            });

            AddToggleStep("Toggle flipV", val =>
            {
                foreach (var layer in drawableStoryboard.ChildrenOfType<DrawableStoryboardLayer.LayerElementContainer>())
                {
                    foreach (var d in layer.Elements.OfType<DrawableStoryboardAnimation>())
                        d.FlipV = val;
                    foreach (var d in layer.Elements.OfType<DrawableStoryboardSprite>())
                        d.FlipV = val;
                }
            });
        }

        [Test]
        public void TestSprite()
        {
            createStoryboard(() => new StoryboardSprite("Resources/Textures/sample-texture.png", Anchor.Centre, new Vector2(320, 240)));
        }

        [Test]
        public void TestAnimation()
        {
            createStoryboard(() => new StoryboardAnimation("Resources/Textures/sample-animation.png", Anchor.Centre, new Vector2(320, 240), 2, 100, AnimationLoopType.LoopForever));
        }

        [Test]
        public void TestVideo()
        {
            createStoryboard(() => new StoryboardVideo("Resources/Videos/test-video.mp4", 0));
        }

        public partial class TestDrawableStoryboard : DrawableStoryboard
        {
            public TestDrawableStoryboard(Storyboard storyboard, IReadOnlyList<Mod>? mods = null)
                : base(storyboard, mods)
            {
            }

            protected override IResourceStore<byte[]> CreateResourceLookupStore() => TestResources.GetStore();
        }

        public class TestStoryboard : Storyboard
        {
            public override string GetStoragePathFromStoryboardPath(string path) => path;
        }
    }
}
