﻿window.Swiper.Initialize = (viewModelReference) => {
    window.Swiper.obj = new Swiper('.swiper-container',
        {
            slidesPerView: 3,
            centeredSlides: true,
            pagination: {
                el: '.swiper-pagination',
                clickable: true
            },
            //slideToClickedSlide: true,
            //on: {
            //    slideChangeTransitionEnd: function (swiper) {
            //        viewModelReference.invokeMethodAsync('OnActiveIndexChanged', window.Swiper.obj.realIndex);
            //    }
            //},
            freeMode: true,
            freeModeSticky: true,
            freeModeMomentumRatio: 0.5,
            spaceBetween: window.innerWidth / 10,
        });
}

window.Swiper.Update = () => {
    window.Swiper.obj.update();
}

window.Swiper.ActiveIndex = () => {
    return window.Swiper.obj.realIndex;
}