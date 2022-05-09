// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

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
