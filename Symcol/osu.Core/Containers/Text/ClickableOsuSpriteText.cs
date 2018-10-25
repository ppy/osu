using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Core.Containers.Text
{
    public class ClickableOsuSpriteText : FillFlowContainer, IHasTooltip
    {
        #region Passthrough

        public string Text
        {
            get => osuSpriteText.Text;
            set => osuSpriteText.Text = value;
        }

        public float TextSize
        {
            get => osuSpriteText.TextSize;
            set => osuSpriteText.TextSize = value;
        }

        public string Font
        {
            get => osuSpriteText.Font;
            set => osuSpriteText.Font = value;
        }

        public new Color4 Colour
        {
            get => osuSpriteText.Colour;
            set => osuSpriteText.Colour = value;
        }

        #endregion

        public string TooltipText => Tooltip;

        public string Tooltip = "";

        private readonly OsuSpriteText osuSpriteText = new OsuSpriteText();

        public Color4 IdleColour
        {
            get => HoverContainer.IdleColour;
            set => HoverContainer.IdleColour = value;
        }

        public Action Action
        {
            get { return HoverContainer.Action; }
            set { HoverContainer.Action = value; }
        }

        public readonly PaintableHoverContainer HoverContainer;

        public override bool HandleNonPositionalInput => HoverContainer.Action != null;
        public override bool HandlePositionalInput => HoverContainer.Action != null;

        protected override Container<Drawable> Content => HoverContainer ?? (Container<Drawable>)this;

        public override IEnumerable<Drawable> FlowingChildren => Children;

        public ClickableOsuSpriteText()
        {
            OsuColour osu = new OsuColour();
            Colour = Color4.White;

            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                HoverContainer = new PaintableHoverContainer
                {
                    AutoSizeAxes = Axes.Both,
                    HoverColour = osu.Blue
                }
            });
            HoverContainer.Add(osuSpriteText);
        }
    }
}
