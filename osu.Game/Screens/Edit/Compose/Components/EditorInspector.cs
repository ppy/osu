// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;

namespace osu.Game.Screens.Edit.Compose.Components
{
    internal partial class EditorInspector : CompositeDrawable
    {
        protected OsuTextFlowContainer InspectorText = null!;

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            InternalChild = InspectorText = new OsuTextFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        protected void AddHeader(string header) => InspectorText.AddParagraph($"{header}: ", s =>
        {
            s.Padding = new MarginPadding { Top = 2 };
            s.Font = s.Font.With(size: 12);
            s.Colour = colourProvider.Content2;
        });

        protected void AddValue(string value) => InspectorText.AddParagraph(value, s =>
        {
            s.Font = s.Font.With(weight: FontWeight.SemiBold);
            s.Colour = colourProvider.Content1;
        });
    }
}
