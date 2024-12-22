// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Audio
{
    public partial class TrackRow : FillFlowContainer
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        private string[] samples;

        public TrackRow(string[] samples)
        {
            RelativeSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            this.samples = samples;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            editorBeatmap.TransactionEnded += updateTicks;
            updateTicks();
        }

        private void updateTicks()
        {
            Children = [];

            editorBeatmap.HitObjects.ForEach(hitObject =>
            {
                Add(new TrackTick
                {
                    FillAspectRatio = 1,
                    Width = 50,
                    RelativeSizeAxes = Axes.Y,
                });
            });
        }
    }
}
