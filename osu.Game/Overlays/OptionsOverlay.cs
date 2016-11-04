//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Overlays.Options;

namespace osu.Game.Overlays
{
    public class OptionsOverlay : OverlayContainer
    {
        internal const float SideMargins = 10;
        private const float width = 400;
        private const float sideNavWidth = 60;
        private const float sideNavPadding = 0;

        private ScrollContainer scrollContainer;
        private FlowContainer flowContainer;
        
        private GeneralOptions generalOptions;
        private GraphicsOptions graphicsOptions;
        private GameplayOptions gameplayOptions;
        private AudioOptions audioOptions;
        private SkinOptions skinOptions;
        private InputOptions inputOptions;
        private EditorOptions editorOptions;
        private OnlineOptions onlineOptions;
        private MaintenanceOptions maintenanceOptions;

        public OptionsOverlay()
        {
            Depth = float.MaxValue;
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(width, 1);
            Position = new Vector2(-width, 0);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                },
                scrollContainer = new ScrollContainer
                {
                    ScrollDraggerAnchor = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = width - (sideNavWidth + sideNavPadding * 2),
                    Position = new Vector2(sideNavWidth + sideNavPadding * 2, 0),
                    // Note: removing this comment causes... compiler bugs? It's weird.2
                    Children = new[]
                    {
                        flowContainer = new FlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                            Direction = FlowDirection.VerticalOnly,
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Text = "settings",
                                    TextSize = 40,
                                    Margin = new MarginPadding { Left = SideMargins, Top = 30 },
                                },
                                new SpriteText
                                {
                                    Colour = new Color4(235, 117, 139, 255),
                                    Text = "Change the way osu! behaves",
                                    TextSize = 18,
                                    Margin = new MarginPadding { Left = SideMargins, Bottom = 30 },
                                },
                                generalOptions = new GeneralOptions(),
                                graphicsOptions = new GraphicsOptions(),
                                gameplayOptions = new GameplayOptions(),
                                audioOptions = new AudioOptions(),
                                skinOptions = new SkinOptions(),
                                inputOptions = new InputOptions(),
                                editorOptions = new EditorOptions(),
                                onlineOptions = new OnlineOptions(),
                                maintenanceOptions = new MaintenanceOptions(),
                            }
                        }
                    }
                },
                new OptionsSideNav
                {
                    Padding = new MarginPadding { Left = sideNavPadding, Right = sideNavPadding },
                    Width = sideNavWidth + sideNavPadding * 2,
                    GeneralAction = () => scrollContainer.ScrollIntoView(generalOptions),
                    GraphicsAction = () => scrollContainer.ScrollIntoView(graphicsOptions),
                    GameplayAction = () => scrollContainer.ScrollIntoView(gameplayOptions),
                    AudioAction = () => scrollContainer.ScrollIntoView(audioOptions),
                    SkinAction = () => scrollContainer.ScrollIntoView(skinOptions),
                    InputAction = () => scrollContainer.ScrollIntoView(inputOptions),
                    EditorAction = () => scrollContainer.ScrollIntoView(editorOptions),
                    OnlineAction = () => scrollContainer.ScrollIntoView(onlineOptions),
                    MaintenanceAction = () => scrollContainer.ScrollIntoView(maintenanceOptions),
                }
            };
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    if (State == Visibility.Hidden) return false;
                    Hide();
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        protected override void PopIn()
        {
            MoveToX(0, 300, EasingTypes.Out);
            FadeTo(1, 300);
        }

        protected override void PopOut()
        {
            MoveToX(-width, 300, EasingTypes.Out);
            FadeTo(0, 300);
        }
    }
}
