// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Changelog;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChangelogOverlay : OsuTestScene
    {
        private TestChangelogOverlay changelog;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChangelogUpdateStreamControl),
            typeof(ChangelogUpdateStreamItem),
            typeof(ChangelogHeader),
            typeof(ChangelogContent),
            typeof(ChangelogListing),
            typeof(ChangelogSingleBuild),
            typeof(ChangelogBuild),
            typeof(Comments),
        };

        protected override bool UseOnlineAPI => true;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
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

        [Test]
        public void ShowWithBuild()
        {
            AddStep(@"Show with Lazer 2018.712.0", () =>
            {
                changelog.ShowBuild(new APIChangelogBuild
                {
                    Version = "2018.712.0",
                    DisplayVersion = "2018.712.0",
                    UpdateStream = new APIUpdateStream { Id = 7, Name = OsuGameBase.CLIENT_STREAM_NAME },
                    ChangelogEntries = new List<APIChangelogEntry>
                    {
                        new APIChangelogEntry
                        {
                            Category = "Test",
                            Title = "Title",
                            MessageHtml = "Message",
                        }
                    }
                });
            });

            AddUntilStep(@"wait for streams", () => changelog.Streams?.Count > 0);
            AddAssert(@"correct build displayed", () => changelog.Current.Value.Version == "2018.712.0");
            AddAssert(@"correct stream selected", () => changelog.Header.Streams.Current.Value.Id == 7);
        }

        [Test]
        public void TestHTMLUnescaping()
        {
            AddStep(@"Ensure HTML string unescaping", () =>
            {
                changelog.ShowBuild(new APIChangelogBuild
                {
                    Version = "2019.920.0",
                    DisplayVersion = "2019.920.0",
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
            });
        }

        private class TestChangelogOverlay : ChangelogOverlay
        {
            public new List<APIUpdateStream> Streams => base.Streams;

            public new ChangelogHeader Header => base.Header;
        }
    }
}
