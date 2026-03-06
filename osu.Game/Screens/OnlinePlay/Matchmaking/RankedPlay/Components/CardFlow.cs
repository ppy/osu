// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    /// <summary>
    /// Container that arranges a collection of <see cref="RankedPlayCard"/>s horizontally.
    /// Layout is not automatic and has to be triggered by calling <see cref="LayoutCards"/>
    /// </summary>
    /// <remarks>
    /// Drawables are expected to be added to this container with an Anchor/Origin of <see cref="Anchor.Centre"/>.
    /// </remarks>
    public partial class CardFlow : Container<RankedPlayCard>
    {
        public float Spacing = 20;

        /// <summary>
        /// Moves all cards into a horizontal arrangement centered within the container's bounds.
        /// </summary>
        /// <param name="stagger">delay to be added to the movement of each subsequent card</param>
        /// <param name="duration">duration of the movement</param>
        /// <param name="easing">easing of the movement</param>
        public void LayoutCards(double stagger = 0, double duration = 400, Easing easing = Easing.OutExpo)
        {
            // makes sure that all facades had a chance to initialize their transforms based on the provided drawQuad
            CheckChildrenLife();

            float totalWidth = Children.Sum(c => c.LayoutSize.X + Spacing) - Spacing;

            float x = -totalWidth / 2;

            double delay = 0;

            foreach (var card in Children)
            {
                card.Delay(delay)
                    .MoveTo(new Vector2(x + card.LayoutSize.X * 0.5f, 0), duration, easing)
                    .RotateTo(0, duration, easing)
                    .ScaleTo(1, duration, easing);

                x += card.LayoutSize.X + Spacing;

                delay += stagger;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        /// <param name="card"></param>
        /// <param name="screenSpaceDrawQuad"></param>
        /// <returns></returns>
        public bool RemoveCard(RankedPlayCardWithPlaylistItem item, [MaybeNullWhen(false)] out RankedPlayCard card, out Quad screenSpaceDrawQuad)
        {
            card = Children.FirstOrDefault(it => it.Item.Equals(item));

            if (card == null)
            {
                screenSpaceDrawQuad = default;
                return false;
            }

            screenSpaceDrawQuad = card.ScreenSpaceDrawQuad;

            Remove(card, false);

            return true;
        }
    }
}
