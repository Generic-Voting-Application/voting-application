(function () {
    'use strict';

    angular
        .module('GVA.Creation')
        .factory('ManageService', ManageService);


    ManageService.$inject = ['$location', '$http', '$routeParams', '$localStorage', '$q', 'AccountService'];

    function ManageService($location, $http, $routeParams, $localStorage, $q, AccountService) {
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

            $http({
                method: 'GET',
                url: '/api/manage/' + manageId,
                headers: { 'Authorization': 'Bearer ' + (AccountService.account ? AccountService.account.token : '') }
            })
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

        self.updatePollExpiry = function (manageId, expiryDate, callback, failureCallback) {
            $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/expiry/',
                data: { ExpiryDate: expiryDate }
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

        self.updatePollType = function (manageId, pollTypeConfig, callback, failureCallback) {
            $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/pollType/',
                data: pollTypeConfig
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

        self.updateQuestion = function (manageId, question, callback, failureCallback) {
            $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/question/',
                data: { Question: question }
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

        self.updatePollMisc = function (manageId, miscConfig, callback, failureCallback) {
            $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/misc/',
                data: miscConfig
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

            $http.get('/api/poll/' + pollId + '/results')
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
                       headers: { 'Content-Type': 'application/json; charset=utf-8' },
                       data: { BallotDeleteRequests: createDeleteRequest(votersToRemove) }
                   })
                .success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        };

        function createDeleteRequest(votersToRemove) {

            var deleteRequests = [];

            votersToRemove.forEach(function (item) {

                var deleteBallotRequest = {
                    BallotManageGuid: item.BallotManageGuid,
                    VoteDeleteRequests: []
                };

                item.Votes.forEach(function (vote) {
                    deleteBallotRequest.VoteDeleteRequests.push({ OptionNumber: vote.OptionNumber });

                });

                deleteRequests.push(deleteBallotRequest);
            });

            return deleteRequests;
        }

        self.setVisited = function (manageId) {
            $localStorage[manageId] = { visited: true };
        };

        self.getVisited = function (manageId) {
            if (!$localStorage[manageId]) {
                return false;
            }
            return $localStorage[manageId].visited;
        };

        self.getInvitations = function (manageId, callback, failureCallback) {
            $http.get('/api/manage/' + manageId + '/invitation')
                .success(function (data) {
                    if (callback) {
                        callback(data);
                    }
                })
                .error(function (data) {
                    if (failureCallback) {
                        failureCallback(data);
                    }
                });
        };

        self.sendInvitations = function (manageId, invitees, callback, failureCallback) {
            $http({
                method: 'POST',
                url: '/api/manage/' + manageId + '/invitation',
                data: invitees
            }).success(function (data) {
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

        self.getOptions = function (manageId) {
            var deferred = $q.defer();

            $http
                .get('/api/manage/' + manageId + '/option')
                .success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        };

        self.updateOptions = function (manageId, options) {
            var deferred = $q.defer();

            $http.put('/api/manage/' + manageId + '/option',
                {
                    Options: options
                })
            .success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        };

        return self;
    }
})();
