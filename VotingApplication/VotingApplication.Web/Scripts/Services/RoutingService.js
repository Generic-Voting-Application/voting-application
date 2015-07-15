(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('RoutingService', RoutingService);

    RoutingService.$inject = ['$window'];

    function RoutingService($window) {

        var service = {
            navigateToVotePage: navigateToVotePage,
            navigateToResultsPage: navigateToResultsPage,
            navigateToManagePage: navigateToManagePage,
            navigateToHomePage: navigateToHomePage,
            navigateToMyPolls: navigateToMyPolls,
            getVotePageUrl: getVotePageUrl,
            getResultsPageUrl: getResultsPageUrl,
            getManagePageUrl: getManagePageUrl,
            getMyPollsUrl: getMyPollsUrl,


            navigateToLoginPage: navigateToLoginPage,
            navigateToRegisterPage: navigateToRegisterPage
        };

        return service;

        function navigateToVotePage(pollId, token) {
            $window.location.href = getVotePageUrl(pollId, token);
        }

        function navigateToResultsPage(pollId, token) {
            $window.location.href = getResultsPageUrl(pollId, token);
        }

        function navigateToManagePage(manageId, subPage) {
            $window.location.href = getManagePageUrl(manageId, subPage);
        }

        function navigateToMyPolls() {
            $window.location.href = getMyPollsUrl();
        }

        function navigateToHomePage() {
            $window.location.href = '';
        }

        function getVotePageUrl(pollId, tokenId) {
            if (!pollId) {
                return null;
            }

            var url = '/Poll/#/' + pollId + '/Vote';
            if (tokenId) {
                url += '/' + tokenId;
            }
            return url;
        }

        function getResultsPageUrl(pollId, tokenId) {
            if (!pollId) {
                return null;
            }

            var url = '/Poll/#/' + pollId + '/Results';
            if (tokenId) {
                url += '/' + tokenId;
            }
            return url;
        }

        function getManagePageUrl(manageId, subPage) {
            if (!manageId) {
                return null;
            }

            var url = '/Manage/#/Manage/' + manageId;
            if (subPage) {
                url += '/' + subPage;
            }
            return url;
        }

        function getMyPollsUrl() {
            return '/Manage/#/MyPolls/';
        }


        function navigateToLoginPage() {
            $window.location.href = '/Login';
        }

        function navigateToRegisterPage() {
            $window.location.href = '/Register';
        }
    }
})();

(function () {
    'use strict';

    angular
        .module('VoteOn-Common')
        .factory('RoutingService', RoutingService);

    RoutingService.$inject = ['$window', '$location'];

    function RoutingService($window, $location) {

        var service = {
            navigateToVotePage: navigateToVotePage,

            navigateToLoginPage: navigateToLoginPage,
            navigateToRegisterPage: navigateToRegisterPage,

            navigateToForgottenPasswordPage: navigateToForgottenPasswordPage,
            navigateToRegistrationCompletePage: navigateToRegistrationCompletePage,

            navigateToAccountPage: navigateToAccountPage,
            navigateToMyPollsPage: navigateToMyPollsPage,

            navigateToResultsPage: navigateToResultsPage,
            redirectToResultsPage: redirectToResultsPage
        };

        return service;

        function navigateToVotePage(pollId) {
            $window.location.href = '/Poll/#/' + pollId + '/Vote/';
        }

        function navigateToLoginPage() {
            $window.location.href = '/Login';
        }

        function navigateToRegisterPage() {
            $window.location.href = '/Register';
        }

        function navigateToForgottenPasswordPage() {
            $window.location.href = '/Login/#/ForgottenPassword';
        }

        function navigateToRegistrationCompletePage(email) {
            $window.location.href = '/Register/#/RegistrationComplete/' + email;
        }

        function navigateToAccountPage() {
            // Temporarily redirect to the new homepage until we've an account page.
            $window.location.href = '/Create/';
        }

        function navigateToMyPollsPage() {
            // This is the old page, but it's not been updated to material yet.
            $window.location.href = '/Manage/#/MyPolls/';
        }

        function navigateToResultsPage(pollId) {
            $window.location.href = '/Poll/#/' + pollId + '/Results';
        }

        function redirectToResultsPage(pollId) {
            // This changes the current page and history, so back doesn't redirect to an invalid page.
            // For example, when a poll expires, you can't get to the vote page anymore.
            $location.path(pollId + '/Results').replace();
        }
    }
})();
