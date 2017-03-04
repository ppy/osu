// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Select.Leaderboards
{
    public class LeaderboardScoreDisplay : Container
    {
        private const float height = 70;
        private const float corner_radius = 5;
        private const float edge_margin = 10;
        private const float background_opacity = 0.25f;
        private const float score_letter_size = 20f;

        private Box background;

        public readonly LeaderboardScore Score;

        protected override bool OnHover(Framework.Input.InputState state)
        {
            background.FadeColour(Color4.Black.Opacity(0.5f), 300, EasingTypes.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(Framework.Input.InputState state)
        {
            background.FadeColour(Color4.Black.Opacity(background_opacity), 200, EasingTypes.OutQuint);
            base.OnHoverLost(state);
        }

        public LeaderboardScoreDisplay(LeaderboardScore score, int index)
        {
            Score = score;

            RelativeSizeAxes = Axes.X;
            Height = height;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 40,
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-MediumItalic",
                            TextSize = 22,
                            Text = index.ToString(),
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 40, },
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = corner_radius,
                            Masking = true,
                            Children = new[]
                            {
                                background = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black.Opacity(background_opacity),
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(edge_margin),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Size = new Vector2(height - edge_margin * 2, height - edge_margin * 2),
                                    CornerRadius = corner_radius,
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Sprite
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fill,
                                            Texture = Score.Avatar,
                                        },
                                    },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Position = new Vector2(height - edge_margin, 0f),
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = Score.Name,
                                            Font = @"Exo2.0-BoldItalic",
                                            TextSize = 23,
                                        },
                                        new FillFlowContainer
                                        {
                                            Origin = Anchor.BottomLeft,
                                            Anchor = Anchor.BottomLeft,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Right,
                                            Spacing = new Vector2(10f, 0f),
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    Origin = Anchor.BottomLeft,
                                                    Anchor = Anchor.BottomLeft,
                                                    Size = new Vector2(87f, 20f),
                                                    Masking = true,
                                                    Children = new Drawable[]
                                                    {
                                                        new Sprite
                                                        {
                                                            RelativeSizeAxes = Axes.Y,
                                                            Width = 30f,
                                                            Texture = Score.Flag,
                                                        },
                                                        new Sprite
                                                        {
                                                            Origin = Anchor.BottomRight,
                                                            Anchor = Anchor.BottomRight,
                                                            RelativeSizeAxes = Axes.Y,
                                                            Width = 50f,
                                                            Texture = Score.Badge,
                                                        },
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Right,
                                                    Spacing = new Vector2(10f, 0f),
                                                    Margin = new MarginPadding { Left = 10, },
                                                    Children = new Drawable[]
                                                    {
                                                        new ScoreComponentLabel(FontAwesome.fa_circle_o, Score.MaxCombo.ToString()),
                                                        new ScoreComponentLabel(FontAwesome.fa_circle_o, Score.Accuracy.ToString()),
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                                new Sprite
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Size = new Vector2(score_letter_size),
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Position = new Vector2(-score_letter_size - 5f, 0f),
                                    Font = @"Venera",
                                    Text = Score.Score.ToString(),
                                    TextSize = 23,
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                    AutoSizeAxes = Axes.Both,
                                    // TODO: Probably remove? Seems like others don't like this kind of thing
                                    Position = new Vector2(0f, 4f), //properly align the mod icons
                                    Direction = FillDirection.Left,
                                    Children = new[]
                                    {
                                        new ScoreModIcon(FontAwesome.fa_osu_mod_doubletime, OsuColour.FromHex(@"ffcc22")),
                                        new ScoreModIcon(FontAwesome.fa_osu_mod_flashlight, OsuColour.FromHex(@"ffcc22")),
                                        new ScoreModIcon(FontAwesome.fa_osu_mod_hidden, OsuColour.FromHex(@"ffcc22")),
                                        new ScoreModIcon(FontAwesome.fa_osu_mod_hardrock, OsuColour.FromHex(@"ffcc22")),
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        class ScoreModIcon : Container
        {
            public ScoreModIcon(FontAwesome icon, Color4 colour)
            {
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = FontAwesome.fa_osu_mod_bg,
                        Colour = colour,
                        Shadow = true,
                        TextSize = 30,
                    },
                    new TextAwesome
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Icon = icon,
                        Colour = OsuColour.Gray(84),
                        TextSize = 18,
                    },
                };
            }
        }

        class ScoreComponentLabel : Container
        {
            public ScoreComponentLabel(FontAwesome icon, string value)
            {
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                Size = new Vector2(60f, 20f);
                Padding = new MarginPadding { Top = 10f, };

                Children = new Drawable[]
                {
                    new TextAwesome
                    {
                        Icon = FontAwesome.fa_square,
                        Colour = OsuColour.FromHex(@"3087ac"),
                        Rotation = 45,
                        Shadow = true,
                    },
                    new TextAwesome
                    {
                        Icon = icon,
                        Colour = OsuColour.FromHex(@"a4edff"),
                        Scale = new Vector2(0.8f),
                    },
                    new OsuSpriteText
                    {
                        Origin = Anchor.CentreLeft,
                        Margin = new MarginPadding { Left = 15, },
                        Font = @"Exo2.0-Bold",
                        Text = value,
                        TextSize = 17,
                    },
                };
            }
        }
    }
}
