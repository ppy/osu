// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class GameplaySpectatorUser : CompositeDrawable
    {
        public const float EXTENDED_WIDTH = regular_width + top_text_left_width_extension;
        private const float regular_width = 100f;
        private const float top_text_left_width_extension = 20f;
        public const float PANEL_HEIGHT = 35f;
        public const float SHEAR_WIDTH = PANEL_HEIGHT * panel_shear;
        private const float panel_shear = 0.15f;
        private const float avatar_size = 25f;
        private const float panel_transition_duration = 200f;

        private OsuSpriteText? usernameText;

        public Color4? BackgroundColour { get; set; }

        public Color4? TextColour { get; set; }

        [CanBeNull]
        public IUser User { get; }
        private Container? mainFillContainer;

        private Box? centralFill;

        private Container? backgroundPaddingAdjustContainer;

        private GridContainer? gridContainer;

        public GameplaySpectatorUser([CanBeNull] IUser user)
        {
            User = user;

            AutoSizeAxes = Axes.X;
            Height = PANEL_HEIGHT;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Container avatarContainer;
            InternalChildren = new Drawable[]
            {
                new Container
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        backgroundPaddingAdjustContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                mainFillContainer = new Container
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 5f,
                                    Shear = new Vector2(panel_shear, 0f),
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            Alpha = 0.5f,
                                            RelativePositionAxes = Axes.Both,
                                        }
                                    },
                                },
                            },
                        },
                        gridContainer = new GridContainer
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = EXTENDED_WIDTH,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            ColumnDimensions = new []
                            {
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        Padding = new MarginPadding { Horizontal = SHEAR_WIDTH / 3 },
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                Masking = true,
                                                CornerRadius = 5f,
                                                Shear = new Vector2(panel_shear, 0f),
                                                RelativeSizeAxes = Axes.Both,
                                                Children = new[]
                                                {
                                                    centralFill = new Box
                                                    {
                                                        Alpha = 0.5f,
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = Color4Extensions.FromHex("3399cc"),
                                                    },
                                                }
                                            },
                                            new FillFlowContainer
                                            {
                                                Padding = new MarginPadding { Left = SHEAR_WIDTH },
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                RelativeSizeAxes = Axes.Both,
                                                Direction = FillDirection.Horizontal,
                                                LayoutDuration = panel_transition_duration,
                                                LayoutEasing = Easing.OutQuint,
                                                Spacing = new Vector2(4f, 0f),
                                                Children = new Drawable[]
                                                {
                                                    avatarContainer = new CircularContainer
                                                    {
                                                        Masking = true,
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Size = new Vector2(avatar_size),
                                                        Children = new Drawable[]
                                                        {
                                                            new Box
                                                            {
                                                                Name = "Placeholder while avatar loads",
                                                                Alpha = 0.3f,
                                                                RelativeSizeAxes = Axes.Both,
                                                                Colour = colours.Gray4,
                                                            }
                                                        }
                                                    },
                                                    usernameText = new OsuSpriteText
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Width = 0.6f,
                                                        Anchor = Anchor.CentreLeft,
                                                        Origin = Anchor.CentreLeft,
                                                        Colour = Color4.White,
                                                        Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold),
                                                        Text = User.Username,
                                                        Truncate = true,
                                                        Shadow = false,
                                                    }
                                                }
                                            },
                                        }
                                    },
                                },
                            },
                        },
                    },
                },
            };

            LoadComponentAsync(new DrawableAvatar(User), avatarContainer.Add);
        }
    }
}
