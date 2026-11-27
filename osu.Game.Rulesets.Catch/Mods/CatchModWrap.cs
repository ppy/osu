// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Catch.Mods
{
    public partial class CatchModWrap : Mod, IApplicableToDrawableRuleset<CatchHitObject>, IApplicableToDrawableHitObject, IApplicableToPlayer, IApplicableToBeatmap
    {
        public override string Name => "Wrap";
        public override string Acronym => "WR";
        public override LocalisableString Description => "Think with portals on the playfield's walls!";
        public override ModType Type => ModType.Conversion;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModRelax) };

        private DrawableCatchRuleset drawableRuleset = null!;
        private CatcherArea catcherArea = null!;

        public void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            this.drawableRuleset = (DrawableCatchRuleset)drawableRuleset;
            catcherArea = ((CatchPlayfield)this.drawableRuleset.Playfield).CatcherArea;

            PlayfieldAdjustmentContainer portalContainer = this.drawableRuleset.CreatePlayfieldAdjustmentContainer();

            portalContainer.Colour = Colour4.Purple;

            const float portal_width = 1.0f;

            portalContainer.Add(new Box { Width = portal_width, Height = CatchPlayfield.HEIGHT, Anchor = Anchor.CentreLeft, Origin = Anchor.CentreLeft });
            portalContainer.Add(new Box { Width = portal_width, Height = CatchPlayfield.HEIGHT, Anchor = Anchor.CentreRight, Origin = Anchor.CentreRight });

            // TODO: All of what's inside the playfield should visually loop around horizontally if they are on the left or right wall
            this.drawableRuleset.Overlays.Add(portalContainer);
        }

        public void ApplyToDrawableHitObject(DrawableHitObject drawable)
        {
            if (drawable is not DrawableCatchHitObject catchHitObject)
                return;

            catchHitObject.CheckPosition = hitObject =>
            {
                if (catcherArea.Catcher.CanCatch(hitObject))
                    return true;

                // The following is Catcher's CanCatch adapted for when fruits do wraparound

                float halfCatchWidth = catcherArea.Catcher.CatchWidth * 0.5f;

                if (hitObject.EffectiveX <= halfCatchWidth) // Fruit wrapping around the playfield's left wall sticking out of the right wall
                {
                    float wraparoundHitObjectEffectiveX = CatchPlayfield.WIDTH + hitObject.EffectiveX;
                    return wraparoundHitObjectEffectiveX >= catcherArea.Catcher.X - halfCatchWidth &&
                           wraparoundHitObjectEffectiveX <= catcherArea.Catcher.X + halfCatchWidth;
                }

                if (hitObject.EffectiveX >= CatchPlayfield.WIDTH - halfCatchWidth) // Fruit wrapping around the playfield's right wall sticking out of the left wall
                {
                    float wraparoundHitObjectEffectiveX = hitObject.EffectiveX - CatchPlayfield.WIDTH;
                    return wraparoundHitObjectEffectiveX >= catcherArea.Catcher.X - halfCatchWidth &&
                           wraparoundHitObjectEffectiveX <= catcherArea.Catcher.X + halfCatchWidth;
                }

                return false;
            };
        }

        public void ApplyToPlayer(Player player)
        {
            if (!drawableRuleset.HasReplayLoaded.Value)
                catcherArea.Add(new CatcherWraparoundHelper(catcherArea));
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            initialiseHyperDashForWrapMod(beatmap);
        }

        // An adaptation of CatchBeatmapProcessor's initialiseHyperDash
        private static void initialiseHyperDashForWrapMod(IBeatmap beatmap)
        {
            var palpableObjects = CatchBeatmap.GetPalpableObjects(beatmap.HitObjects)
                                              .Where(h => h is Fruit || (h is Droplet && h is not TinyDroplet))
                                              .ToArray();

            double halfCatcherWidth = Catcher.CalculateCatchWidth(beatmap.Difficulty) / 2;

            // Todo: This is wrong. osu!stable calculated hyperdashes using the full catcher size, excluding the margins.
            // This should theoretically cause impossible scenarios, but practically, likely due to the size of the playfield, it doesn't seem possible.
            // For now, to bring gameplay (and diffcalc!) completely in-line with stable, this code also uses the full catcher size.
            halfCatcherWidth /= Catcher.ALLOWED_CATCH_RANGE;

            int lastDirection = 0;
            double lastExcess = halfCatcherWidth;

            for (int i = 0; i < palpableObjects.Length - 1; i++)
            {
                var currentObject = palpableObjects[i];
                var nextObject = palpableObjects[i + 1];

                // Reset variables in-case values have changed (e.g. after applying HR)
                currentObject.HyperDashTarget = null;
                currentObject.DistanceToHyperDash = 0;

                // Wrap: Calculation is different for if the shorter path to a fruit is a wraparound
                bool isNotWraparound = Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) <= CatchPlayfield.CENTER_X;

                int thisDirection = (nextObject.EffectiveX > currentObject.EffectiveX ? 1 : -1) * (isNotWraparound ? 1 : -1); // Wrap: Wraparound being the shorter path means turn around to it

                // Int truncation added to match osu!stable.
                double timeToNext = (int)nextObject.StartTime - (int)currentObject.StartTime - 1000f / 60f / 4; // 1/4th of a frame of grace time, taken from osu-stable
                double distanceToNext = // Wrap: Wraparound path is what is not of the default path of the osu!catch playfield's width
                    (isNotWraparound ? Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX) : CatchPlayfield.WIDTH - Math.Abs(nextObject.EffectiveX - currentObject.EffectiveX))
                    - (lastDirection == thisDirection ? lastExcess : halfCatcherWidth);
                float distanceToHyper = (float)(timeToNext * Catcher.BASE_DASH_SPEED - distanceToNext);

                if (distanceToHyper < 0)
                {
                    currentObject.HyperDashTarget = nextObject;
                    lastExcess = halfCatcherWidth;
                }
                else
                {
                    currentObject.DistanceToHyperDash = distanceToHyper;
                    lastExcess = Math.Clamp(distanceToHyper, 0, halfCatcherWidth);
                }

                lastDirection = thisDirection;
            }
        }

        private partial class CatcherWraparoundHelper : Drawable, IKeyBindingHandler<CatchAction>
        {
            private readonly CatcherArea catcherArea;

            // To store a calculated CatcherArea's currentDirection because it is private
            private int currentDirection;

            public CatcherWraparoundHelper(CatcherArea catcherArea)
            {
                this.catcherArea = catcherArea;
            }

            public bool OnPressed(KeyBindingPressEvent<CatchAction> e)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeft:
                        currentDirection--;
                        break;

                    case CatchAction.MoveRight:
                        currentDirection++;
                        break;

                    case CatchAction.Dash: // bool Dashing is already public in CatcherArea
                        break;
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<CatchAction> e)
            {
                switch (e.Action)
                {
                    case CatchAction.MoveLeft:
                        currentDirection++;
                        break;

                    case CatchAction.MoveRight:
                        currentDirection--;
                        break;

                    case CatchAction.Dash: // bool Dashing is already public in CatcherArea
                        break;
                }
            }

            protected override void Update()
            {
                if (currentDirection == 0)
                    return;

                float newUnclampedPosition = (float)(catcherArea.Catcher.X + catcherArea.Catcher.Speed * currentDirection * catcherArea.Clock.ElapsedFrameTime);
                if (newUnclampedPosition >= 0.0f && newUnclampedPosition <= CatchPlayfield.WIDTH)
                    return;

                // % can return incorrect values with negative parameters, so modulus using floored division is manually implemented and used here.
                // Catcher's X is intentionally set to the precise out-of-bounds X that CatcherArea's Update will cancel out with to place the Catcher on the other side of the playfield.
                // This pre-Catcher-Area X computation setup also conveniently makes Catcher's updated VisualDirection stay correct.
                // Maths: x_new = x + delta_x  ====>  x_end = x_new - width * floor(x_new / width)  ====>  x_pre_catcher_area = x_end - delta_x = x - width * floor(x_new / width)
                // ====>  x_post_catcher_area = x_pre_catcher_area + delta_x = x_end
                catcherArea.Catcher.X = (float)(catcherArea.Catcher.X - CatchPlayfield.WIDTH * Math.Floor(newUnclampedPosition / CatchPlayfield.WIDTH));
            }
        }
    }
}
