//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Overlays
{
    class UserButton : Container
    {
        public Sprite Image
        {
            get { return DrawableAvatar; }
            set
            {
                DrawableAvatar.Texture = value.Texture;
            }
        }

        public string Text
        {
            get { return DrawableText.Text; }
            set
            {
                DrawableText.Text = value;
            }
        }

        public int UserId
        {
            get { return userId; }
            set
            {
                userId = value;
            }
        }


        public const float WIDTH = 60;
        public Action Action;
        protected Avatar DrawableAvatar;
        protected SpriteText DrawableText;
        protected Box HoverBackground;
        protected int userId;


        public UserButton(int userId)
        {
            DrawableAvatar = new Avatar(userId, 48);
            Children = new Drawable[]
            {
                HoverBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Additive = true,
                    Colour = new Color4(60, 60, 60, 255),
                    Alpha = 0,
                },
                new FlowContainer
                {
                    Direction = FlowDirection.HorizontalOnly,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding { Left = 5, Right = 5 },
                    RelativeSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        DrawableText = new SpriteText
                        {
                            Margin = new MarginPadding { Left = 5 },
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new FlowContainer {
                            Masking = true,
                            CornerRadius = 6f,
                            Children = new Drawable[] {
                                DrawableAvatar,
                            },
                        },
                    },
                }
            };

            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(WIDTH, 1);
        }

        public void UpdateButton(int userid)
        {
            DrawableAvatar.UpdateAvatar(userid);
        }

        protected override void Update()
        {
            base.Update();

            //todo: find a way to avoid using this (autosize needs to be able to ignore certain drawables.. in this case the tooltip)
            Size = new Vector2(WIDTH + (DrawableText.IsVisible ? DrawableText.Size.X : 0), 1);
        }

        protected override bool OnClick(InputState state)
        {
            Action?.Invoke();
            HoverBackground.FlashColour(Color4.White, 400);
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            HoverBackground.FadeTo(0.4f, 200);
            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            HoverBackground.FadeTo(0, 200);
        }
    }
}
