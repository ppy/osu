// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Setup
{
    public class BackgroundChooser : CompositeDrawable, ICanAcceptFiles
    {
        public IEnumerable<string> HandledExtensions => ImageExtensions;

        public static string[] ImageExtensions { get; } = { ".jpg", ".jpeg", ".png" };

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; }

        private readonly Container content;

        public BackgroundChooser()
        {
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            updateBackgroundSprite();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            game.RegisterImportHandler(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            game?.UnregisterImportHandler(this);
        }

        Task ICanAcceptFiles.Import(params string[] paths)
        {
            Schedule(() =>
            {
                var firstFile = new FileInfo(paths.First());

                ChangeBackgroundImage(firstFile.FullName);
            });
            return Task.CompletedTask;
        }

        Task ICanAcceptFiles.Import(params ImportTask[] tasks) => throw new NotImplementedException();

        public bool ChangeBackgroundImage(string path)
        {
            var info = new FileInfo(path);

            if (!info.Exists)
                return false;

            var set = working.Value.BeatmapSetInfo;

            // remove the previous background for now.
            // in the future we probably want to check if this is being used elsewhere (other difficulties?)
            var oldFile = set.Files.FirstOrDefault(f => f.Filename == working.Value.Metadata.BackgroundFile);

            using (var stream = info.OpenRead())
            {
                if (oldFile != null)
                    beatmaps.ReplaceFile(set, oldFile, stream, info.Name);
                else
                    beatmaps.AddFile(set, stream, info.Name);
            }

            working.Value.Metadata.BackgroundFile = info.Name;
            updateBackgroundSprite();

            return true;
        }

        private void updateBackgroundSprite()
        {
            LoadComponentAsync(new BeatmapBackgroundSprite(working.Value)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            }, background =>
            {
                if (background.Texture != null)
                    content.Child = background;
                else
                {
                    content.Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeafoamDarker,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(size: 24))
                        {
                            Text = "Drag image here to set beatmap background!",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.X,
                        }
                    };
                }

                background.FadeInFromZero(500);
            });
        }
    }
}
