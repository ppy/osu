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
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        public void UpdateAvatar(int userid)
        {
            string url = "https://a.ppy.sh/" + userid.ToString();

            byte[] imageData = null;

            using (var wc = new System.Net.WebClient())
                imageData = wc.DownloadData(url);

            Scheduler.Add(delegate
            {
                if (imageData == null)
                {
                    return;
                }
                Texture = TextureLoader.FromBytes(imageData);
                Size = new Vector2(avatarSize);
                Alpha = 1f;
            });
        }

        public override void Load(BaseGame game)
        {
            this.game = game;
            base.Load(game);

            UpdateAvatar(userId);
        }
    }
}
