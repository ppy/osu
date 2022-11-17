// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public class LegacyScoreExporter : LegacyModelExporter<ScoreInfo>
    {
        protected override string FileExtension => ".osr";

        public LegacyScoreExporter(Storage storage, RealmAccess realm, ProgressNotification notification)
            : base(storage, realm, notification)
        {
        }

        //public override void ExportModelTo(ScoreInfo model, Stream outputStream)
        //{
        //    var file = model.Files.SingleOrDefault();
        //    if (file == null)
        //        return;
        //
        //    using (var inputStream = UserFileStorage.GetStream(file.File.GetStoragePath()))
        //        inputStream.CopyTo(outputStream);
        //
        //    Notification.State = ProgressNotificationState.Completed;
        //    outputStream.Dispose();
        //}
    }
}
