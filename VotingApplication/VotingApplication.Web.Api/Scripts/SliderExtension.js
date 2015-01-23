define("SliderExtension", ['jquery', 'knockout'], function ($, ko) {

    function SliderExtension($element, valueObservable, rangeOption) {

        var prevRange = [0, 0];

        this.initialise = function () {
            $element.slider({
                range: true,
                slide: function (event, ui) {
                    valueObservable(ui.values);
                }
            });
        };

        this.update = function () {
            // Set the range (if changed
            var range = ko.unwrap(rangeOption);
            if (range[0] !== prevRange[0]) $element.slider("option", "min", range[0]);
            if (range[1] !== prevRange[1]) $element.slider("option", "max", range[1]);
            prevRange = range;

            // Set the current value
            var value = ko.unwrap(valueObservable);
            $element.slider("values", value[0], value[1]);
        };

        this.initialise();
    };

    ko.bindingHandlers.slider = {
        init: function (element, valueAccessor, allBindingsAccessor) {
            var $element = $(element);
            $element.data('slider', new SliderExtension(
                $element, valueAccessor(), allBindingsAccessor().range));
        },
        update: function (element) {
            $(element).data('slider').update();
        }
    };

});
