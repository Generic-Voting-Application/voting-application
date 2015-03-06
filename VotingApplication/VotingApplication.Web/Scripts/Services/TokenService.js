(function () {
    angular
        .module('GVA.Voting')
        .factory('TokenService', ['$location', '$http', '$localStorage',
        function ($location, $http, $localStorage) {
            var self = this;

            self.getToken = function (pollId, callback) {

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
                        callback(token, status)
                    }
                })
                .error(function (data, status) { if (callback) { callback(data, status) } });
            }

            return self;
        }]);
})();