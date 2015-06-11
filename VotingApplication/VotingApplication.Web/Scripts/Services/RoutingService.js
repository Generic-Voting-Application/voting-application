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
            navigateToRegistrationConfirmation : navigateToRegistrationConfirmation,
            getVotePageUrl: getVotePageUrl,
            getResultsPageUrl: getResultsPageUrl,
            getManagePageUrl: getManagePageUrl,
            getMyPollsUrl: getMyPollsUrl,
            getRegistrationConfirmationUrl: getRegistrationConfirmationUrl
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

        function navigateToRegistrationConfirmation(email) {
            $window.location.href = getRegistrationConfirmationUrl(email);
        }

        function getVotePageUrl(pollId, tokenId) {
            var url = '/Poll/#/Vote/' + pollId;
            if (tokenId) {
                url += '/' + tokenId;
            }
            return url;
        }

        function getResultsPageUrl(pollId, tokenId) {
            var url = '/Poll/#/Results/' + pollId;
            if (tokenId) {
                url += '/' + tokenId;
            }
            return url;
        }

        function getManagePageUrl(manageId, subPage) {
            var url = '/Manage/#/Manage/' + manageId;
            if (subPage) {
                url += '/' + subPage;
            }
            return url;
        }

        function getMyPollsUrl() {
            return '/Manage/#/MyPolls/';
        }

        function getRegistrationConfirmationUrl(email) {

            if (!email) {
                return null;
            }

            return '/Manage/#/RegistrationConfirmation/' + encodeURIComponent(email);
        }
    }
})();
