
class DraggableSvg {


    innerCircleStart = 2;
    innerCircleDiameter = 96; // The inner circle has a radius of 48;

    _touchStartY = undefined
    _slidingValueStart = undefined
    _slidingValueCurrent = undefined
    _slidingValue = undefined
    _start = undefined
    _touchIdentifier = undefined
    _onDrag = undefined
    _isVerticalMove = null
    _isEnabled = undefined
    _svg = undefined
    _clipRect = undefined
    _viewModelReference = undefined
    _propagateSlidingValueChanged = undefined

    initialize(viewModelReference, svg, initialSlidingValue, isEnabledInitialValue) {
        this._viewModelReference = viewModelReference;
        this._svg = svg;
        this._clipRect = svg.querySelector("clipPath > rect");
        this._slidingValue = initialSlidingValue;
        this._isEnabled = isEnabledInitialValue;
        this.updateSliderPosition();

        svg.ontouchstart = e => this.onTouchStart(e);
        svg.ontouchmove = e => this.onTouchMove(e);
        svg.ontouchend = e => this.onTouchEnd(e);
        svg.ontouchcancel = e => this.onTouchEnd(e);

        this._propagateSlidingValueChanged = this.debounce(() => {
                this._viewModelReference.invokeMethodAsync('OnSlidingValueChanged', this._slidingValue);
            },
            200);
    }

    onTouchStart(event) {
        const touchPoint = event.touches[0];
        this._touchIdentifier = touchPoint.identifier;
        this.dragStart({ x: touchPoint.clientX, y: touchPoint.clientY });
    }

    onTouchMove(event) {
        if (!this._isEnabled)
            return;

        const touch = this.getCurrentTouch(event.changedTouches);

        if (!touch)
            return;

        this.drag({ x: touch.clientX, y: touch.clientY });
    }

    onTouchEnd(event) {
        this.setOnDrag(false);
    }

    dragStart(start) {

        if (!this._isEnabled)
            return;

        this._start = start;
        this._isVerticalMove = null;
        this._slidingValueStart = this._slidingValue;
        this._slidingValueCurrent = this._slidingValue;
        this._touchStartY = this.getMousePositionY(this._svg, this._start.y);
    }

    drag(newPosition) {

        if (!this._start)
            return;

        if (this._isVerticalMove === null) {
            this._isVerticalMove = this.isVerticalMove(this._start, newPosition);
        }

        if (!this._isVerticalMove)
            return;

        this.setOnDrag(true);

        const currentY = this.getMousePositionY(this._svg, newPosition.y);
        const deltaY = (currentY - this._touchStartY) / this.innerCircleDiameter;
        const newValue = Math.max(Math.min(this._slidingValueStart - deltaY, 1), 0);

        if (Math.abs(this._slidingValue - newValue) < 0.01)
            return;

        this.setSlidingValue(newValue);
    }

    getCurrentTouch(touchList) {
        for (let i = 0; i < touchList.length; i++) {
            const touch = touchList.item(i);

            if (touch.identifier == this._touchIdentifier)
                return touch;
        }
    }

    isVerticalMove(from, to) {
        const radian = Math.atan2((from.y - to.y), (from.x - to.x));
        const angle = (radian * (180 / Math.PI) + 360) % 360;

        return angle >= 45 && angle < 135 // Vertical move up
        || angle >= 225 && angle < 315; // Vertical move down
    }

    getMousePositionY (svg, clientY) {
        const CTM = svg.getScreenCTM();
        return (clientY - CTM.f) / CTM.d;
    }

    setIsEnabled(value) {
        this._isEnabled = value;
    }

    setSlidingValue(value) {
        this._slidingValue = value;
        this.updateSliderPosition();
    }

    setOnDrag(value) {
        this._onDrag = value;

        if (value) {
            this._svg.classList.add("on-drag");
        } else {
            this._svg.classList.remove("on-drag");
        }
    }

    getSlidingValue() {
        return this._slidingValue;
    }

    updateSliderPosition() {
        if (!this._clipRect || !this._propagateSlidingValueChanged)
            return;

        this._clipRect.setAttribute("y", (this.innerCircleDiameter - Math.round(this._slidingValue * this.innerCircleDiameter, 2)) + this.innerCircleStart);

        this._propagateSlidingValueChanged();
    }

    debounce(func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    }
}

window.DraggableSvg = {
    counter: 1,

    Create: function(viewModelReference, svg, initialSlidingValue, isEnabledInitialValue) {

        this[this.counter] = new DraggableSvg();
        this[this.counter].initialize(viewModelReference, svg, initialSlidingValue, isEnabledInitialValue);

        return this.counter++;
    },

    SetSlidingValue: function (reference, newValue) {
        this[reference].setSlidingValue(newValue);
    },

    SetIsEnabled: function (reference, newValue) {
        this[reference].setIsEnabled(newValue);
    }
};
