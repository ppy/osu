//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using osu.Framework;
using osu.Game.Online;

namespace osu.Game.Graphics
{
    public class Avatar : Sprite
    {
        private int? userId;
        private BaseGame game;

        public Avatar(User user)
        {
            try
            {
                userId = user.UserId;
            }
            catch { }
        }

        public override async void Load(BaseGame game)
        {
            base.Load(game);
            this.game = game;

            Texture tex = null;

            if (userId != 0)
            {
                try
                {
                    tex = await game.Textures.GetAsync($@"https://a.ppy.sh/{userId}");
                }
                catch { }
            }
            else
            {
                tex = game.Textures.Get(@"Menu/avatar-guest");
            }

            Scheduler.Add(delegate
            {
                if (tex == null)
                {
                    Expire();
                    return;
                }
                Texture = tex;
            }, true);
        }
    }
}