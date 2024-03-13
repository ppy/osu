// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Humanizer;
using osu.Game.Database;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Collections
{
    public partial class DeleteCollectionDialog : DangerousActionDialog
    {
        public DeleteCollectionDialog(Live<BeatmapCollection> collection, Action deleteAction)
        {
            BodyText = collection.PerformRead(c => $"{c.Name} ({"beatmap".ToQuantity(c.BeatmapMD5Hashes.Count)})");
            DangerousAction = deleteAction;
        }
    }
}
