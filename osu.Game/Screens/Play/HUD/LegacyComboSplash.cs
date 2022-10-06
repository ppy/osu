// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class LegacyComboSplash : Container, ISkinnableDrawable
    {
        [SettingSource("Side, where bursts will appear")]
        public Bindable<Side> BurstsSide { get; } = new Bindable<Side>(Side.Random);

        private IBindable<bool> randomOrder = null!;

        private readonly Bindable<int> current = new BindableInt { MinValue = 0 };

        public bool UsesFixedAnchor { get; set; }

        public Func<int, bool> BurstCondition { get; set; } = combo =>
        {
            // by default, std/taiko/catch milestones are used.
            if (combo >= 100)
            {
                return combo % 50 == 0;
            }

            return combo == 30 || combo == 60;
        };

        private readonly Random random = new Random();

        private Container<Sprite> left = null!;
        private Container<Sprite> right = null!;

        /// <summary>
        /// Stores next burst sprite's index, when non-random order is used.
        /// </summary>
        private int spriteIndex;

        public LegacyComboSplash()
        {
            AutoSizeAxes = Axes.Both;
            Origin = Anchor.CentreLeft;
        }

        private void OnNewCombo(int combo)
        {
            if (!BurstCondition(combo)) return;

            Container<Sprite> toShow = BurstsSide.Value switch
            {
                Side.Left => left,
                Side.Right => right,
                _ => random.Next(0, 2) == 0 ? left : right
            };

            if (toShow.Count == 0) return;

            Sprite sprite;

            if (randomOrder.Value)
                sprite = toShow[random.Next(0, toShow.Count)];
            else
            {
                if (spriteIndex >= toShow.Count) spriteIndex = 0;
                sprite = toShow[spriteIndex];
                spriteIndex++;
            }

            sprite.MoveToX(-sprite.Width * 0.625f).Then().MoveToX(0, 700, Easing.Out);
            sprite.FadeTo(1).Delay(200).FadeOut(1000, Easing.In);
        }

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor, ISkinSource skin)
        {
            current.BindTo(scoreProcessor.Combo);
            randomOrder = skin.GetConfig<SkinConfiguration.LegacySetting, bool>(SkinConfiguration.LegacySetting.ComboBurstRandom) ?? new Bindable<bool>();

            var c = new LegacyComboSplashComponent();

            if (skin.GetDrawableComponent(c) is LegacyComboSplashSide side1)
            {
                side1.Origin = Anchor.CentreLeft;
                side1.Anchor = Anchor.CentreLeft;
                left = side1;
                Add(left);
            }
            else
                Add(left = new Container<Sprite>());

            if (skin.GetDrawableComponent(c) is LegacyComboSplashSide side2)
            {
                side2.Origin = Anchor.CentreLeft;
                side2.Anchor = Anchor.CentreRight;
                side2.Scale = new Vector2(-1, 1);
                right = side2;
                Add(right);
            }
            else
                Add(right = new Container<Sprite>());
        }

        private void OnSideChanged(Side side)
        {
            switch (side)
            {
                case Side.Random:
                    // when both sides are used, the component should cover the entire screen.
                    AutoSizeAxes = Axes.None;
                    RelativeSizeAxes = Axes.Both;
                    Position = new Vector2(0);
                    Size = new Vector2(1, 1);
                    break;

                default:
                    RelativeSizeAxes = Axes.None;
                    AutoSizeAxes = Axes.Both;
                    break;
            }

            switch (side)
            {
                case Side.Random:
                    left.FadeIn(500);
                    right.FadeIn(500);
                    break;

                case Side.Left:
                    left.FadeIn(500);
                    right.FadeOut(500);
                    break;

                case Side.Right:
                    left.FadeOut(500);
                    right.FadeIn(500);
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            current.BindValueChanged(e => OnNewCombo(e.NewValue));
            BurstsSide.BindValueChanged(e => OnSideChanged(e.NewValue), true);

            if (Parent is SkinnableTargetComponentsContainer container)
            {
                container.ChangeChildDepth(this, 1000);
            }
        }

        public enum Side
        {
            Left,
            Right,
            Random
        }

        public class LegacyComboSplashComponent : ISkinComponent
        {
            public string LookupName => "comboburst";
        }

        public class LegacyComboSplashSide : Container<Sprite>
        {
            public LegacyComboSplashSide(string spriteName)
            {
                this.spriteName = spriteName;
                AutoSizeAxes = Axes.Both;
            }

            private readonly string spriteName;

            [BackgroundDependencyLoader]
            private void load(ISkinSource skin)
            {
                if (skin.GetTexture($"{spriteName}-0") != null)
                {
                    // loading comboburst-{n}.png files
                    for (int i = 0;; i++)
                    {
                        var tex = skin.GetTexture($"{spriteName}-{i}");
                        if (tex == null)
                            break;

                        Add(createSprite(tex));
                    }

                    return;
                }

                var defaultTex = skin.GetTexture(spriteName);

                if (defaultTex != null)
                {
                    Add(createSprite(defaultTex));
                }
            }

            private Sprite createSprite(Texture tex) => new Sprite
            {
                Texture = tex,
                Alpha = 0,
                AlwaysPresent = true, // needed to make the component having size in editor
            };
        }
    }
}
