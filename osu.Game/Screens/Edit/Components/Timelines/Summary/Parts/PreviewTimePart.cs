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
        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);
            Add(new PreviewTimeVisualisation(beatmap.PreviewTime));
            beatmap.PreviewTime.BindValueChanged(s =>
            {
                Alpha = s.NewValue == -1 ? 0 : 1;
            }, true);
        }

        private partial class PreviewTimeVisualisation : PointVisualisation
        {
            private BindableInt previewTime = new BindableInt();

            public PreviewTimeVisualisation(BindableInt time)
                : base(time.Value)
            {
                previewTime.BindTo(time);
                previewTime.BindValueChanged(s => X = s.NewValue);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Lime;
        }
    }
}
