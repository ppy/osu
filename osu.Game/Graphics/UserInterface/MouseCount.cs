// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using OpenTK.Input;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Framework.Graphics
{
	class MouseCount : Count
	{
		internal MouseButton triggerButton;

		public MouseCount(string name, MouseButton mouseButton)
		{
			this.name = name;
			this.triggerButton = mouseButton;
		}

		protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
		{
			base.OnMouseDown(state, args);

			if (args.Button == triggerButton)
			{
				CountTriggerPressed();
			}

			return false;
		}

		protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
		{
			base.OnMouseUp(state, args);

			if (args.Button == triggerButton)
			{
				CountTriggerReleased();
			}

			return false;

		}

	}
}
