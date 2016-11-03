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
        private FlowContainer optionsContainer;
        private BasicStorage storage;
        private OsuConfigManager configManager;
        private APIAccess api;

        protected override void Load(BaseGame game)
        {
            base.Load(game);

            storage = game.Host.Storage;

            var osuGame = game as OsuGameBase;
            if (osuGame != null)
            {
                configManager = osuGame.Config;
                api = osuGame.API;
            }

            Depth = float.MaxValue;
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(width, 1);
            Position = new Vector2(-width, 0);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(51, 51, 51, 255)
                },
                // TODO: Links on the side to jump to a section
                new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollDraggerOnLeft = true,
                    Children = new[]
                    {
                        optionsContainer = new FlowContainer
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
                                new GeneralOptions(storage, api),
                                new GraphicsOptions(),
                                new GameplayOptions(),
                                new AudioOptions(),
                                new SkinOptions(),
                                new InputOptions(),
                                new EditorOptions(),
                                new OnlineOptions(),
                                new MaintenanceOptions(),
                            }
                        }
                    }
                }
            };
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    if (State == Visibility.Hidden) return false;

                    State = Visibility.Hidden;
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        protected override void PopIn()
        {
            MoveToX(0, 300, EasingTypes.Out);
        }

        protected override void PopOut()
        {
            MoveToX(-width, 300, EasingTypes.Out);
        }
    }
}
