(function () {
    angular
        .module('GVA.Voting')
        .factory('TokenService', TokenService);

    TokenService.$inject = ['$location', '$http', '$localStorage'];

    function TokenService($location, $http, $localStorage) {

        var service = {
            getToken: getTokenForPoll
        };

        return service;

        function getTokenForPoll(pollId, callback) {

            if (!pollId) {
                return null;
            }

            if ($localStorage[pollId]) {
                callback($localStorage[pollId]);
                return;
            }

            $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/token'
            })
            .success(function (data, status) {
                var token = data.replace(/\"/g, '');
                $localStorage[pollId] = token;
                if (callback) {
                    callback(token, status);
                }
            })
            .error(function (data, status) {
                if (callback)
                { callback(data, status) }
            });
        }
    }
})();