(function () {
    'use strict';

    angular
        .module('GVA.Manage')
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

        self.getPoll = function (manageId) {

            if (!manageId) {
                return null;
            }

            return $http({
                method: 'GET',
                url: '/api/manage/' + manageId,
                headers: { 'Authorization': 'Bearer ' + (AccountService.account ? AccountService.account.token : '') }
            })
            .then(function (response) {
                self.poll = response.data;
                notifyObservers();

                return response.data;
            });
        };

        self.getPollType = function (manageId) {

            if (!manageId) {
                return null;
            }

            var prom = $q.defer();

            return $http({
                method: 'GET',
                url: '/api/manage/' + manageId + '/pollType/',
            })
            .then(function (response) {
                prom.resolve(response.data);
                return prom.promise;
            });
        };

        self.updatePollExpiry = function (manageId, expiryDateUtc) {
            return $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/expiry/',
                data: { ExpiryDateUtc: expiryDateUtc }
            });
        };

        self.updatePollType = function (manageId, pollTypeConfig) {
            return $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/pollType/',
                data: pollTypeConfig
            });
        };

        self.updateQuestion = function (manageId, question) {
            return $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/question/',
                data: { Question: question }
            });
        };

        self.updatePollMisc = function (manageId, miscConfig) {
            return $http({
                method: 'PUT',
                url: '/api/manage/' + manageId + '/misc/',
                data: miscConfig
            });
        };

        self.getVotes = function (pollId) {
            return $http.get('/api/poll/' + pollId + '/results');
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
                    deleteBallotRequest.VoteDeleteRequests.push({ ChoiceNumber: vote.ChoiceNumber });

                });

                deleteRequests.push(deleteBallotRequest);
            });

            return deleteRequests;
        }

        self.getInvitations = function (manageId) {
            return $http.get('/api/manage/' + manageId + '/invitation');
        };

        self.sendInvitations = function (manageId, invitees) {
            return $http({
                method: 'POST',
                url: '/api/manage/' + manageId + '/invitation',
                data: invitees
            });
        };

        self.getChoices = function (manageId) {
            var deferred = $q.defer();

            $http
                .get('/api/manage/' + manageId + '/choice')
                .success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        };

        self.updateChoices = function (manageId, choices) {
            var deferred = $q.defer();

            $http.put('/api/manage/' + manageId + '/choice',
                {
                    Choices: choices
                })
            .success(function (data) { deferred.resolve(data); });

            return deferred.promise;
        };

        return self;
    }
})();
