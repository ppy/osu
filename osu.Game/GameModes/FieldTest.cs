//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Threading;

namespace osu.Game.GameModes
{
    internal class FieldTest : GameMode
    {
        private AutoSizeContainer container;

		public override void Load()
		{
			base.Load();
			OsuGame game = Game as OsuGame;

			ClickableBox button;
			Add(new Drawable[]
			{
				new Box()
				{
					SizeMode = InheritMode.XY,
					Size = new Vector2(1, 1),
					Colour = Color4.DarkRed
				},
				button = new ClickableBox(Color4.Pink)
				{
					Size = new Vector2(50, 50)
				},
				container = new AutoSizeContainer()
				{
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre,
					Children = new Drawable[]
					{
						new ClickableBox(Color4.SkyBlue)
						{
							SizeMode = InheritMode.XY,
							Size = Vector2.One
						},
						new ClickableBox(Color4.Orange)
						{
							Size = new Vector2(50, 50),
							Anchor = Anchor.TopLeft,
							Origin = Anchor.TopLeft
						},
						new ClickableBox(Color4.Orange)
						{
							Size = new Vector2(50, 50),
							Anchor = Anchor.TopRight,
							Origin = Anchor.TopRight
						},
						new ClickableBox(Color4.Orange)
						{
							Size = new Vector2(50, 50),
							Anchor = Anchor.BottomLeft,
							Origin = Anchor.BottomLeft
						},
						new ClickableBox(Color4.Blue)
						{
							Size = new Vector2(10, 10),
							Anchor = Anchor.Centre,
							Origin = Anchor.Centre
						},
						new ClickableBox(Color4.Orange)
						{
							Size = new Vector2(50, 50),
							Anchor = Anchor.BottomRight,
							Origin = Anchor.BottomRight
						},
						new SpriteCircular(game.Textures.Get("coin"))
						{
							Position = new Vector2(100),
							Anchor = Anchor.Centre,
							Origin = Anchor.Centre
						},
						new SpriteText(game.Textures)
						{
							Anchor = Anchor.CentreLeft,
							Origin = Anchor.CentreLeft,
							Text = "12345"
						}
					}
				}
			});

			button.Activated += () => ExitMode();

		}

        class ClickableBox : Box
        {
            public ClickableBox(Color4? colour = default(Color4?))
            {
                Colour = colour ?? Color4.White;
            }

            internal event Action Activated;

            protected override bool OnClick(InputState state)
            {
                Scale = new Vector2(1.5f);
                ScaleTo(1, 500, EasingTypes.In);

                Activated?.Invoke();

                return true;
            }
        }
    }
}
