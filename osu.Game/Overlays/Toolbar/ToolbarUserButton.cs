// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Online.API;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarUserButton : ToolbarButton, IOnlineComponent
    {
        private Avatar avatar;

        public ToolbarUserButton()
        {
            AutoSizeAxes = Axes.X;

            DrawableText.Font = @"Exo2.0-MediumItalic";

            Add(new OpaqueBackground { Depth = 1 });

            Flow.Add(avatar = new Avatar());
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            api.Register(this);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                default:
                    Text = @"Guest";
                    avatar.UserId = 1;
                    break;
                case APIState.Online:
                    Text = api.Username;
                    avatar.UserId = api.LocalUser.Value.Id;
                    break;
            }
        }

        public class Avatar : Container
        {
            public Drawable Sprite;

            private int userId;
            private OsuGame game;
            private Texture guestTexture;

            public Avatar()
            {
                Size = new Vector2(32);
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;

                CornerRadius = Size.X / 8;

                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 4,
                    Colour = Color4.Black.Opacity(0.1f),
                };

                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load(OsuGame game, TextureStore textures)
            {
                this.game = game;

                guestTexture = textures.Get(@"Online/avatar-guest");
            }

            public int UserId
            {
                get { return userId; }
                set
                {
                    if (userId == value)
                        return;

                    userId = value;

                    var newSprite = userId > 1 ? new OnlineSprite($@"https://a.ppy.sh/{userId}") : new Sprite { Texture = guestTexture };

                    newSprite.FillMode = FillMode.Fit;

                    newSprite.LoadAsync(game, s =>
                    {
                        Sprite?.FadeOut();
                        Sprite?.Expire();
                        Sprite = s;

                        Add(s);

                        //todo: fix this... clock dependencies are a pain
                        if (s.Clock != null)
                            s.FadeInFromZero(200);
                    });
                }
            }

            public class OnlineSprite : Sprite
            {
                private readonly string url;

                public OnlineSprite(string url)
                {
                    Debug.Assert(url != null);
                    this.url = url;
                }

                [BackgroundDependencyLoader]
                private void load(TextureStore textures)
                {
                    Texture = textures.Get(url);
                }
            }
        }
    }
}
