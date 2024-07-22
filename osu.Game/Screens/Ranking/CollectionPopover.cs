// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Ranking
{
    public partial class CollectionPopover : OsuPopover
    {
        private readonly BeatmapInfo beatmapInfo;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        public CollectionPopover(BeatmapInfo beatmapInfo)
            : base(false)
        {
            this.beatmapInfo = beatmapInfo;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Margin = new MarginPadding(5);
            Body.CornerRadius = 4;

            Children = new[]
            {
                new OsuMenu(Direction.Vertical, true)
                {
                    Items = items,
                },
            };
        }

        protected override void OnFocusLost(FocusLostEvent e)
        {
            base.OnFocusLost(e);
            Hide();
        }

        private OsuMenuItem[] items
        {
            get
            {
                var collectionItems = realm.Realm.All<BeatmapCollection>()
                                           .OrderBy(c => c.Name)
                                           .AsEnumerable()
                                           .Select(c => new CollectionToggleMenuItem(c.ToLive(realm), beatmapInfo)).Cast<OsuMenuItem>().ToList();

                if (manageCollectionsDialog != null)
                    collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, manageCollectionsDialog.Show));

                return collectionItems.ToArray();
            }
        }
    }
}
