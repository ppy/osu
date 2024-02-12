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
            var localLookupResult = new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked };
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out localLookupResult))
                                         .Returns(true);

            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: false);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            localCachedMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata>.IsAny!), Times.Once);
            apiMetadataSourceMock.Verify(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out It.Ref<OnlineBeatmapMetadata>.IsAny!), Times.Never);
        }

        [Test]
        public void TestAPIQueriedSecond()
        {
            OnlineBeatmapMetadata? localLookupResult = null;
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out localLookupResult))
                                         .Returns(false);

            var onlineLookupResult = new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked };
            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);
            apiMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out onlineLookupResult))
                                 .Returns(true);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: false);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            localCachedMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Once);
            apiMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Once);
        }

        [Test]
        public void TestPreferOnlineFetch()
        {
            var localLookupResult = new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked };
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out localLookupResult))
                                         .Returns(true);

            var onlineLookupResult = new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Graveyard };
            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);
            apiMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out onlineLookupResult))
                                 .Returns(true);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: true);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Graveyard));
            localCachedMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Never);
            apiMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Once);
        }

        [Test]
        public void TestPreferOnlineFetchFallsBackToLocalCacheIfOnlineSourceUnavailable()
        {
            var localLookupResult = new OnlineBeatmapMetadata { BeatmapID = 123456, BeatmapStatus = BeatmapOnlineStatus.Ranked };
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out localLookupResult))
                                         .Returns(true);

            apiMetadataSourceMock.Setup(src => src.Available).Returns(false);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: true);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            localCachedMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Once);
            apiMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Never);
        }

        [Test]
        public void TestMetadataLookupFailed()
        {
            OnlineBeatmapMetadata? lookupResult = null;

            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                                         .Returns(false);

            apiMetadataSourceMock.Setup(src => src.Available).Returns(true);
            apiMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                                 .Returns(true);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch: false);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(beatmap.OnlineID, Is.EqualTo(-1));
            localCachedMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Once);
            apiMetadataSourceMock.Verify(src => src.TryLookup(beatmap, out It.Ref<OnlineBeatmapMetadata?>.IsAny!), Times.Once);
        }

        /// <remarks>
        /// For the time being, if we fail to find a match in the local cache but online retrieval is not available, we trust the incoming beatmap verbatim wrt online ID.
        /// While this is suboptimal as it implicitly trusts the contents of the beatmap,
        /// throwing away the online data would be anti-user as it would make all beatmaps imported offline stop working in online.
        /// TODO: revisit if/when we have a better flow of queueing metadata retrieval.
        /// </remarks>
        [Test]
        public void TestLocalMetadataLookupReturnedNoMatchAndOnlineLookupIsUnavailable([Values] bool preferOnlineFetch)
        {
            OnlineBeatmapMetadata? localLookupResult = null;
            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(true);
            localCachedMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out localLookupResult))
                                         .Returns(false);

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
        public void TestNoAvailableSources([Values] bool preferOnlineFetch)
        {
            OnlineBeatmapMetadata? lookupResult = null;

            localCachedMetadataSourceMock.Setup(src => src.Available).Returns(false);
            localCachedMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                                         .Returns(false);

            apiMetadataSourceMock.Setup(src => src.Available).Returns(false);
            apiMetadataSourceMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                                 .Returns(false);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(beatmap.OnlineID, Is.EqualTo(123456));
        }

        [Test]
        public void TestReturnedMetadataHasDifferentOnlineID([Values] bool preferOnlineFetch)
        {
            var lookupResult = new OnlineBeatmapMetadata { BeatmapID = 654321, BeatmapStatus = BeatmapOnlineStatus.Ranked };

            var targetMock = preferOnlineFetch ? apiMetadataSourceMock : localCachedMetadataSourceMock;
            targetMock.Setup(src => src.Available).Returns(true);
            targetMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                      .Returns(true);

            var beatmap = new BeatmapInfo { OnlineID = 123456 };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(beatmap.OnlineID, Is.EqualTo(-1));
        }

        [Test]
        public void TestMetadataLookupForBeatmapWithoutPopulatedIDAndCorrectHash([Values] bool preferOnlineFetch)
        {
            var lookupResult = new OnlineBeatmapMetadata
            {
                BeatmapID = 654321,
                BeatmapStatus = BeatmapOnlineStatus.Ranked,
                MD5Hash = @"deadbeef",
            };

            var targetMock = preferOnlineFetch ? apiMetadataSourceMock : localCachedMetadataSourceMock;
            targetMock.Setup(src => src.Available).Returns(true);
            targetMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                      .Returns(true);

            var beatmap = new BeatmapInfo
            {
                MD5Hash = @"deadbeef"
            };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            Assert.That(beatmap.OnlineID, Is.EqualTo(654321));
        }

        [Test]
        public void TestMetadataLookupForBeatmapWithoutPopulatedIDAndIncorrectHash([Values] bool preferOnlineFetch)
        {
            var lookupResult = new OnlineBeatmapMetadata
            {
                BeatmapID = 654321,
                BeatmapStatus = BeatmapOnlineStatus.Ranked,
                MD5Hash = @"cafebabe",
            };

            var targetMock = preferOnlineFetch ? apiMetadataSourceMock : localCachedMetadataSourceMock;
            targetMock.Setup(src => src.Available).Returns(true);
            targetMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                      .Returns(true);

            var beatmap = new BeatmapInfo
            {
                MD5Hash = @"deadbeef"
            };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(beatmap.OnlineID, Is.EqualTo(-1));
        }

        [Test]
        public void TestReturnedMetadataHasDifferentHash([Values] bool preferOnlineFetch)
        {
            var lookupResult = new OnlineBeatmapMetadata
            {
                BeatmapID = 654321,
                BeatmapStatus = BeatmapOnlineStatus.Ranked,
                MD5Hash = @"deadbeef"
            };

            var targetMock = preferOnlineFetch ? apiMetadataSourceMock : localCachedMetadataSourceMock;
            targetMock.Setup(src => src.Available).Returns(true);
            targetMock.Setup(src => src.TryLookup(It.IsAny<BeatmapInfo>(), out lookupResult))
                      .Returns(true);

            var beatmap = new BeatmapInfo
            {
                OnlineID = 654321,
                MD5Hash = @"cafebabe",
            };
            var beatmapSet = new BeatmapSetInfo(beatmap.Yield());
            beatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(beatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(beatmap.OnlineID, Is.EqualTo(654321));
        }

        [Test]
        public void TestPartiallyModifiedSet([Values] bool preferOnlineFetch)
        {
            var firstResult = new OnlineBeatmapMetadata
            {
                BeatmapID = 654321,
                BeatmapStatus = BeatmapOnlineStatus.Ranked,
                BeatmapSetStatus = BeatmapOnlineStatus.Ranked,
                MD5Hash = @"cafebabe"
            };
            var secondResult = new OnlineBeatmapMetadata
            {
                BeatmapID = 666666,
                BeatmapStatus = BeatmapOnlineStatus.Ranked,
                BeatmapSetStatus = BeatmapOnlineStatus.Ranked,
                MD5Hash = @"dededede"
            };

            var targetMock = preferOnlineFetch ? apiMetadataSourceMock : localCachedMetadataSourceMock;
            targetMock.Setup(src => src.Available).Returns(true);
            targetMock.Setup(src => src.TryLookup(It.Is<BeatmapInfo>(bi => bi.OnlineID == 654321), out firstResult))
                      .Returns(true);
            targetMock.Setup(src => src.TryLookup(It.Is<BeatmapInfo>(bi => bi.OnlineID == 666666), out secondResult))
                      .Returns(true);

            var firstBeatmap = new BeatmapInfo
            {
                OnlineID = 654321,
                MD5Hash = @"cafebabe",
            };
            var secondBeatmap = new BeatmapInfo
            {
                OnlineID = 666666,
                MD5Hash = @"deadbeef"
            };
            var beatmapSet = new BeatmapSetInfo(new[]
            {
                firstBeatmap,
                secondBeatmap
            });
            firstBeatmap.BeatmapSet = beatmapSet;
            secondBeatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(firstBeatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            Assert.That(firstBeatmap.OnlineID, Is.EqualTo(654321));

            Assert.That(secondBeatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(secondBeatmap.OnlineID, Is.EqualTo(666666));

            Assert.That(beatmapSet.Status, Is.EqualTo(BeatmapOnlineStatus.None));
        }

        [Test]
        public void TestPartiallyMaliciousSet([Values] bool preferOnlineFetch)
        {
            var firstResult = new OnlineBeatmapMetadata
            {
                BeatmapID = 654321,
                BeatmapStatus = BeatmapOnlineStatus.Ranked,
                BeatmapSetStatus = BeatmapOnlineStatus.Ranked,
                MD5Hash = @"cafebabe"
            };
            var secondResult = new OnlineBeatmapMetadata
            {
                BeatmapStatus = BeatmapOnlineStatus.Ranked,
                BeatmapSetStatus = BeatmapOnlineStatus.Ranked,
                MD5Hash = @"dededede"
            };

            var targetMock = preferOnlineFetch ? apiMetadataSourceMock : localCachedMetadataSourceMock;
            targetMock.Setup(src => src.Available).Returns(true);
            targetMock.Setup(src => src.TryLookup(It.Is<BeatmapInfo>(bi => bi.OnlineID == 654321), out firstResult))
                      .Returns(true);
            targetMock.Setup(src => src.TryLookup(It.Is<BeatmapInfo>(bi => bi.OnlineID == 666666), out secondResult))
                      .Returns(true);

            var firstBeatmap = new BeatmapInfo
            {
                OnlineID = 654321,
                MD5Hash = @"cafebabe",
            };
            var secondBeatmap = new BeatmapInfo
            {
                OnlineID = 666666,
                MD5Hash = @"deadbeef"
            };
            var beatmapSet = new BeatmapSetInfo(new[]
            {
                firstBeatmap,
                secondBeatmap
            });
            firstBeatmap.BeatmapSet = beatmapSet;
            secondBeatmap.BeatmapSet = beatmapSet;

            metadataLookup.Update(beatmapSet, preferOnlineFetch);

            Assert.That(firstBeatmap.Status, Is.EqualTo(BeatmapOnlineStatus.Ranked));
            Assert.That(firstBeatmap.OnlineID, Is.EqualTo(654321));

            Assert.That(secondBeatmap.Status, Is.EqualTo(BeatmapOnlineStatus.None));
            Assert.That(secondBeatmap.OnlineID, Is.EqualTo(-1));

            Assert.That(beatmapSet.Status, Is.EqualTo(BeatmapOnlineStatus.None));
        }
    }
}
