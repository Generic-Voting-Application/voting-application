(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('HomePageController', function ($scope) {
        $scope.models = {
            pageTitle: 'Pollster'
        };
    });

})();