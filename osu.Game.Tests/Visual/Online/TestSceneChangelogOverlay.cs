// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Changelog;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChangelogOverlay : OsuTestScene
    {
        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;

        private readonly Dictionary<string, APIUpdateStream> streams;
        private readonly Dictionary<string, APIChangelogBuild> builds;

        private APIChangelogBuild requestedBuild;
        private TestChangelogOverlay changelog;

        public TestSceneChangelogOverlay()
        {
            streams = APIUpdateStream.KNOWN_STREAMS.Keys.Select((stream, id) => new APIUpdateStream
            {
                Id = id + 1,
                Name = stream,
                DisplayName = stream.Humanize(), // not quite there, but good enough.
            }).ToDictionary(stream => stream.Name);

            string version = DateTimeOffset.Now.ToString("yyyy.Mdd.0");
            builds = APIUpdateStream.KNOWN_STREAMS.Keys.Select(stream => new APIChangelogBuild
            {
                Version = version,
                DisplayVersion = version,
                UpdateStream = streams[stream],
                ChangelogEntries = new List<APIChangelogEntry>()
            }).ToDictionary(build => build.UpdateStream.Name);

            foreach (var stream in streams.Values)
                stream.LatestBuild = builds[stream.Name];
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            requestedBuild = null;

            dummyAPI.HandleRequest = request =>
            {
                switch (request)
                {
                    case GetChangelogRequest changelogRequest:
                        var changelogResponse = new APIChangelogIndex
                        {
                            Streams = streams.Values.ToList(),
                            Builds = builds.Values.ToList()
                        };
                        changelogRequest.TriggerSuccess(changelogResponse);
                        return true;

                    case GetChangelogBuildRequest buildRequest:
                        if (requestedBuild != null)
                            buildRequest.TriggerSuccess(requestedBuild);
                        return true;
                }

                return false;
            };

            Child = changelog = new TestChangelogOverlay();
        });

        [Test]
        public void ShowWithNoFetch()
        {
            AddStep(@"Show", () => changelog.Show());
            AddUntilStep(@"wait for streams", () => changelog.Streams?.Count > 0);
            AddAssert(@"listing displayed", () => changelog.Current.Value == null);
            AddAssert(@"no stream selected", () => changelog.Header.Streams.Current.Value == null);
        }

        [Test]
        public void ShowWithListing()
        {
            AddStep(@"Show with listing", () => changelog.ShowListing());
            AddUntilStep(@"wait for streams", () => changelog.Streams?.Count > 0);
            AddAssert(@"listing displayed", () => changelog.Current.Value == null);
            AddAssert(@"no stream selected", () => changelog.Header.Streams.Current.Value == null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ShowWithBuild(bool isSupporter)
        {
            AddStep(@"set supporter", () => dummyAPI.LocalUser.Value.IsSupporter = isSupporter);
            showBuild(() => new APIChangelogBuild
            {
                Version = "2018.712.0",
                DisplayVersion = "2018.712.0",
                UpdateStream = streams[OsuGameBase.CLIENT_STREAM_NAME],
                CreatedAt = new DateTime(2018, 7, 12),
                ChangelogEntries = new List<APIChangelogEntry>
                {
                    new APIChangelogEntry
                    {
                        Type = ChangelogEntryType.Fix,
                        Category = "osu!",
                        Title = "Fix thing",
                        MessageHtml = "Additional info goes here.",
                        Repository = "osu",
                        GithubPullRequestId = 11100,
                        GithubUser = new APIChangelogUser
                        {
                            OsuUsername = "smoogipoo",
                            UserId = 1040328
                        }
                    },
                    new APIChangelogEntry
                    {
                        Type = ChangelogEntryType.Add,
                        Category = "osu!",
                        Title = "Add thing",
                        Major = true,
                        Repository = "ppy/osu-framework",
                        GithubPullRequestId = 4444,
                        GithubUser = new APIChangelogUser
                        {
                            DisplayName = "frenzibyte",
                            GithubUrl = "https://github.com/frenzibyte"
                        }
                    },
                    new APIChangelogEntry
                    {
                        Type = ChangelogEntryType.Misc,
                        Category = "Code quality",
                        Title = "Clean up thing",
                        GithubUser = new APIChangelogUser
                        {
                            DisplayName = "some dude"
                        }
                    },
                    new APIChangelogEntry
                    {
                        Type = ChangelogEntryType.Misc,
                        Category = "Code quality",
                        Title = "Clean up another thing"
                    },
                    new APIChangelogEntry
                    {
                        Type = ChangelogEntryType.Add,
                        Category = "osu!",
                        Title = "Add entry with news url",
                        Url = "https://osu.ppy.sh/home/news/2023-07-27-summer-splash"
                    },
                }
            });

            AddUntilStep(@"wait for streams", () => changelog.Streams?.Count > 0);
            AddAssert(@"correct build displayed", () => changelog.Current.Value.Version == "2018.712.0");
            AddAssert(@"correct stream selected", () => changelog.Header.Streams.Current.Value.Id == 5);
            AddUntilStep(@"wait for content load", () => changelog.ChildrenOfType<ChangelogSupporterPromo>().Any());
            AddAssert(@"supporter promo showed", () => changelog.ChildrenOfType<ChangelogSupporterPromo>().First().Alpha == (isSupporter ? 0 : 1));
        }

        [Test]
        public void TestHTMLUnescaping()
        {
            showBuild(() => new APIChangelogBuild
            {
                Version = "2019.920.0",
                DisplayVersion = "2019.920.0",
                CreatedAt = new DateTime(2019, 9, 20),
                UpdateStream = new APIUpdateStream
                {
                    Name = "Test",
                    DisplayName = "Test"
                },
                ChangelogEntries = new List<APIChangelogEntry>
                {
                    new APIChangelogEntry
                    {
                        Category = "Testing HTML strings unescaping",
                        Title = "Ensuring HTML strings are being unescaped",
                        MessageHtml = "&quot;&quot;&quot;This text should appear triple-quoted&quot;&quot;&quot;    &gt;_&lt;",
                        GithubUser = new APIChangelogUser
                        {
                            DisplayName = "Dummy",
                            OsuUsername = "Dummy",
                        }
                    },
                }
            });
        }

        private void showBuild(Func<APIChangelogBuild> build)
        {
            AddStep("set up build", () => requestedBuild = build.Invoke());
            AddStep("show build", () => changelog.ShowBuild(requestedBuild));
        }

        private partial class TestChangelogOverlay : ChangelogOverlay
        {
            public new List<APIUpdateStream> Streams => base.Streams;

            public new ChangelogHeader Header => base.Header;
        }
    }
}
