'use strict';

(function () {
    angular
        .module('VoteOn-Common')
        .factory('ErrorRoutingService', ErrorRoutingService);

    ErrorRoutingService.$inject = ['$window'];

    function ErrorRoutingService($window) {

        var service = {
            navigateToHomePage: navigateToHomePage,

            navigateToEmailNotConfirmedPage: navigateToEmailNotConfirmedPage

        };

        return service;

        function navigateToHomePage() {
            $window.location.href = '/';
        }

        function navigateToEmailNotConfirmedPage(email) {
            $window.location.href = '/Login/#/EmailNotConfirmed/' + email;
        }
    }
})();
