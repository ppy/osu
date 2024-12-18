// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Setup
{
    internal partial class ActionableInfo : LabelledDrawable<Drawable>
    {
        public const float BUTTON_SIZE = 120;

        public Action? Action;

        protected FillFlowContainer FlowContainer = null!;

        protected OsuButton Button = null!;

        private TournamentSpriteText valueText = null!;

        public ActionableInfo()
            : base(true)
        {
        }

        public string ButtonText
        {
            set => Button.Text = value;
        }

        public string Value
        {
            set => valueText.Text = value;
        }

        public bool Failing
        {
            set => valueText.Colour = value ? Color4.Red : Color4.White;
        }

        protected override Drawable CreateComponent() => new Container
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X,
            Children = new Drawable[]
            {
                valueText = new TournamentSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                FlowContainer = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        Button = new RoundedButton
                        {
                            Size = new Vector2(BUTTON_SIZE, 40),
                            Action = () => Action?.Invoke()
                        }
                    }
                }
            }
        };
    }
}
