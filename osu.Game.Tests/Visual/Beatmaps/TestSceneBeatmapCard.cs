// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public class TestSceneBeatmapCard : OsuTestScene
    {
        private IBeatmapSetInfo[] testCases;

        #region Test case generation

        [BackgroundDependencyLoader]
        private void load()
        {
            var normal = CreateAPIBeatmapSet(Ruleset.Value);
            normal.HasVideo = true;
            normal.HasStoryboard = true;

            var undownloadable = getUndownloadableBeatmapSet();
            var manyDifficulties = getManyDifficultiesBeatmapSet();

            var explicitMap = CreateAPIBeatmapSet(Ruleset.Value);
            explicitMap.HasExplicitContent = true;

            var featuredMap = CreateAPIBeatmapSet(Ruleset.Value);
            featuredMap.TrackId = 1;

            var explicitFeaturedMap = CreateAPIBeatmapSet(Ruleset.Value);
            explicitFeaturedMap.HasExplicitContent = true;
            explicitFeaturedMap.TrackId = 2;

            testCases = new IBeatmapSetInfo[]
            {
                normal,
                undownloadable,
                manyDifficulties,
                explicitMap,
                featuredMap,
                explicitFeaturedMap
            };
        }

        private APIBeatmapSet getUndownloadableBeatmapSet() => new APIBeatmapSet
        {
            OnlineID = 123,
            Title = "undownloadable beatmap",
            Artist = "test",
            Source = "more tests",
            Author = new User
            {
                Username = "BanchoBot",
                Id = 3,
            },
            Availability = new BeatmapSetOnlineAvailability
            {
                DownloadDisabled = true,
            },
            Preview = @"https://b.ppy.sh/preview/12345.mp3",
            PlayCount = 123,
            FavouriteCount = 456,
            BPM = 111,
            HasVideo = true,
            HasStoryboard = true,
            Covers = new BeatmapSetOnlineCovers(),
            Beatmaps = new List<APIBeatmap>
            {
                new APIBeatmap
                {
                    RulesetID = Ruleset.Value.OnlineID,
                    DifficultyName = "Test",
                    StarRating = 6.42,
                }
            }
        };

        private static APIBeatmapSet getManyDifficultiesBeatmapSet()
        {
            var beatmaps = new List<APIBeatmap>();

            for (int i = 0; i < 100; i++)
            {
                beatmaps.Add(new APIBeatmap
                {
                    RulesetID = i % 4,
                    StarRating = 2 + i % 4 * 2,
                });
            }

            return new APIBeatmapSet
            {
                OnlineID = 1,
                Title = "many difficulties beatmap",
                Artist = "test",
                Author = new User
                {
                    Username = "BanchoBot",
                    Id = 3,
                },
                HasVideo = true,
                HasStoryboard = true,
                Covers = new BeatmapSetOnlineCovers(),
                Beatmaps = beatmaps,
            };
        }

        #endregion

        private Drawable createContent(OverlayColourScheme colourScheme, Func<IBeatmapSetInfo, Drawable> creationFunc)
        {
            var colourProvider = new OverlayColourProvider(colourScheme);

            return new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(OverlayColourProvider), colourProvider)
                },
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5
                    },
                    new BasicScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Full,
                            Padding = new MarginPadding(10),
                            Spacing = new Vector2(10),
                            ChildrenEnumerable = testCases.Select(creationFunc)
                        }
                    }
                }
            };
        }

        private void createTestCase(Func<IBeatmapSetInfo, Drawable> creationFunc)
        {
            foreach (var scheme in Enum.GetValues(typeof(OverlayColourScheme)).Cast<OverlayColourScheme>())
                AddStep($"set {scheme} scheme", () => Child = createContent(scheme, creationFunc));
        }
    }
}
