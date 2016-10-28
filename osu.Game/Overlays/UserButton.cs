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
using osu.Game.Online;

namespace osu.Game.Overlays
{
    public class UserButton : Container
    {
        public const float WIDTH = 60;
        public Action Action;
        protected Avatar DrawableAvatar;
        protected Container AvatarContainer;
        protected SpriteText DrawableText;
        protected Box HoverBackground;

        /// <summary>
        /// Create a new user button.
        /// </summary>
        public UserButton(User user)
        {
            DrawableAvatar = new Avatar(user);
            DrawableAvatar.Size = new Vector2(48);
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
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[] {
                        DrawableText = new SpriteText
                        {
                            Text = "Not signed in",
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        AvatarContainer = new Container {
                            Margin = new MarginPadding { Left = 5 },
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Masking = true,
                            CornerRadius = 6f,
                            Size = new Vector2(48),
                            Children = new Drawable[] {
                                DrawableAvatar,
                            },
                        },
                    }
                }
            };

            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(WIDTH, 1);
        }

        public void UpdateButton(LocalUser user)
        {
            AvatarContainer.Clear();
            AvatarContainer.Add(DrawableAvatar = new Avatar(user));
            DrawableAvatar.Size = new Vector2(48);
            if(user.Name != null)
                DrawableText.Text = user.Name;
        }

        protected override void Update()
        {
            base.Update();
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