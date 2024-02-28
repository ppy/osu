// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModPresetTooltip : VisibilityContainer, ITooltip<ModPreset>
    {
        protected override Container<Drawable> Content { get; }

        private const double transition_duration = 200;

        public ModPresetTooltip(OverlayColourProvider colourProvider)
        {
            Width = 250;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 7;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background6
                },
                Content = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(7),
                    Spacing = new Vector2(7)
                }
            };
        }

        private ModPreset? lastPreset;

        public void SetContent(ModPreset preset)
        {
            if (ReferenceEquals(preset, lastPreset))
                return;

            lastPreset = preset;
            Content.ChildrenEnumerable = preset.Mods.AsOrdered().Select(mod => new ModPresetRow(mod));
        }

        protected override void PopIn() => this.FadeIn(transition_duration, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(transition_duration, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;
    }
}
