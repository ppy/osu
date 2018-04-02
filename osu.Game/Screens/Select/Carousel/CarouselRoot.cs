// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselRoot : CarouselGroupEagerSelect
    {
        private readonly BeatmapCarousel carousel;

        public CarouselRoot(BeatmapCarousel carousel)
        {
            this.carousel = carousel;
        }

        protected override void PerformSelection()
        {
            if (LastSelected == null)
                carousel.SelectNextRandom();
            else
                base.PerformSelection();
        }
    }
}
