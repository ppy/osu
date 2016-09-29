//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.GameModes;
using osu.Framework.Graphics;

namespace osu.Game.GameModes.Menu
{
    internal class MainMenu : GameMode
    {
        public override string Name => @"Main Menu";

        //private AudioTrackBass bgm;

		public override void Load()
		{
			base.Load();

			AudioSample welcome = Game.Audio.Sample.Get(@"welcome");

			Children = new Drawable[]
			{
				new ButtonSystem(),
			};
		}
    }
}
