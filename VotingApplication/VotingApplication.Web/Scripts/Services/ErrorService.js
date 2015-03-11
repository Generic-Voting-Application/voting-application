(function () {
    angular
        .module('GVA.Common')
        .factory('ErrorService', ErrorService);


    function ErrorService() {
        var service = {
            bindModelStateToForm: bindModelState
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
    }
})();