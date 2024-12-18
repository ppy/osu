// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class NonSkinnableScreenPlaceholder : CompositeDrawable
    {
        [Resolved]
        private SkinEditorOverlay? skinEditorOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Dark6,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.95f,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0, 5),
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Icon = FontAwesome.Solid.ExclamationCircle,
                            Size = new Vector2(24),
                            Y = -5,
                        },
                        new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold, size: 18))
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = "Please navigate to a skinnable screen using the scene library",
                        },
                        new RoundedButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = 200,
                            Margin = new MarginPadding { Top = 20 },
                            Action = () => skinEditorOverlay?.Hide(),
                            Text = "Return to game"
                        }
                    }
                },
            };
        }
    }
}
