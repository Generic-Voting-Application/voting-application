(function () {
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
            getVotePageUrl: getVotePageUrl,
            getResultsPageUrl: getResultsPageUrl,
            getManagePageUrl: getManagePageUrl
        };

        return service;

        function navigateToVotePage(pollId) {
            $window.location.href = getVotePageUrl(pollId);
        }

        function navigateToResultsPage(pollId) {
            $window.location.href = getResultsPageUrl(pollId);
        }

        function navigateToManagePage(manageId, subPage) {
            $window.location.href = getManagePageUrl(manageId, subPage);
        }

        function navigateToHomePage() {
            $window.location.href = '';
        }

        function getVotePageUrl(pollId, tokenId) {
            return '/Poll/#/Vote/' + pollId + (tokenId ? '/' + tokenId : '');
        }

        function getResultsPageUrl(pollId, tokenId) {
            return '/Poll/#/Results/' + pollId + (tokenId ? '/' + tokenId : '');
        }

        function getManagePageUrl(manageId, subPage) {
            return '/Create/#/Manage/' + manageId + (subPage ? '/' + subPage : '');
        }
    }
})();
