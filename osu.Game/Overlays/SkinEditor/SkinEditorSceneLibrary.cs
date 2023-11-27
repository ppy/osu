// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Screens;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinEditorSceneLibrary : CompositeDrawable
    {
        public const float HEIGHT = BUTTON_HEIGHT + padding * 2;

        public const float BUTTON_HEIGHT = 40;

        private const float padding = 10;

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private SkinEditorOverlay skinEditorOverlay { get; set; } = null!;

        public SkinEditorSceneLibrary()
        {
            Height = HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColourProvider.Background6,
                },
                new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Name = @"Scene library",
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Spacing = new Vector2(padding),
                            Padding = new MarginPadding(padding),
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = SkinEditorStrings.SceneLibrary,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding(10),
                                },
                                new SceneButton
                                {
                                    Text = SkinEditorStrings.SongSelect,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Action = () => performer?.PerformFromScreen(screen =>
                                    {
                                        if (screen is SongSelect)
                                            return;

                                        screen.Push(new PlaySongSelect());
                                    }, new[] { typeof(SongSelect) })
                                },
                                new SceneButton
                                {
                                    Text = SkinEditorStrings.Gameplay,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Action = () => skinEditorOverlay.PresentGameplay(),
                                },
                            }
                        },
                    }
                }
            };
        }

        public partial class SceneButton : OsuButton
        {
            public SceneButton()
            {
                Width = 100;
                Height = BUTTON_HEIGHT;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
            {
                BackgroundColour = overlayColourProvider?.Background3 ?? colours.Blue3;
                Content.CornerRadius = 5;
            }
        }
    }
}
