// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics
{
	public class Count : AutoSizeContainer
	{
		public int count = 0;
		public bool isLit = false;
		public string name = String.Empty;
		internal Sprite buttonSprite;
		internal Sprite glowSprite;
		internal SpriteText keySpriteText;
		internal SpriteText countSpriteText;
		internal Color4 textColourNormal = Color4.White;
		internal Color4 textColourGlow = Color4.Black;
		internal KeyCounter keyCounter;

		public override bool HandleInput => true;
		public override bool Contains(Vector2 screenSpacePos) => true;

		internal bool IsCounting => (keyCounter != null && keyCounter.isCounting);

		public override void Load()
		{
			base.Load();

			Children = new Drawable[]
			{
				buttonSprite = new Sprite
				{
					Texture = Game.Textures.Get(@"KeyCounter/key-up"),
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre,
					Children = new Drawable[]
					{
						glowSprite = new Sprite
						{
							Texture = Game.Textures.Get(@"KeyCounter/key-glow"),
							Anchor = Anchor.Centre,
							Origin = Anchor.Centre,
							Alpha = 0.4f,
						}
					}
				},
				keySpriteText = new SpriteText
				{
					Text = name,
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre,
					Position = new Vector2(0, -buttonSprite.Height / 4),
					Colour = textColourNormal,
				},
				countSpriteText = new SpriteText
				{
					Text = count.ToString(),
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre,
					Position = new Vector2(0, buttonSprite.Height / 4),
					Colour = textColourNormal,
				}
			};

			glowSprite.Hide();
		}

		public void CountTriggerPressed()
		{
			if (isLit || !IsCounting)
				return;
			isLit = true;
			countSpriteText.Text = (++count).ToString();
			countSpriteText.Colour = textColourGlow;
			keySpriteText.Colour = textColourGlow;
			glowSprite.Show();
		}

		public void CountTriggerReleased()
		{
			if(!IsCounting)
				return;
			countSpriteText.Colour = textColourNormal;
			keySpriteText.Colour = textColourNormal;
			glowSprite.FadeOut(4);
			isLit = false;
		}
	}
}
