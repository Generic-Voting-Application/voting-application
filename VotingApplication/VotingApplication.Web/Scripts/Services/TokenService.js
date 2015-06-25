(function () {
    'use strict';

    angular
        .module('GVA.Common')
        .factory('TokenService', TokenService);

    TokenService.$inject = ['$location', '$http', '$localStorage', '$routeParams', '$q', '$timeout'];

    function TokenService($location, $http, $localStorage, $routeParams, $q, $timeout) {

        var service = {
            setToken: setTokenForPoll,
            getManageId: getManageIdForPoll,
            setManageId: setManageIdForPoll,

            retrieveToken: retrieveToken
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

            var storageData = $localStorage[pollId];

            if (storageData) {
                if (storageData.manageId) {
                    deferred.resolve(storageData.manageId);
                } else {
                    deferred.reject();
                }

                return deferred.promise;
            }

            return deferred.promise;
        }

        function retrieveToken(pollId) {

            if (!pollId) {
                return null;
            }

            if ($routeParams['tokenId']) {
                return $routeParams['tokenId'];
            }

            var userData = $localStorage[pollId];
            if (userData) {
                if (userData.token) {
                    return userData.token;
                }
                    // Old style token, not stored in an object, but straight in localStorage.
                else if (typeof userData === 'string') {
                    return userData;
                }
            }

            return null;
        }
    }
})();