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
            navigateToConfirmRegistration : navigateToConfirmRegistration,
            getVotePageUrl: getVotePageUrl,
            getResultsPageUrl: getResultsPageUrl,
            getManagePageUrl: getManagePageUrl,
            getMyPollsUrl: getMyPollsUrl,
            getConfirmRegistrationUrl: getConfirmRegistrationUrl
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

        function navigateToConfirmRegistration(email) {
            $window.location.href = getConfirmRegistrationUrl(email);
        }

        function getVotePageUrl(pollId, tokenId) {
            if (!pollId) {
                return null;
            }

            var url = '/Poll/#/Vote/' + pollId;
            if (tokenId) {
                url += '/' + tokenId;
            }
            return url;
        }

        function getResultsPageUrl(pollId, tokenId) {
            if (!pollId) {
                return null;
            }

            var url = '/Poll/#/Results/' + pollId;
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

        function getConfirmRegistrationUrl(email) {

            if (!email) {
                return null;
            }

            return '/Manage/#/ConfirmRegistration/' + encodeURIComponent(email);
        }
    }
})();
