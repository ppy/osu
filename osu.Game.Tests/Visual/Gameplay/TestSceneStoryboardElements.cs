// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
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

        [Cached(typeof(Storyboard))]
        private TestStoryboard storyboard = new TestStoryboard();

        private void createStoryboard(Func<IStoryboardElement> createElement)
        {
            AddStep("create storyboard", () =>
            {
                storyboard.BeatmapInfo = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;

                var videoLayer = storyboard.GetLayer("Video");
                var backgroundLayer = storyboard.GetLayer("Background");

                // We need to cache the storyboard, so rather than faffing around with re-caching each
                // reconstruction, just clear old elements between tests.
                videoLayer.Elements.Clear();
                backgroundLayer.Elements.Clear();

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

                Child = drawableStoryboard = new DrawableStoryboard(storyboard)
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

        public class TestStoryboard : Storyboard
        {
            public override Texture? GetTextureFromPath(string path, TextureStore textureStore)
            {
                textureStore.AddTextureSource(new TextureLoaderStore(TestResources.GetStore()));

                return textureStore.Get(path);
            }
        }
    }
}
