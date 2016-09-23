// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Input;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Framework.Graphics
{
	class KeyboardCount : Count
	{
		internal Key triggerKey;

		public KeyboardCount(string name, Key key)
		{
			this.name = name;
			this.triggerKey = key;
		}

		protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
		{
			base.OnKeyDown(state, args);

			if (args.Key == triggerKey)
			{
				CountTriggerPressed();
			}

			return false;
		}

		protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
		{
			base.OnKeyUp(state, args);

			if (args.Key == triggerKey)
			{
				CountTriggerReleased();
			}

			return false;
		}

	}
}
