(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('TokenService', TokenService);

    TokenService.$inject = ['$location', '$http', '$localStorage', '$routeParams', '$q', '$timeout'];

    function TokenService($location, $http, $localStorage, $routeParams, $q, $timeout) {

        var service = {
            getToken: getTokenForPoll,
            setToken: setTokenForPoll
        };

        return service;

        function setTokenForPoll(pollId, token) {
            var deferred = $q.defer();

            $localStorage[pollId] = token;
            // Ensure that we've actually saved the values before continuing. (see https://github.com/gsklee/ngStorage/issues/39)
            $timeout(function () { deferred.resolve(); }, 110);

            return deferred.promise;
        }

        function getTokenForPoll(pollId) {

            var deferred = $q.defer();

            if (!pollId) {
                deferred.reject();
                return deferred.promise;
            }

            if ($routeParams['tokenId']) {
                deferred.resolve($routeParams['tokenId']);
                return deferred.promise;
            }

            if ($localStorage[pollId]) {
                deferred.resolve($localStorage[pollId]);
                return deferred.promise;
            }

            $http({
                method: 'GET',
                url: '/api/poll/' + pollId + '/token'
            })
            .success(function (data) {
                var token = data.replace(/\"/g, '');
                setTokenForPoll(pollId, token);

                deferred.resolve(token);
            });

            return deferred.promise;
        }
    }
})();