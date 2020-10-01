window.Swiper.Initialize = () => {
    window.Swiper.obj = new Swiper('.swiper-container', {
        slidesPerView: 3,
        centeredSlides: true,
        pagination: {
            el: '.swiper-pagination',
            clickable: true,
        },
        slideToClickedSlide: true,
    })
}

window.Swiper.Update = () => {
    window.Swiper.obj.update();
}