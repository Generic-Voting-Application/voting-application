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
            var url = '/Create/#/Manage/' + manageId;
            if (subPage) {
                url += '/' + subPage;
            }
            return url;
        }
    }
})();
