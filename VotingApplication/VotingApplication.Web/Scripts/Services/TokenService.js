(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('TokenService', TokenService);

    TokenService.$inject = ['$location', '$http', '$localStorage', '$routeParams', '$q', '$timeout'];

    function TokenService($location, $http, $localStorage, $routeParams, $q, $timeout) {

        var service = {
            getToken: getTokenForPoll,
            setToken: setTokenForPoll,
            getManageId: getManageIdForPoll,
            setManageId: setManageIdForPoll
        };

        return service;

        function setTokenForPoll(pollId, token) {
            var deferred = $q.defer();

            if (!$localStorage[pollId]) {
                $localStorage[pollId] = {};
            }

            $localStorage[pollId].token = token;
            // Ensure that we've actually saved the values before continuing. (see https://github.com/gsklee/ngStorage/issues/39)
            $timeout(function () { deferred.resolve(); }, 110);

            return deferred.promise;
        }

        function setManageIdForPoll(pollId, manageId) {
            var deferred = $q.defer();

            if (!$localStorage[pollId]) {
                $localStorage[pollId] = {};
            }

            $localStorage[pollId].manageId = manageId;
            // Ensure that we've actually saved the values before continuing. (see https://github.com/gsklee/ngStorage/issues/39)
            $timeout(function () { deferred.resolve(); }, 110);

            return deferred.promise;
        }

        function getManageIdForPoll(pollId) {
            var deferred = $q.defer();

            if (!pollId) {
                deferred.reject();
                return deferred.promise;
            }

            if ($localStorage[pollId]) {
                deferred.resolve($localStorage[pollId].manageId);
                return deferred.promise;
            }

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

                // Fallback for old system
                if (typeof ($localStorage[pollId]) === 'string') {
                    deferred.resolve($localStorage[pollId]);
                } else {
                    deferred.resolve($localStorage[pollId].token);
                }

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