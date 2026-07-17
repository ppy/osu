// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using osu.Game.Storyboards;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyStoryboardEncoderTest
    {
        [Test]
        public void TestBackground()
        {
            var initial = createComponents();
            initial.Beatmap.BeatmapInfo.Metadata.BackgroundFile = "bg.jpg";

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            Assert.That(decodedAfterEncode.Beatmap.BeatmapInfo.Metadata.BackgroundFile, Is.EqualTo("bg.jpg"));
        }

        [Test]
        public void TestBackgroundOffset()
        {
            var initial = createComponents();
            initial.Beatmap.BeatmapInfo.Metadata.BackgroundFile = "bg_offset.jpg";
            initial.Storyboard.BackgroundOffset = new Vector2(0, 45);

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            Assert.Multiple(() =>
            {
                Assert.That(decodedAfterEncode.Beatmap.BeatmapInfo.Metadata.BackgroundFile, Is.EqualTo("bg_offset.jpg"));
                Assert.That(decodedAfterEncode.Storyboard.BackgroundOffset, Is.EqualTo(new Vector2(0, 45)));
            });
        }

        [Test]
        public void TestVideos()
        {
            var initial = createComponents();

            initial.Storyboard.GetLayer("Video").Add(new StoryboardVideo(StoryboardElementSource.Beatmap, "video1.avi", 0));
            initial.Storyboard.GetLayer("Video").Add(new StoryboardVideo(StoryboardElementSource.Shared, "video2.mp4", 1234));

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            Assert.Multiple(() =>
            {
                var videoLayer = decodedAfterEncode.Storyboard.GetLayer("Video");
                Assert.That(videoLayer.Elements, Has.Count.EqualTo(2));

                Assert.That(videoLayer.Elements[0].Source, Is.EqualTo(StoryboardElementSource.Beatmap));
                Assert.That(videoLayer.Elements[0].Path, Is.EqualTo("video1.avi"));
                Assert.That(videoLayer.Elements[0].StartTime, Is.EqualTo(0));

                Assert.That(videoLayer.Elements[1].Source, Is.EqualTo(StoryboardElementSource.Shared));
                Assert.That(videoLayer.Elements[1].Path, Is.EqualTo("video2.mp4"));
                Assert.That(videoLayer.Elements[1].StartTime, Is.EqualTo(1234));
            });
        }

        [Test]
        public void TestVideoWithCommands()
        {
            var initial = createComponents();

            var video = new StoryboardVideo(StoryboardElementSource.Beatmap, "video1.avi", 0);
            video.Commands.AddScale(Easing.None, 0, 0, 0.7f, 0.7f);
            initial.Storyboard.GetLayer("Video").Add(video);

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            Assert.Multiple(() =>
            {
                var decodedVideo = (StoryboardVideo)decodedAfterEncode.Storyboard.GetLayer("Video").Elements.Single();

                Assert.That(decodedVideo.Source, Is.EqualTo(StoryboardElementSource.Beatmap));
                Assert.That(decodedVideo.Path, Is.EqualTo("video1.avi"));
                Assert.That(decodedVideo.StartTime, Is.EqualTo(0));

                Assert.That(decodedVideo.Commands.Scale, Has.Count.EqualTo(1));
                var scaleCommand = (decodedVideo.Commands.Scale.Single());
                Assert.That(scaleCommand.Easing, Is.EqualTo(Easing.None));
                Assert.That(scaleCommand.StartTime, Is.EqualTo(0));
                Assert.That(scaleCommand.EndTime, Is.EqualTo(0));
                Assert.That(scaleCommand.StartValue, Is.EqualTo(0.7f));
                Assert.That(scaleCommand.EndValue, Is.EqualTo(0.7f));
            });
        }

        [Test]
        public void TestSpritesAndAnimations()
        {
            var initial = createComponents();

            initial.Storyboard.GetLayer("Background").Add(new StoryboardSprite(StoryboardElementSource.Beatmap, "1.png", Anchor.TopLeft, new Vector2()));
            initial.Storyboard.GetLayer("Fail").Add(new StoryboardSprite(StoryboardElementSource.Shared, "2.png", Anchor.Centre, new Vector2(-3)));
            initial.Storyboard.GetLayer("Pass").Add(new StoryboardSprite(StoryboardElementSource.Shared, "3.png", Anchor.BottomRight, new Vector2(30, -30)));
            initial.Storyboard.GetLayer("Foreground").Add(new StoryboardAnimation(StoryboardElementSource.Beatmap, "anim1", Anchor.CentreLeft, new Vector2(30), frameCount: 10, frameDelay: 30, AnimationLoopType.LoopForever));
            initial.Storyboard.GetLayer("Overlay").Add(new StoryboardAnimation(StoryboardElementSource.Shared, "anim2", Anchor.CentreRight, new Vector2(30), frameCount: 4, frameDelay: 100, AnimationLoopType.LoopOnce));

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            var sb = decodedAfterEncode.Storyboard;

            Assert.Multiple(() =>
            {
                var backgroundSprite = (StoryboardSprite)sb.GetLayer("Background").Elements.Single();
                Assert.That(backgroundSprite.Source, Is.EqualTo(StoryboardElementSource.Beatmap));
                Assert.That(backgroundSprite.Path, Is.EqualTo("1.png"));
                Assert.That(backgroundSprite.Origin, Is.EqualTo(Anchor.TopLeft));
                Assert.That(backgroundSprite.InitialPosition, Is.EqualTo(new Vector2()));

                var failSprite = (StoryboardSprite)sb.GetLayer("Fail").Elements.Single();
                Assert.That(failSprite.Source, Is.EqualTo(StoryboardElementSource.Shared));
                Assert.That(failSprite.Path, Is.EqualTo("2.png"));
                Assert.That(failSprite.Origin, Is.EqualTo(Anchor.Centre));
                Assert.That(failSprite.InitialPosition, Is.EqualTo(new Vector2(-3)));

                var passSprite = (StoryboardSprite)sb.GetLayer("Pass").Elements.Single();
                Assert.That(passSprite.Source, Is.EqualTo(StoryboardElementSource.Shared));
                Assert.That(passSprite.Path, Is.EqualTo("3.png"));
                Assert.That(passSprite.Origin, Is.EqualTo(Anchor.BottomRight));
                Assert.That(passSprite.InitialPosition, Is.EqualTo(new Vector2(30, -30)));

                var foregroundAnimation = (StoryboardAnimation)sb.GetLayer("Foreground").Elements.Single();
                Assert.That(foregroundAnimation.Source, Is.EqualTo(StoryboardElementSource.Beatmap));
                Assert.That(foregroundAnimation.Path, Is.EqualTo("anim1"));
                Assert.That(foregroundAnimation.Origin, Is.EqualTo(Anchor.CentreLeft));
                Assert.That(foregroundAnimation.InitialPosition, Is.EqualTo(new Vector2(30)));
                Assert.That(foregroundAnimation.FrameCount, Is.EqualTo(10));
                Assert.That(foregroundAnimation.FrameDelay, Is.EqualTo(30));
                Assert.That(foregroundAnimation.LoopType, Is.EqualTo(AnimationLoopType.LoopForever));

                var overlayAnimation = (StoryboardAnimation)sb.GetLayer("Overlay").Elements.Single();
                Assert.That(overlayAnimation.Source, Is.EqualTo(StoryboardElementSource.Shared));
                Assert.That(overlayAnimation.Path, Is.EqualTo("anim2"));
                Assert.That(overlayAnimation.Origin, Is.EqualTo(Anchor.CentreRight));
                Assert.That(overlayAnimation.InitialPosition, Is.EqualTo(new Vector2(30)));
                Assert.That(overlayAnimation.FrameCount, Is.EqualTo(4));
                Assert.That(overlayAnimation.FrameDelay, Is.EqualTo(100));
                Assert.That(overlayAnimation.LoopType, Is.EqualTo(AnimationLoopType.LoopOnce));
            });
        }

        [Test]
        public void TestCommands()
        {
            var initial = createComponents();

            var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, "test.jpg", Anchor.Centre, new Vector2(300));
            sprite.Commands.AddAlpha(Easing.InBack, 100, 200, 0, 1);
            sprite.Commands.AddBlendingParameters(Easing.None, 300, 300, BlendingParameters.Additive, BlendingParameters.Additive);
            sprite.Commands.AddColour(Easing.InCubic, 400, 500, Color4.White, Color4.Aquamarine);
            sprite.Commands.AddFlipH(Easing.InOutQuad, 600, 600, true, true);
            sprite.Commands.AddFlipV(Easing.InOutQuad, 800, 900, true, false);
            sprite.Commands.AddRotation(Easing.OutSine, 1000, 1100, 0, 720);
            sprite.Commands.AddScale(Easing.OutQuint, 1200, 1300, 1, 4);
            sprite.Commands.AddVectorScale(Easing.InCirc, 1400, 1500, new Vector2(4), new Vector2(3, 1));
            sprite.Commands.AddX(Easing.InOutQuad, 1600, 1700, 300, 500);
            sprite.Commands.AddY(Easing.OutBounce, 1800, 1800, 300, 100);
            initial.Storyboard.GetLayer("Background").Add(sprite);

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            var decodedSprite = (StoryboardSprite)decodedAfterEncode.Storyboard.GetLayer("Background").Elements.Single();

            Assert.Multiple(() =>
            {
                var alphaCommand = decodedSprite.Commands.Alpha.Single();
                Assert.That(alphaCommand.Easing, Is.EqualTo(Easing.InBack));
                Assert.That(alphaCommand.StartTime, Is.EqualTo(100));
                Assert.That(alphaCommand.EndTime, Is.EqualTo(200));
                Assert.That(alphaCommand.StartValue, Is.EqualTo(0));
                Assert.That(alphaCommand.EndValue, Is.EqualTo(1));

                var blendingCommand = decodedSprite.Commands.BlendingParameters.Single();
                Assert.That(blendingCommand.Easing, Is.EqualTo(Easing.None));
                Assert.That(blendingCommand.StartTime, Is.EqualTo(300));
                Assert.That(blendingCommand.EndTime, Is.EqualTo(300));
                Assert.That(blendingCommand.StartValue, Is.EqualTo(BlendingParameters.Additive));
                Assert.That(blendingCommand.EndValue, Is.EqualTo(BlendingParameters.Additive));

                var colourCommand = decodedSprite.Commands.Colour.Single();
                Assert.That(colourCommand.Easing, Is.EqualTo(Easing.InCubic));
                Assert.That(colourCommand.StartTime, Is.EqualTo(400));
                Assert.That(colourCommand.EndTime, Is.EqualTo(500));
                Assert.That(colourCommand.StartValue, Is.EqualTo(Color4.White));
                Assert.That(colourCommand.EndValue, Is.EqualTo(Color4.Aquamarine));

                var flipHCommand = decodedSprite.Commands.FlipH.Single();
                Assert.That(flipHCommand.Easing, Is.EqualTo(Easing.InOutQuad));
                Assert.That(flipHCommand.StartTime, Is.EqualTo(600));
                Assert.That(flipHCommand.EndTime, Is.EqualTo(600));
                Assert.That(flipHCommand.StartValue, Is.EqualTo(true));
                Assert.That(flipHCommand.EndValue, Is.EqualTo(true));

                var flipVCommand = decodedSprite.Commands.FlipV.Single();
                Assert.That(flipVCommand.Easing, Is.EqualTo(Easing.InOutQuad));
                Assert.That(flipVCommand.StartTime, Is.EqualTo(800));
                Assert.That(flipVCommand.EndTime, Is.EqualTo(900));
                Assert.That(flipVCommand.StartValue, Is.EqualTo(true));
                Assert.That(flipVCommand.EndValue, Is.EqualTo(false));

                var rotationCommand = decodedSprite.Commands.Rotation.Single();
                Assert.That(rotationCommand.Easing, Is.EqualTo(Easing.OutSine));
                Assert.That(rotationCommand.StartTime, Is.EqualTo(1000));
                Assert.That(rotationCommand.EndTime, Is.EqualTo(1100));
                Assert.That(rotationCommand.StartValue, Is.EqualTo(0));
                Assert.That(rotationCommand.EndValue, Is.EqualTo(720));

                var scaleCommand = decodedSprite.Commands.Scale.Single();
                Assert.That(scaleCommand.Easing, Is.EqualTo(Easing.OutQuint));
                Assert.That(scaleCommand.StartTime, Is.EqualTo(1200));
                Assert.That(scaleCommand.EndTime, Is.EqualTo(1300));
                Assert.That(scaleCommand.StartValue, Is.EqualTo(1));
                Assert.That(scaleCommand.EndValue, Is.EqualTo(4));

                var vectorScaleCommand = decodedSprite.Commands.VectorScale.Single();
                Assert.That(vectorScaleCommand.Easing, Is.EqualTo(Easing.InCirc));
                Assert.That(vectorScaleCommand.StartTime, Is.EqualTo(1400));
                Assert.That(vectorScaleCommand.EndTime, Is.EqualTo(1500));
                Assert.That(vectorScaleCommand.StartValue, Is.EqualTo(new Vector2(4)));
                Assert.That(vectorScaleCommand.EndValue, Is.EqualTo(new Vector2(3, 1)));

                var xCommand = decodedSprite.Commands.X.Single();
                Assert.That(xCommand.Easing, Is.EqualTo(Easing.InOutQuad));
                Assert.That(xCommand.StartTime, Is.EqualTo(1600));
                Assert.That(xCommand.EndTime, Is.EqualTo(1700));
                Assert.That(xCommand.StartValue, Is.EqualTo(300));
                Assert.That(xCommand.EndValue, Is.EqualTo(500));

                var yCommand = decodedSprite.Commands.Y.Single();
                Assert.That(yCommand.Easing, Is.EqualTo(Easing.OutBounce));
                Assert.That(yCommand.StartTime, Is.EqualTo(1800));
                Assert.That(yCommand.EndTime, Is.EqualTo(1800));
                Assert.That(yCommand.StartValue, Is.EqualTo(300));
                Assert.That(yCommand.EndValue, Is.EqualTo(100));
            });
        }

        [Test]
        public void TestLoopingGroup()
        {
            var initial = createComponents();

            var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, "test.jpg", Anchor.Centre, new Vector2(300));
            var loopingGroup = sprite.AddLoopingGroup(1000, 44);
            loopingGroup.AddAlpha(Easing.OutQuint, 1000, 1500, 0, 1);
            initial.Storyboard.GetLayer("Background").Add(sprite);

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            var decodedSprite = (StoryboardSprite)decodedAfterEncode.Storyboard.GetLayer("Background").Elements.Single();

            Assert.Multiple(() =>
            {
                Assert.That(decodedSprite.LoopingGroups, Has.Count.EqualTo(1));
                var decodedLoopingGroup = decodedSprite.LoopingGroups.Single();
                Assert.That(decodedLoopingGroup.StartTime, Is.EqualTo(1000));
                Assert.That(decodedLoopingGroup.TotalIterations, Is.EqualTo(45));

                var alphaCommand = decodedLoopingGroup.Alpha.Single();
                Assert.That(alphaCommand.Easing, Is.EqualTo(Easing.OutQuint));
                Assert.That(alphaCommand.StartTime, Is.EqualTo(1000));
                Assert.That(alphaCommand.EndTime, Is.EqualTo(1500));
                Assert.That(alphaCommand.StartValue, Is.EqualTo(0));
                Assert.That(alphaCommand.EndValue, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestTriggerGroup()
        {
            var initial = createComponents();

            var sprite = new StoryboardSprite(StoryboardElementSource.Beatmap, "test.jpg", Anchor.Centre, new Vector2(300));
            var triggerGroup = sprite.AddTriggerGroup("Passing", 0, 100000, 33);
            triggerGroup.AddAlpha(Easing.OutQuint, 0, 500, 0, 1);
            initial.Storyboard.GetLayer("Background").Add(sprite);

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            var decodedSprite = (StoryboardSprite)decodedAfterEncode.Storyboard.GetLayer("Background").Elements.Single();

            Assert.Multiple(() =>
            {
                Assert.That(decodedSprite.TriggerGroups, Has.Count.EqualTo(1));
                var decodedTriggerGroup = decodedSprite.TriggerGroups.Single();
                Assert.That(decodedTriggerGroup.TriggerName, Is.EqualTo("Passing"));
                Assert.That(decodedTriggerGroup.TriggerStartTime, Is.EqualTo(0));
                Assert.That(decodedTriggerGroup.TriggerEndTime, Is.EqualTo(100000));
                Assert.That(decodedTriggerGroup.GroupNumber, Is.EqualTo(33));

                var alphaCommand = decodedTriggerGroup.Alpha.Single();
                Assert.That(alphaCommand.Easing, Is.EqualTo(Easing.OutQuint));
                Assert.That(alphaCommand.StartTime, Is.EqualTo(0));
                Assert.That(alphaCommand.EndTime, Is.EqualTo(500));
                Assert.That(alphaCommand.StartValue, Is.EqualTo(0));
                Assert.That(alphaCommand.EndValue, Is.EqualTo(1));
            });
        }

        [Test]
        public void TestStoryboardSamples()
        {
            var initial = createComponents();

            initial.Storyboard.GetLayer("Pass").Add(new StoryboardSampleInfo(StoryboardElementSource.Beatmap, "pass.wav", 4000, 85));
            initial.Storyboard.GetLayer("Fail").Add(new StoryboardSampleInfo(StoryboardElementSource.Shared, "fail.wav", 4000, 100));

            var encoded = encode(initial);
            var decodedAfterEncode = decode(encoded);

            Assert.Multiple(() =>
            {
                var passingSample = (StoryboardSampleInfo)decodedAfterEncode.Storyboard.GetLayer("Pass").Elements.Single();
                Assert.That(passingSample.Source, Is.EqualTo(StoryboardElementSource.Beatmap));
                Assert.That(passingSample.Path, Is.EqualTo("pass.wav"));
                Assert.That(passingSample.StartTime, Is.EqualTo(4000));
                Assert.That(passingSample.Volume, Is.EqualTo(85));

                var failingSample = (StoryboardSampleInfo)decodedAfterEncode.Storyboard.GetLayer("Fail").Elements.Single();
                Assert.That(failingSample.Source, Is.EqualTo(StoryboardElementSource.Shared));
                Assert.That(failingSample.Path, Is.EqualTo("fail.wav"));
                Assert.That(failingSample.StartTime, Is.EqualTo(4000));
                Assert.That(failingSample.Volume, Is.EqualTo(100));
            });
        }

        private record DecodedBeatmapComponents(IBeatmap Beatmap, Storyboard Storyboard);

        private record EncodedBeatmapComponents(MemoryStream Beatmap, MemoryStream Storyboard);

        private DecodedBeatmapComponents createComponents()
        {
            var beatmapInfo = new BeatmapInfo();
            var beatmap = new Beatmap
            {
                BeatmapInfo = beatmapInfo
            };
            var storyboard = new Storyboard
            {
                Beatmap = beatmap,
                BeatmapInfo = beatmapInfo
            };

            return new DecodedBeatmapComponents(beatmap, storyboard);
        }

        private EncodedBeatmapComponents encode(DecodedBeatmapComponents decoded)
        {
            var beatmapStream = new MemoryStream();
            using (var beatmapWriter = new StreamWriter(beatmapStream, Encoding.UTF8, 1024, leaveOpen: true))
                new LegacyBeatmapEncoder(decoded.Beatmap, null, decoded.Storyboard).Encode(beatmapWriter);
            beatmapStream.Position = 0;

            var storyboardStream = new MemoryStream();
            using (var storyboardWriter = new StreamWriter(storyboardStream, Encoding.UTF8, 1024, leaveOpen: true))
                new LegacyStoryboardEncoder(decoded.Storyboard).EncodeStandaloneStoryboard(storyboardWriter);
            storyboardStream.Position = 0;

            return new EncodedBeatmapComponents(beatmapStream, storyboardStream);
        }

        private DecodedBeatmapComponents decode(EncodedBeatmapComponents encoded)
        {
            using var beatmapReader = new LineBufferedReader(encoded.Beatmap, leaveOpen: true);
            var beatmap = new LegacyBeatmapDecoder().Decode(beatmapReader);

            encoded.Beatmap.Position = 0;
            using var storyboardReader = new LineBufferedReader(encoded.Storyboard, leaveOpen: true);
            var storyboard = new LegacyStoryboardDecoder().Decode(beatmapReader, storyboardReader);

            encoded.Beatmap.Position = 0;
            encoded.Storyboard.Position = 0;

            return new DecodedBeatmapComponents(beatmap, storyboard);
        }
    }
}
