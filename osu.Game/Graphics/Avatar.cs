//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using osu.Framework;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.OpenGL;
using System.Net;
using System.Collections.Generic;

namespace osu.Game.Graphics
{
    public class Avatar : Sprite
    {
        private int userId;
        private int avatarSize = 50;
        private BaseGame game;

        public Avatar(int userid, int avatarsize)
        {
            this.userId = userid;
            this.avatarSize = avatarsize;
        }

        /// <summary>
        /// Update a given avatar sprite with a new user ID. If the user ID is 1 it will use the guest avatar.
        /// </summary>
        /// <param name="userid">The user ID of the desired avatar</param>
        public async void UpdateAvatar(int userid)
        {
            string url = "https://a.ppy.sh/" + userid.ToString();

            byte[] imageData = null;

            if (userid != 1)
            {
                try
                {
                    using (var wc = new System.Net.WebClient())
                        imageData = await wc.DownloadDataTaskAsync(new System.Uri(url));
                }
                catch { }
            }
            Scheduler.Add(delegate
            {
                if (imageData != null)
                {
                    Texture = TextureLoader.FromBytes(imageData);
                }
            });
        }

        public override void Load(BaseGame game)
        {
            this.game = game;
            base.Load(game);
            Texture = game.Textures.Get(@"Menu/avatar-guest");
            Size = new Vector2(avatarSize);
            UpdateAvatar(userId);
        }
    }
}
