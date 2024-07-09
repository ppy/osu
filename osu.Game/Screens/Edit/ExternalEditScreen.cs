#nullable enable
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit
{
    internal partial class ExternalEditScreen : OsuScreen
    {
        private readonly Task<ExternalEditOperation<BeatmapSetInfo>> fileMountOperation;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        private readonly Editor? editor;

        public ExternalEditScreen(Task<ExternalEditOperation<BeatmapSetInfo>> fileMountOperation, Editor editor)
        {
            this.fileMountOperation = fileMountOperation;
            this.editor = editor;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            fileMountOperation.ContinueWith(t =>
            {
                var operation = t.GetResultSafely<ExternalEditOperation<BeatmapSetInfo>>();

                // Ensure the trailing separator is present in order to show the folder contents.
                gameHost.OpenFileExternally(operation.MountedPath.TrimDirectorySeparator() + Path.DirectorySeparatorChar);
            });

            InternalChildren = new Drawable[]
            {
                new SettingsButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = "end editing",
                    Action = finish,
                }
            };
        }

        private void finish()
        {
            fileMountOperation.GetResultSafely().Finish().ContinueWith(t =>
            {
                Schedule(() =>
                {
                    editor?.SwitchToDifficulty(t.GetResultSafely<Live<BeatmapSetInfo>>().Value.Detach().Beatmaps.First());
                });
            });
        }
    }
}
