(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .factory('ManageService', ManageService);


    ManageService.$inject = ['$location', '$http', '$routeParams', '$localStorage', '$q'];

    function ManageService($location, $http, $routeParams, $localStorage, $q) {
        var self = this;

        var observerCallbacks = [];

        var notifyObservers = function () {
            angular.forEach(observerCallbacks, function (callback) {
                callback();
            });
        };

        self.poll = null;

        self.registerPollObserver = function (callback) {

            if (self.poll === null) {
                self.getPoll($routeParams.manageId);
            }

            observerCallbacks.push(callback);
        };

        self.getPoll = function (manageId, callback, failureCallback) {

            if (!manageId) {
                return null;
            }

            $http.get('/api/manage/' + manageId)
                .success(function (data) {
                    self.poll = data;
                    notifyObservers();
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data, status) {
                    if (failureCallback) {
                        failureCallback(data, status);
                    }
                });
        };

        self.updatePoll = function (manageId, poll, callback, failureCallback) {

            $http({
                method: 'PUT',
                url: '/api/manage/' + manageId,
                data: poll
            })
                .success(function (data) {
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data, status) {
                    if (failureCallback) {
                        failureCallback(data, status);
                    }
                });

        };

        self.getVotes = function (pollId, callback, failureCallback) {

            $http.get('/api/poll/' + pollId + '/vote')
                .success(function (data) {
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data, status) {
                    if (failureCallback) {
                        failureCallback(data, status);
                    }
                });
        };

        self.getVoters = function (manageId) {
            var deferred = $q.defer();

            $http
                .get('/api/manage/' + manageId + '/voters')
                .success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        };

        self.deleteVoters = function (manageId, votersToRemove) {
            var deferred = $q.defer();

            $http
                .delete('/api/manage/' + manageId + '/voters',
               {
                   headers: {
                       'Content-Type': 'application/json; charset=utf-8'
                   },
                   data: {
                       votersToRemove: votersToRemove
                   }
               }
                ).success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        };

        self.setVisited = function (manageId) {
            $localStorage[manageId] = { visited: true };
        };

        self.getVisited = function (manageId) {
            if (!$localStorage[manageId]) {
                return false;
            }
            return $localStorage[manageId].visited;
        };

        self.sendInvitations = function (manageId, callback, failureCallback) {
            $http.post('/api/manage/' + manageId + '/invitation')
                .success(function (data) {
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data, status) {
                    if (failureCallback) {
                        failureCallback(data, status);
                    }
                });
        };

        return self;
    }
})();
