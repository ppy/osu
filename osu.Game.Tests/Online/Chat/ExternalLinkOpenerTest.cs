// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Moq;
using NUnit.Framework;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online.Chat
{
    [HeadlessTest]
    public partial class ExternalLinkOpenerTest : OsuTestScene
    {
        private Mock<GameHost> gameHostMock = null!;
        private Mock<IDialogOverlay> dialogOverlayMock = null!;
        private Mock<INotificationOverlay> notificationOverlayMock = null!;
        private ExternalLinkOpener externalLinkOpener = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("reset mocks", () =>
            {
                gameHostMock = new Mock<GameHost>(@"mock-host", new HostOptions());
                dialogOverlayMock = new Mock<IDialogOverlay>();
                notificationOverlayMock = new Mock<INotificationOverlay>();
            });
            AddStep("enable external link warning", () => config.SetValue(OsuSetting.ExternalLinkWarning, true));
            AddStep("create link opener", () => Child = new DependencyProvidingContainer
            {
                CachedDependencies =
                [
                    (typeof(GameHost), gameHostMock.Object),
                    (typeof(IDialogOverlay), dialogOverlayMock.Object),
                    (typeof(INotificationOverlay), notificationOverlayMock.Object),
                ],
                Child = externalLinkOpener = new ExternalLinkOpener(),
            });
        }

        [Test]
        public void ValidRootedSubpath()
        {
            AddStep("open /home", () => externalLinkOpener.OpenUrlExternally(@"/home"));
            AddStep("link opened", () => gameHostMock.Verify(h => h.OpenUrlExternally($@"{API.Endpoints.WebsiteUrl}/home"), Times.Once));
        }

        [Test]
        public void ValidAbsoluteHttpsUrl()
        {
            AddStep(@$"open {API.Endpoints.WebsiteUrl}/home", () => externalLinkOpener.OpenUrlExternally(@$"{API.Endpoints.WebsiteUrl}/home"));
            AddStep("link opened", () => gameHostMock.Verify(h => h.OpenUrlExternally($@"{API.Endpoints.WebsiteUrl}/home"), Times.Once));
        }

        [Test]
        public void CaseInsensitivity()
        {
            AddStep(@$"open {API.Endpoints.WebsiteUrl.ToUpperInvariant()}/HOME", () => externalLinkOpener.OpenUrlExternally(@$"{API.Endpoints.WebsiteUrl.ToUpperInvariant()}/HOME"));
            AddStep("link opened", () => gameHostMock.Verify(h => h.OpenUrlExternally($@"{API.Endpoints.WebsiteUrl}/HOME"), Times.Once));
        }

        [Test]
        public void ValidMailto()
        {
            AddStep("open mailto:user@mail.test", () => externalLinkOpener.OpenUrlExternally(@"mailto:user@fake.mail"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("warning triggered", () => dialogOverlayMock.Verify(d => d.Push(It.IsAny<ExternalLinkOpener.ExternalLinkDialog>()), Times.Once));
        }

        [Test]
        public void InvalidSchemeRejected()
        {
            AddStep("open file:///home", () => externalLinkOpener.OpenUrlExternally(@"file:///home", LinkWarnMode.NeverWarn));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("dangerous protocol notification posted", () => notificationOverlayMock.Verify(n => n.Post(It.IsAny<SimpleErrorNotification>()), Times.Once));
        }

        [Test]
        public void UntrustedWebsiteTriggersWarning()
        {
            AddStep("open https://awebsite.test", () => externalLinkOpener.OpenUrlExternally(@"https://awebsite.test"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("warning triggered", () => dialogOverlayMock.Verify(d => d.Push(It.IsAny<ExternalLinkOpener.ExternalLinkDialog>()), Times.Once));
        }

        [Test]
        public void MatchingPrefixDoesNotFoolWarning()
        {
            AddStep($@"open {API.Endpoints.WebsiteUrl}.evil.test", () => externalLinkOpener.OpenUrlExternally($@"{API.Endpoints.WebsiteUrl}.evil.test"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("warning triggered", () => dialogOverlayMock.Verify(d => d.Push(It.IsAny<ExternalLinkOpener.ExternalLinkDialog>()), Times.Once));
        }

        [Test]
        public void ProtocolRelativeUriDoesNotFoolWarning()
        {
            AddStep("open //evil.test", () => externalLinkOpener.OpenUrlExternally("//evil.test"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("warning triggered", () => dialogOverlayMock.Verify(d => d.Push(It.IsAny<ExternalLinkOpener.ExternalLinkDialog>()), Times.Once));
        }

        [Test]
        public void CustomPortOnWebsiteRejected()
        {
            AddStep($@"open {API.Endpoints.WebsiteUrl}:333", () => externalLinkOpener.OpenUrlExternally($@"{API.Endpoints.WebsiteUrl}:333"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("dangerous protocol notification posted", () => notificationOverlayMock.Verify(n => n.Post(It.IsAny<SimpleErrorNotification>()), Times.Once));
        }

        [Test]
        public void CustomPortOnWebsiteAcceptedIfSpecifiedInEndpointsConfig()
        {
            string originalWebsiteUrl = API.Endpoints.WebsiteUrl;

            AddStep("append port to website url", () => API.Endpoints.WebsiteUrl = @"http://localhost:8080");
            AddStep($@"open {API.Endpoints.WebsiteUrl}/test", () => externalLinkOpener.OpenUrlExternally($@"{API.Endpoints.WebsiteUrl}/test"));
            AddStep("link opened", () => gameHostMock.Verify(h => h.OpenUrlExternally($@"{API.Endpoints.WebsiteUrl}/test"), Times.Once));
            AddStep("revert to original website url", () => API.Endpoints.WebsiteUrl = originalWebsiteUrl);
        }

        [Test]
        public void CustomPortOnWebsiteRejectedOnMismatch()
        {
            string originalWebsiteUrl = API.Endpoints.WebsiteUrl;

            AddStep("append port to website url", () => API.Endpoints.WebsiteUrl = @"http://localhost:8080");
            AddStep(@"open http://localhost:8181/test", () => externalLinkOpener.OpenUrlExternally("http://localhost:8181/test"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("dangerous protocol notification posted", () => notificationOverlayMock.Verify(n => n.Post(It.IsAny<SimpleErrorNotification>()), Times.Once));
            AddStep("revert to original website url", () => API.Endpoints.WebsiteUrl = originalWebsiteUrl);
        }

        [Test]
        public void CustomPortOnExternalWebsiteRejected()
        {
            AddStep(@"open https://fake.test:333", () => externalLinkOpener.OpenUrlExternally("https://fake.test:333"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("dangerous protocol notification posted", () => notificationOverlayMock.Verify(n => n.Post(It.IsAny<SimpleErrorNotification>()), Times.Once));
        }

        [Test]
        public void CredentialsRejected()
        {
            AddStep(@"open https://user:pass@localhost/test/", () => externalLinkOpener.OpenUrlExternally(@"https://user:pass@localhost/test/"));
            AddStep("link not opened", () => gameHostMock.Verify(h => h.OpenUrlExternally(It.IsAny<string>()), Times.Never));
            AddStep("dangerous protocol notification posted", () => notificationOverlayMock.Verify(n => n.Post(It.IsAny<SimpleErrorNotification>()), Times.Once));
        }
    }
}
