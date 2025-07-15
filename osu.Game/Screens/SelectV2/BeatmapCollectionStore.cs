// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Collections;
using osu.Game.Database;
using Realms;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapCollectionStore : Component
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private IDisposable? realmSubscription;

        private readonly BindableList<BeatmapCollection> collections = new BindableList<BeatmapCollection>();

        /// <summary>
        /// Gets a thread-safe bound copy to the list of <see cref="BeatmapCollection"/> present in the user's database.
        /// </summary>
        public IBindableList<BeatmapCollection> GetCollections()
        {
            lock (collections)
                return collections.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            realmSubscription = realm.RegisterForNotifications(r => r.All<BeatmapCollection>().OrderBy(c => c.Name), onChanged);
        }

        private void onChanged(IRealmCollection<BeatmapCollection> sender, ChangeSet? changes)
        {
            lock (collections)
            {
                collections.Clear();

                foreach (var c in sender)
                    collections.Add(c.Detach());
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            realmSubscription?.Dispose();
        }
    }
}
