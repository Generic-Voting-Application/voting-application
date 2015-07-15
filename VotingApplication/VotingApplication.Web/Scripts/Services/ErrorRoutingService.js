'use strict';

(function () {
    angular
        .module('VoteOn-Common')
        .factory('ErrorRoutingService', ErrorRoutingService);

    ErrorRoutingService.$inject = ['$window', '$location'];

    function ErrorRoutingService($window, $location) {

        var service = {
            navigateToHomePage: navigateToHomePage,

            navigateToEmailNotConfirmedPage: navigateToEmailNotConfirmedPage,
            navigateToPollInviteOnlyPage: navigateToPollInviteOnlyPage

        };

        return service;

        function navigateToHomePage() {
            $window.location.href = '/';
        }

        function navigateToEmailNotConfirmedPage(email) {
            $window.location.href = '/Login/#/EmailNotConfirmed/' + email;
        }

        function navigateToPollInviteOnlyPage(pollId) {
            // This changes the current page and history, so back doesn't redirect to an invalid page.
            // In this case we don't want to keep redirecting away from the Vote/Result page.
            $location.path(pollId + '/InviteOnly').replace();
        }
    }
})();
