// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Newtonsoft.Json;
using NUnit.Framework;
using osu.Game.IO.Serialization;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Online
{
    /// <summary>
    /// Basic testing to ensure our attribute-based naming is correctly working.
    /// </summary>
    [TestFixture]
    public class TestSoloScoreInfoJsonSerialization
    {
        [Test]
        public void TestScoreSerialisationViaExtensionMethod()
        {
            var score = SoloScoreInfo.ForSubmission(TestResources.CreateTestScoreInfo());

            string serialised = score.Serialize();

            Assert.That(serialised, Contains.Substring("large_tick_hit"));
            Assert.That(serialised, Contains.Substring("\"rank\": \"S\""));
        }

        [Test]
        public void TestScoreSerialisationWithoutSettings()
        {
            var score = SoloScoreInfo.ForSubmission(TestResources.CreateTestScoreInfo());

            string serialised = JsonConvert.SerializeObject(score);

            Assert.That(serialised, Contains.Substring("large_tick_hit"));
            Assert.That(serialised, Contains.Substring("\"rank\":\"S\""));
        }
    }
}
