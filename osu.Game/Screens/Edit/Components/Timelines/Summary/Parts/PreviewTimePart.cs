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
        }

        private partial class PreviewTimeVisualisation : PointVisualisation
        {
            public PreviewTimeVisualisation(BindableInt time)
                : base(time.Value)
            {
                time.BindValueChanged(s => X = s.NewValue);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours) => Colour = colours.Lime;
        }
    }
}
