// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Playlists;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestSceneAddPlaylistToCollectionButton : OsuManualInputManagerTestScene
    {
        private RulesetStore rulesets = null!;
        private BeatmapManager manager = null!;
        private BeatmapSetInfo importedBeatmap = null!;
        private Room room = null!;
        private AddPlaylistToCollectionButton button = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, API, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            Add(notificationOverlay);
        }

        [Cached(typeof(INotificationOverlay))]
        private NotificationOverlay notificationOverlay = new NotificationOverlay
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
        };

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear realm", () => Realm.Realm.Write(() => Realm.Realm.RemoveAll<BeatmapCollection>()));

            AddStep("clear notifications", () =>
            {
                foreach (var notification in notificationOverlay.AllNotifications)
                    notification.Close(runFlingAnimation: false);
            });

            importBeatmap();

            setupRoom();

            AddStep("create button", () =>
            {
                Add(button = new AddPlaylistToCollectionButton(room)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(300, 40),
                });
            });
        }

        [Test]
        public void TestButtonFlow()
        {
            AddStep("move mouse to button", () => InputManager.MoveMouseTo(button));

            AddStep("click button", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("notification shown", () => notificationOverlay.AllNotifications.Any(n => n.Text.ToString().StartsWith("Created new collection", StringComparison.Ordinal)));

            AddUntilStep("realm is updated", () => Realm.Realm.All<BeatmapCollection>().FirstOrDefault(c => c.Name == room.Name) != null);
        }

        private void importBeatmap() => AddStep("import beatmap", () =>
        {
            var beatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);

            Debug.Assert(beatmap.BeatmapInfo.BeatmapSet != null);

            importedBeatmap = manager.Import(beatmap.BeatmapInfo.BeatmapSet)!.Value.Detach();
        });

        private void setupRoom() => AddStep("setup room", () =>
        {
            room = new Room
            {
                Name = "my awesome room",
                MaxAttempts = 5,
                Host = API.LocalUser.Value
            };
            room.RecentParticipants = [room.Host];
            room.EndDate = DateTimeOffset.Now.AddMinutes(5);
            room.Playlist =
            [
                new PlaylistItem(importedBeatmap.Beatmaps.First())
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                }
            ];
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }
    }
}
