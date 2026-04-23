// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Intro;

namespace osu.Game.Tests.Visual.RankedPlay
{
    [TestFixture]
    public partial class TestSceneStarRatingSequence : RankedPlayTestScene
    {
        [Test]
        public void TestBasicAppearance()
        {
            float starRating = 5;

            AddSliderStep("set star rating", 0f, 10, 5, sr => starRating = sr);
            AddStep("play sequence", () =>
            {
                StarRatingSequence sequence;

                Child = sequence = new StarRatingSequence
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                };
                double delay = 0;
                sequence.Play(ref delay, starRating);
            });
        }
    }
}
