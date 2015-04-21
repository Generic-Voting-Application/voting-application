(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('ErrorService', ErrorService);


    function ErrorService() {

        var stringReplacements = {
            'Poll .{8}-.{4}-.{4}-.{4}-.{12} not found': 'Poll does not exist',
            'Poll .{8}-.{4}-.{4}-.{4}-.{12} is invite only': 'This poll is invite only',
            'Invalid ExpiryDate': 'Expiry date must be in the future',
            'Invalid or unspecified': 'Empty',
            'Option Name': 'option name'
        };

        var service = {
            bindModelStateToForm: bindModelState,
            createReadableString: createReadableString
        };

        return service;

        function bindModelState(modelState, form, displayGenericError) {
            form.errors = {};

            for (var modelError in modelState) {
                if (modelState.hasOwnProperty(modelError)) {
                    var key = modelError.replace('model.', '').toLowerCase();

                    if (form.hasOwnProperty(key)) {
                        form.errors[key] = modelState[modelError][0];
                    }
                    else {
                        displayGenericError(modelState[modelError][0]);
                    }
                }
            }
        }

        function createReadableString(string) {
            var readableString = string;

            for (var replacement in stringReplacements) {
                if (stringReplacements.hasOwnProperty(replacement)) {
                    var regEx = new RegExp(replacement, 'g');
                    readableString = readableString.replace(regEx, stringReplacements[replacement]);
                }
            }

            return readableString;
        }
    }
})();