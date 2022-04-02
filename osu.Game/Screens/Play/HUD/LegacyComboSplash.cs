// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class LegacyComboSplash : CompositeDrawable, ISkinnableDrawable
    {
        public Bindable<int> Current { get; } = new BindableInt { MinValue = 0 };

        public bool UsesFixedAnchor { get; set; }

        private readonly Random random = new Random();

        public LegacyComboSplash()
        {
            AutoSizeAxes = Axes.Both;
            Origin = Anchor.CentreLeft;
        }

        private void OnNewCombo(int combo)
        {
            if (shouldDisplay(combo) && InternalChildren.Count != 0)
            {
                Drawable sprite = InternalChildren[random.Next(0, InternalChildren.Count)];
                sprite.FadeTo(1).MoveToX(-sprite.Width).Then().MoveToX(0, 1000, Easing.OutCirc).FadeOut(1000, Easing.OutQuad);
            }
        }

        private bool shouldDisplay(int currentCombo)
        {
            if (currentCombo >= 100)
            {
                return currentCombo % 50 == 0;
            }

            return currentCombo == 30 || currentCombo == 60;
        }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor, ISkinSource skin)
        {
            Current.BindTo(scoreProcessor.Combo);

            if (skin.GetTexture("comboburst-0") != null)
            {
                // loading comboburst-{n}.png files
                for (int i = 0;; i++)
                {
                    var tex = skin.GetTexture($"comboburst-{i}");
                    if (tex == null)
                        break;

                    AddInternal(createSprite(tex));
                }

                return;
            }

            var defaultTex = skin.GetTexture("comboburst");
            if (defaultTex != null)
                InternalChild = createSprite(defaultTex);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(e => OnNewCombo(e.NewValue));
        }

        private Sprite createSprite(Texture tex) => new Sprite
        {
            Texture = tex,
            Alpha = 0,
            AlwaysPresent = true, // needed to make the component having size in editor
        };
    }
}
