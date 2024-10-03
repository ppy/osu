// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class GenerateToolboxGroup : EditorToolboxGroup
    {
        private readonly EditorToolButton polygonButton;

        public GenerateToolboxGroup()
            : base("Generate")
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    polygonButton = new EditorToolButton("Polygon",
                        () => new SpriteIcon { Icon = FontAwesome.Solid.Spinner },
                        () => new PolygonGenerationPopover()),
                }
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            switch (e.Key)
            {
                case Key.D:
                    if (!e.ControlPressed || !e.ShiftPressed)
                        return false;

                    polygonButton.TriggerClick();
                    return true;

                default:
                    return false;
            }
        }
    }
}
