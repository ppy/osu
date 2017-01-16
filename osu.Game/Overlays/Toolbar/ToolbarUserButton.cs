//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Game.Online.API;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    class ToolbarUserButton : ToolbarButton, IOnlineComponent
    {
        private Avatar avatar;

        public ToolbarUserButton()
        {
            DrawableText.Font = @"Exo2.0-MediumItalic";

            Add(new OpaqueBackground { Depth = 1 });

            Flow.Add(avatar = new Avatar());
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api, OsuConfigManager config)
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

                guestTexture = textures.Get(@"Online/avatar-guest@2x");
            }

            public int UserId
            {
                get { return userId; }
                set
                {
                    if (userId == value)
                        return;

                    userId = value;

                    Sprite newSprite;
                    if (userId > 1)
                        newSprite = new OnlineSprite($@"https://a.ppy.sh/{userId}");
                    else
                        newSprite = new Sprite { Texture = guestTexture };

                    newSprite.FillMode = FillMode.Fit;

                    newSprite.Preload(game, s =>
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
                private readonly int userId;

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
