//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.KeyCounter;

namespace osu.Game.GameModes.Menu
{
    internal class MainMenu : GameMode
    {
        public override string Name => @"Main Menu";

        private AudioTrackBass bgm;
        private KeyCounter kc;

		public override void Load()
		{
			base.Load();

			AudioSample welcome = Game.Audio.Sample.Get(@"welcome");

			Children = new Drawable[]
			{
				new ButtonSystem(),
			};
		}
            Add(kc = new KeyCounter
            {
                Position = new Vector2(250, 280)
            });            

            kc.AddKey(new KeyboardCount(@"Z", OpenTK.Input.Key.Z));
            kc.AddKey(new KeyboardCount(@"X", OpenTK.Input.Key.X));
            kc.AddKey(new MouseCount(@"M1", OpenTK.Input.MouseButton.Left));
            kc.AddKey(new MouseCount(@"M2", OpenTK.Input.MouseButton.Right));
            
            //kc.IsCounting = false;
        }
    }
}
