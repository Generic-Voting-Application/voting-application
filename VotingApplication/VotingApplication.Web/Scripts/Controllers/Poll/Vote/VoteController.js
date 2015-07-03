(function () {
    'use strict';

    angular
        .module('VoteOn-Vote')
        .controller('VoteController', VoteController);

    VoteController.$inject = ['$scope'];

    function VoteController($scope) {

        $scope.poll = { Name: 'Why are we here?', PollType: 'Multi' };

        $scope.choices = [
            { name: 'Choice 1', selected:false },
            { name: 'Choice 2', selected:false },
            { name: 'Choice 3', selected:false },
            { name: 'Choice 4', selected:false }
        ];
    }
})();
