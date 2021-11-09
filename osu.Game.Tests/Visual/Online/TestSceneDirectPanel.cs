// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Rulesets;
using osuTK;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Online
{
    [Cached(typeof(IPreviewTrackOwner))]
    public class TestSceneDirectPanel : OsuTestScene, IPreviewTrackOwner
    {
        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var normal = getBeatmapSet();
            normal.HasVideo = true;
            normal.HasStoryboard = true;

            var undownloadable = getUndownloadableBeatmapSet();
            var manyDifficulties = getManyDifficultiesBeatmapSet();

            var explicitMap = getBeatmapSet();
            explicitMap.HasExplicitContent = true;

            var featuredMap = getBeatmapSet();
            featuredMap.TrackId = 1;

            var explicitFeaturedMap = getBeatmapSet();
            explicitFeaturedMap.HasExplicitContent = true;
            explicitFeaturedMap.TrackId = 2;

            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Full,
                    Padding = new MarginPadding(20),
                    Spacing = new Vector2(5, 20),
                    Children = new Drawable[]
                    {
                        new GridBeatmapPanel(normal),
                        new GridBeatmapPanel(undownloadable),
                        new GridBeatmapPanel(manyDifficulties),
                        new GridBeatmapPanel(explicitMap),
                        new GridBeatmapPanel(featuredMap),
                        new GridBeatmapPanel(explicitFeaturedMap),
                        new ListBeatmapPanel(normal),
                        new ListBeatmapPanel(undownloadable),
                        new ListBeatmapPanel(manyDifficulties),
                        new ListBeatmapPanel(explicitMap),
                        new ListBeatmapPanel(featuredMap),
                        new ListBeatmapPanel(explicitFeaturedMap)
                    },
                },
            };

            APIBeatmapSet getBeatmapSet() => CreateAPIBeatmapSet(Ruleset.Value);

            APIBeatmapSet getUndownloadableBeatmapSet() => new APIBeatmapSet
            {
                OnlineID = 123,
                Title = "undownloadable beatmap",
                Artist = "test",
                Source = "more tests",
                Author = new APIUser
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
                Beatmaps = new[]
                {
                    new APIBeatmap
                    {
                        RulesetID = Ruleset.Value.ID ?? 0,
                        DifficultyName = "Test",
                        StarRating = 6.42,
                    }
                }
            };

            APIBeatmapSet getManyDifficultiesBeatmapSet()
            {
                var beatmaps = new List<APIBeatmap>();

                for (int i = 0; i < 100; i++)
                {
                    beatmaps.Add(new APIBeatmap
                    {
                        RulesetID = i % 4,
                        StarRating = 2 + i % 4 * 2,
                        OverallDifficulty = 3.5f,
                    });
                }

                return new APIBeatmapSet
                {
                    OnlineID = 1,
                    Title = "undownloadable beatmap",
                    Artist = "test",
                    Source = "more tests",
                    Author = new APIUser
                    {
                        Username = "BanchoBot",
                        Id = 3,
                    },
                    HasVideo = true,
                    HasStoryboard = true,
                    Covers = new BeatmapSetOnlineCovers(),
                    Beatmaps = beatmaps.ToArray(),
                };
            }
        }
    }
}
