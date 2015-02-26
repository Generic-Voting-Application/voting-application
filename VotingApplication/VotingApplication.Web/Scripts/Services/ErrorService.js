(function () {
    angular.module('GVA.Common').factory('ErrorService', [function () {
        var self = this;

        self.bindModelStateToForm = function (modelState, form, displayGenericError) {
            form.errors = {};

            for (var modelError in modelState) {
                if (modelState.hasOwnProperty(modelError)) {
                    var key = modelError.replace('model.', '').toLowerCase();
                    if (form.hasOwnProperty(key)) {
                        form.errors[key] = modelState[modelError][0];
                    } else {
                        displayGenericError(modelState[modelError][0]);
                    }
                }
            }
        }

        return self;
    }]);
})();