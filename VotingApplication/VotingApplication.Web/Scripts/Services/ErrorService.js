/// <reference path="ErrorRoutingService.js" />
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
            'Choice Name': 'choice name'
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

(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .factory('ErrorService', ErrorService);

    ErrorService.$inject = ['$mdToast', 'ErrorRoutingService'];

    function ErrorService($mdToast, ErrorRoutingService) {


        var service = {
            handleLoginError: handleLoginError,
            handleVotingError: handleVotingError
        };

        return service;

        function handleLoginError(errorResponse, email) {

            if (errorResponse) {
                if (errorResponse.data && errorResponse.data.error_description) {
                    handleLoginErrorDescription(errorResponse.data.error_description, email);
                }
            }
            else {
                displayGenericErrorPage();
            }
        }

        function handleLoginErrorDescription(description, email) {

            switch (description) {
                case 'The user name or password is incorrect.':
                    {
                        displayToast(description);
                        break;
                    }
                case 'Email for this user not yet confirmed.':
                    {
                        ErrorRoutingService.navigateToEmailNotConfirmedPage(email);
                        break;
                    }
                default:
                    {
                        displayGenericErrorPage();
                    }
            }
        }

        function handleVotingError(response, pollId) {
            ErrorRoutingService.navigateToPollInviteOnlyPage(pollId);
        }

        function displayToast(content) {

            /*  Known issues with the toast display:
                + Scrollbar when displaying (https://github.com/angular/material/issues/2936)
                + Not able to centre toast (https://github.com/angular/material/issues/1773)
            */

            var toast = $mdToast.simple()
                .content(content)
                .action('Close')
                .highlightAction(true)
                .hideDelay(0)   // hideDelay of 0 never automatically hides the toast.
                .position('bottom right');

            $mdToast.show(toast);
        }

        function displayGenericErrorPage() {
            ErrorRoutingService.navigateToHomePage();
        }
    }
})();