//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.GameModes.Menu
{
    internal class MainMenu : GameMode
    {
        public override string Name => @"Main Menu";

        private AudioTrackBass bgm;

		public override void Load()
		{
			base.Load();

			AudioSample welcome = Game.Audio.Sample.Get(@"welcome");

			Add(new Drawable[]
			{
				new ButtonSystem(),
				new TextBox()
				{
					Text = @"The quick brown fox jumped over the lazy dog.",
					Position = new Vector2(50, 50),
					Size = new Vector2(300, 20)
				},
				new SpriteText()
				{
					Text = @"The quick brown fox jumped over the lazy dog.",
					Position = new Vector2(50, 80),
					Size = new Vector2(300, 20)
				}
			});
		}
    }
}
