// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
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

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create storyboard", () =>
            {
                storyboard.BeatmapInfo = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;

                var background = storyboard.GetLayer("Background");

                background.Elements.Clear();

                var sprite = new StoryboardSprite("Resources/Textures/test-image.png", Anchor.Centre, new Vector2(320, 240));
                sprite.AddLoop(Time.Current, 100).Alpha.Add(Easing.None, 0, 10000, 1, 1);
                background.Add(sprite);

                var video = new StoryboardVideo("Resources/Videos/test-video.mp4", Time.Current);
                storyboard.GetLayer("Video").Add(video);

                Child = drawableStoryboard = new DrawableStoryboard(storyboard);
            });
        }

        [Test]
        public void TestBasic()
        {
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
