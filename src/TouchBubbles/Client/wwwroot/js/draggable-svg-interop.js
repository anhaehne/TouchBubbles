window.DraggableSvg = {
    GetMousePositionY: (svg, clientY) => {
        var CTM = svg.getScreenCTM();
        return (clientY - CTM.f) / CTM.d;
    }   
}