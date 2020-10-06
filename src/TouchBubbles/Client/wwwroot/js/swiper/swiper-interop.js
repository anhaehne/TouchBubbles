window.Swiper.Initialize = (viewModelReference) => {
    window.Swiper.obj = new Swiper('.swiper-container',
        {
            slidesPerView: 3,
            centeredSlides: true,
            pagination: {
                el: '.swiper-pagination',
                clickable: true
            },
            slideToClickedSlide: true,
            on: {
                slideChangeTransitionEnd: function (swiper) {
                    viewModelReference.invokeMethodAsync('OnActiveIndexChanged', swiper.realIndex);
                }
            }
        });
}

window.Swiper.Update = () => {
    window.Swiper.obj.update();
}

window.Swiper.ActiveIndex = () => {
    return window.Swiper.obj.realIndex;
}