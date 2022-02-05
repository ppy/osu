// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Database;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public abstract class ToolbarButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        protected GlobalAction? Hotkey { get; set; }

        public void SetIcon(Drawable icon)
        {
            IconContainer.Icon = icon;
            IconContainer.Show();
        }

        [Resolved]
        private TextureStore textures { get; set; }

        [Resolved]
        private ReadableKeyCombinationProvider keyCombinationProvider { get; set; }

        public void SetIcon(string texture) =>
            SetIcon(new Sprite
            {
                Texture = textures.Get(texture),
            });

        public LocalisableString Text
        {
            get => DrawableText.Text;
            set => DrawableText.Text = value;
        }

        public LocalisableString TooltipMain
        {
            get => tooltip1.Text;
            set => tooltip1.Text = value;
        }

        public LocalisableString TooltipSub
        {
            get => tooltip2.Text;
            set => tooltip2.Text = value;
        }

        protected virtual Anchor TooltipAnchor => Anchor.TopLeft;

        protected ConstrainedIconContainer IconContainer;
        protected SpriteText DrawableText;
        protected Box HoverBackground;
        private readonly Box flashBackground;
        private readonly FillFlowContainer tooltipContainer;
        private readonly SpriteText tooltip1;
        private readonly SpriteText tooltip2;
        private readonly SpriteText keyBindingTooltip;
        protected FillFlowContainer Flow;

        [Resolved]
        private RealmAccess realm { get; set; }

        protected ToolbarButton()
            : base(HoverSampleSet.Toolbar)
        {
            Width = Toolbar.HEIGHT;
            RelativeSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                HoverBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(80).Opacity(180),
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
                flashBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Colour = Color4.White.Opacity(100),
                    Blending = BlendingParameters.Additive,
                },
                Flow = new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding { Left = Toolbar.HEIGHT / 2, Right = Toolbar.HEIGHT / 2 },
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Children = new Drawable[]
                    {
                        IconContainer = new ConstrainedIconContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(26),
                            Alpha = 0,
                        },
                        DrawableText = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                    },
                },
                tooltipContainer = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.Both, // stops us being considered in parent's autosize
                    Anchor = TooltipAnchor.HasFlagFast(Anchor.x0) ? Anchor.BottomLeft : Anchor.BottomRight,
                    Origin = TooltipAnchor,
                    Position = new Vector2(TooltipAnchor.HasFlagFast(Anchor.x0) ? 5 : -5, 5),
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        tooltip1 = new OsuSpriteText
                        {
                            Anchor = TooltipAnchor,
                            Origin = TooltipAnchor,
                            Shadow = true,
                            Font = OsuFont.GetFont(size: 22, weight: FontWeight.Bold),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = TooltipAnchor,
                            Origin = TooltipAnchor,
                            Direction = FillDirection.Horizontal,
                            Children = new[]
                            {
                                tooltip2 = new OsuSpriteText { Shadow = true },
                                keyBindingTooltip = new OsuSpriteText { Shadow = true }
                            }
                        }
                    }
                }
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override bool OnClick(ClickEvent e)
        {
            flashBackground.FadeOutFromOne(800, Easing.OutQuint);
            tooltipContainer.FadeOut(100);
            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateKeyBindingTooltip();

            HoverBackground.FadeIn(200);
            tooltipContainer.FadeIn(100);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            HoverBackground.FadeOut(200);
            tooltipContainer.FadeOut(100);
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == Hotkey)
            {
                TriggerClick();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void updateKeyBindingTooltip()
        {
            if (Hotkey == null) return;

            var realmKeyBinding = realm.Realm.All<RealmKeyBinding>().FirstOrDefault(rkb => rkb.RulesetName == null && rkb.ActionInt == (int)Hotkey.Value);

            if (realmKeyBinding != null)
            {
                string keyBindingString = keyCombinationProvider.GetReadableString(realmKeyBinding.KeyCombination);

                if (!string.IsNullOrEmpty(keyBindingString))
                    keyBindingTooltip.Text = $" ({keyBindingString})";
            }
        }
    }

    public class OpaqueBackground : Container
    {
        public OpaqueBackground()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            MaskingSmoothness = 0;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(40),
                Radius = 5,
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(30)
                },
                new Triangles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = OsuColour.Gray(40),
                    ColourDark = OsuColour.Gray(20),
                },
            };
        }
    }
}
