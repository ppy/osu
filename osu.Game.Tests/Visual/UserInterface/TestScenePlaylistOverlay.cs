// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Music;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestScenePlaylistOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PlaylistOverlay),
            typeof(Playlist)
        };

        private readonly BindableList<BeatmapSetInfo> beatmapSets = new BindableList<BeatmapSetInfo>();

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            PlaylistOverlay overlay;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300, 500),
                Child = overlay = new PlaylistOverlay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    State = { Value = Visibility.Visible }
                }
            };

            beatmapSets.Clear();

            for (int i = 0; i < 100; i++)
            {
                beatmapSets.Add(new BeatmapSetInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        // Create random metadata, then we can check if sorting works based on these
                        Artist = "Some Artist " + RNG.Next(0, 9),
                        Title = $"Some Song {i + 1}",
                        AuthorString = "Some Guy " + RNG.Next(0, 9),
                    },
                    DateAdded = DateTimeOffset.UtcNow,
                });
            }

            overlay.BeatmapSets.BindTo(beatmapSets);
        });
    }
}
