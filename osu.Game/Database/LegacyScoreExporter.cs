// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;

namespace osu.Game.Database
{
    public class LegacyScoreExporter : LegacyModelExporter<ScoreInfo>
    {
        protected override string FileExtension => ".osr";

        public LegacyScoreExporter(Storage storage, RealmAccess realm, ProgressNotification notification, Stream? stream = null)
            : base(storage, realm, notification, stream)
        {
        }

        public override async Task ExportASync(IHasGuidPrimaryKey uuid)
        {
            await Task.Run(() =>
            {
                RealmAccess.Run(r =>
                {
                    if (r.Find<ScoreInfo>(uuid.ID) is IHasNamedFiles model)
                    {
                        Filename = $"{model.GetDisplayString().GetValidFilename()}{FileExtension}";
                    }
                    else
                    {
                        return;
                    }

                    var file = model.Files.SingleOrDefault();
                    if (file == null)
                        return;

                    if (Notification.CancellationToken.IsCancellationRequested) return;

                    if (OutputStream == null)
                    {
                        OutputStream = ExportStorage.CreateFileSafely(Filename);
                        ShouldDisposeStream = true;
                    }

                    using (var inputStream = UserFileStorage.GetStream(file.File.GetStoragePath()))
                        inputStream.CopyTo(OutputStream);
                });
            }).ContinueWith(OnComplete);
        }
    }
}
