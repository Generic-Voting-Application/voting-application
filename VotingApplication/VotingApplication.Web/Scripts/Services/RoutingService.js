(function () {
    angular
        .module('GVA.Common')
        .factory('RoutingService', RoutingService);

    function RoutingService() {

        var service = {
            navigateToVotePage: navigateToVotePage,
            navigateToResultsPage: navigateToResultsPage,
            navigateToManagePage: navigateToManagePage,
            navigateToHomePage: navigateToHomePage
        };

        return service;

        function navigateToVotePage(pollId) {
            $window.location.href = 'Poll/#/Vote' + pollId;
        }

        function navigateToResultsPage(pollId) {
            $window.location.href = 'Poll/#/Results' + pollId;
        }

        function navigateToManagePage(manageId, subPage) {
            $window.location.href = 'Manage/#/' + manageId + (subPage ? '/' + subPage : '');
        }

        function navigateToHomePage() {
            $window.location.href = '';
        }
    }
})();
