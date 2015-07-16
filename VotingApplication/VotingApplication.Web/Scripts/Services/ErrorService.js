/// <reference path="ErrorRoutingService.js" />

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

            if (response.status === 404) {
                ErrorRoutingService.navigateToPollNotFound();
            }
            else if (response.status === 401) {
                ErrorRoutingService.navigateToPollInviteOnlyPage(pollId);
            }
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