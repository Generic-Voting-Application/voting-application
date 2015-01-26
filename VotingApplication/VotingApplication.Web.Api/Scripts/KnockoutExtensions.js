define(['jquery', 'knockout', 'datetimepicker', 'moment', 'jqueryUI'], function ($, ko, datetimepicker, moment) {

    // Custom binding for initialising a date-time picker
    ko.bindingHandlers.datetimepicker = {
        init: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            var options = ko.unwrap(valueAccessor());

            var getDateOption = function getDateOption(option) {
                if (option !== undefined) {
                    var optionParts = option.split(' ');
                    if (optionParts.length > 0) {
                        var count = optionParts[0];
                        var units = optionParts.length > 1 ? optionParts[1] : 'days';
                        return moment().add(count, units);
                    }
                }
                return undefined;
            };

            // Work out the date-time picker settings from the binding options
            var pickerSettings = {
                defaultDate: getDateOption(options.default),
                minDate: getDateOption(options.min)
            };

            // Initialise the jQueryUI control
            $(element).datetimepicker(pickerSettings);

            // Trigger knockout to update the bound variable with the default value
            ko.utils.triggerEvent(element, "change");
        }
    };

    // Custom binding for a window that scrolls to the bottom when the observable changes
    ko.bindingHandlers.scrollOnChange = {
        update: function (element, valueAccessor) {
            var value = ko.unwrap(valueAccessor()),
                $element = $(element);

            // Animate to end of scroll window
            $element.animate({
                scrollTop: element.scrollHeight
            });
        }
    };

});