// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public partial class PreviewTimePart : TimelinePart
    {
        private readonly BindableInt previewTime = new BindableInt();

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            previewTime.UnbindAll();
            previewTime.BindTo(beatmap.PreviewTime);
            previewTime.BindValueChanged(t =>
            {
                Clear();

                if (t.NewValue >= 0)
                    Add(new PreviewTimeVisualisation(t.NewValue));
            }, true);
        }

        private partial class PreviewTimeVisualisation : PointVisualisation
        {
            public PreviewTimeVisualisation(double time)
                : base(time)
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Green1;
        }
    }
}
