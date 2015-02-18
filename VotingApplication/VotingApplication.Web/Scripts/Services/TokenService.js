(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.factory('TokenService', ['$location', '$http', function ($location, $http) {
        var self = this;

        self.requestToken = function (pollId, callback) {

            if (!pollId) {
                return null;
            }

            var getUri = '/api/poll/' + pollId + '/token';

            $http.get(getUri)
                .success(function (data, status) { if (callback) { callback(data, status) } })
                .error(function (data, status) { if (callback) { callback(data, status) } });
        }

        return self;
    }]);
})();