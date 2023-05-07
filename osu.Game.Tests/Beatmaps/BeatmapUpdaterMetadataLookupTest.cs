// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Moq;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public class BeatmapUpdaterMetadataLookupTest
    {
        private Mock<IOnlineBeatmapMetadataSource> apiMetadataSourceMock = null!;
        private Mock<IOnlineBeatmapMetadataSource> localCachedMetadataSourceMock = null!;

        private BeatmapUpdaterMetadataLookup metadataLookup = null!;

        [SetUp]
        public void SetUp()
        {
            apiMetadataSourceMock = new Mock<IOnlineBeatmapMetadataSource>();
            localCachedMetadataSourceMock = new Mock<IOnlineBeatmapMetadataSource>();

            metadataLookup = new BeatmapUpdaterMetadataLookup(apiMetadataSourceMock.Object, localCachedMetadataSourceMock.Object);
        }

        [Test]
        public void TestLocalCacheQueriedFirst()
        {
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                         .Returns(new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked });
            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: false);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            localCachedMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Once);
            apiMetadataSourceMock.Verify(src => src.Lookup(It.IsAny<BeatmapInfo>()), Times.Never);
        }

        [Test]
        public void TestAPIQueriedSecond()
        {
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                         .Returns((OnlineBeatmapMetadata?)null);
            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);
            apiMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                 .Returns(new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked });

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: false);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            localCachedMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Once);
            apiMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Once);
        }

        [Test]
        public void TestPreferOnlineFetch()
        {
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                         .Returns(new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked });
            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);
            apiMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                 .Returns(new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Graveyard });

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: true);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Graveyard));
            localCachedMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Never);
            apiMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Once);
        }

        [Test]
        public void TestPreferOnlineFetchFallsBackToLocalCacheIfOnlineSourceUnavailable()
        {
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                         .Returns(new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked });
            apiMetadataSourceMock.Setup(src => src.Available).Returns(false);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: true);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            localCachedMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Once);
            apiMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Never);
        }

        [Test]
        public void TestMetadataLookupFailed()
        {
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                         .Returns((OnlineBeatmapMetadata?)null);
            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);
            apiMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                 .Returns((OnlineBeatmapMetadata?)null);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: false);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(beatmap.OnlineID, Is.EqualTo(-1));
            localCachedMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Once);
            apiMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Once);
        }

        /// <remarks>
        /// For the time being, if we fail to find a match in the local cache but online retrieval is not available, we trust the incoming beatmap verbatim wrt online ID.
        /// While this is suboptimal as it implicitly trusts the contents of the beatmap,
        /// throwing away the online data would be anti-user as it would make all beatmaps imported offline stop working in online.
        /// TODO: revisit if/when we have a better flow of queueing metadata retrieval.
        /// </remarks>
        [Test]
        public void TestLocalMetadataLookupFailedAndOnlineLookupIsUnavailable([Values] bool preferOnlineFetch)
        {
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.Lookup(It.IsAny<BeatmapInfo>()))
                                         .Returns((OnlineBeatmapMetadata?)null);
            apiMetadataSourceMock.Setup(src => src.Available).Returns(false);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(beatmap.OnlineID, Is.EqualTo(123456));
        }

        /// <remarks>
        /// For the time being, if there are no available metadata lookup sources, we trust the incoming beatmap verbatim wrt online ID.
        /// While this is suboptimal as it implicitly trusts the contents of the beatmap,
        /// throwing away the online data would be anti-user as it would make all beatmaps imported offline stop working in online.
        /// TODO: revisit if/when we have a better flow of queueing metadata retrieval.
        /// </remarks>
        [Test]
        public void TestNoAvailableSources()
        {
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(false);
            apiMetadataSourceMock.Setup(src => src.Available).Returns(false);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: false);

            Assert.That(beatmap.OnlineID, Is.EqualTo(123456));
            localCachedMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Never);
            apiMetadataSourceMock.Verify(src => src.Lookup(beatmap), Times.Never);
        }
    }
}
