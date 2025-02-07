// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class AddPlaylistToCollectionButton : RoundedButton
    {
        private readonly Room room;

        private LoadingLayer loading = null!;

        [Resolved]
        private RealmAccess realmAccess { get; set; } = null!;

        [Resolved]
        private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private INotificationOverlay? notifications { get; set; }

        public AddPlaylistToCollectionButton(Room room)
        {
            this.room = room;
            Text = "Add Maps to Collection";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Gray5;

            Add(loading = new LoadingLayer(true, false));

            Action = () =>
            {
                int[] ids = room.Playlist.Select(item => item.Beatmap.OnlineID).Where(onlineId => onlineId > 0).ToArray();

                if (ids.Length == 0)
                {
                    notifications?.Post(new SimpleErrorNotification { Text = "Cannot add local beatmaps" });
                    return;
                }

                Enabled.Value = false;
                loading.Show();
                beatmapLookupCache.GetBeatmapsAsync(ids).ContinueWith(task => Schedule(() =>
                {
                    var beatmaps = task.GetResultSafely().Where(item => item?.BeatmapSet != null).ToList();

                    var collection = realmAccess.Realm.All<BeatmapCollection>().FirstOrDefault(c => c.Name == room.Name);

                    if (collection == null)
                    {
                        collection = new BeatmapCollection(room.Name, beatmaps.Select(i => i!.MD5Hash).Distinct().ToList());
                        realmAccess.Realm.Write(() => realmAccess.Realm.Add(collection));
                        notifications?.Post(new SimpleNotification { Text = $"Created new playlist: {room.Name}" });
                    }
                    else
                    {
                        collection.ToLive(realmAccess).PerformWrite(c =>
                        {
                            beatmaps = beatmaps.Where(i => !c.BeatmapMD5Hashes.Contains(i!.MD5Hash)).ToList();
                            foreach (var item in beatmaps)
                                c.BeatmapMD5Hashes.Add(item!.MD5Hash);
                            notifications?.Post(new SimpleNotification { Text = $"Updated playlist: {room.Name}" });
                        });
                    }

                    loading.Hide();
                    Enabled.Value = true;
                }), TaskContinuationOptions.OnlyOnRanToCompletion);
            };
        }
    }
}
