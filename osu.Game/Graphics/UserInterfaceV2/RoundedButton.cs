// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class RoundedButton : OsuButton, IFilterable
    {
        public override float Height
        {
            get => base.Height;
            set
            {
                base.Height = value;

                if (IsLoaded)
                    updateCornerRadius();
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Blue3;

            Content.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(0, 2),
                Radius = 4,
                Colour = Colour4.Black.Opacity(0.15f)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateCornerRadius();
        }

        private void updateCornerRadius() => Content.CornerRadius = DrawHeight / 2;

        public virtual IEnumerable<string> FilterTerms => new[] { Text.ToString() };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }
    }
}
