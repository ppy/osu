// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Database;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// Manages replay bookmarks for a replay session.
    /// Bookmarks are persisted when the score is stored in the database (e.g. not for autoplay).
    /// </summary>
    public partial class ReplayBookmarkController : Component
    {
        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private ReplaySeekController seekController { get; set; } = null!;

        [Resolved]
        private ScoreInfo scoreInfo { get; set; } = null!;

        public readonly BindableList<int> Bookmarks = new BindableList<int>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Bookmarks.AddRange(scoreInfo.ReplayBookmarks);

            var id = scoreInfo.ID;
            Bookmarks.BindCollectionChanged((_, _) =>
            {
                int[] current = Bookmarks.ToArray();
                realm.Write(r =>
                {
                    var s = r.Find<ScoreInfo>(id);
                    if (s == null) return;

                    s.ReplayBookmarks.Clear();
                    foreach (int b in current)
                        s.ReplayBookmarks.Add(b);
                });
            });
        }

        public void AddBookmarkAtCurrentTime()
        {
            int bookmark = (int)seekController.CurrentTime;
            int idx = Bookmarks.BinarySearch(bookmark);
            if (idx < 0)
                Bookmarks.Insert(~idx, bookmark);
        }

        public void RemoveClosestBookmark()
        {
            if (!Bookmarks.Any(b => Math.Abs(b - seekController.CurrentTime) < 2000))
                return;

            int closestBookmark = Bookmarks.MinBy(b => Math.Abs(b - seekController.CurrentTime));
            Bookmarks.Remove(closestBookmark);
        }

        public void SeekBookmark(int direction)
        {
            // In the case of a backwards seek while playing, it can be hard to jump before a bookmark.
            // Adding some lenience here makes it more user-friendly.
            double seekLenience = seekController.IsRunning ? 1000 * seekController.Rate : 0;

            int? targetBookmark = direction < 1
                ? Bookmarks.Cast<int?>().LastOrDefault(b => b < seekController.CurrentTime - seekLenience)
                : Bookmarks.Cast<int?>().FirstOrDefault(b => b > seekController.CurrentTime);

            if (targetBookmark != null)
                seekController.SeekSmoothlyTo(targetBookmark.Value);
        }
    }
}
