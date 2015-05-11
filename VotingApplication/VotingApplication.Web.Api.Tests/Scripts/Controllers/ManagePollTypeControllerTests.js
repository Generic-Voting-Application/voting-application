'use strict';

describe('ManagePollTypeController', function () {

    beforeEach(module('GVA.Creation'));

    var scope;

    var manageServiceMock;
    var routingServiceMock;
    var ngDialogMock;

    var manageUpdatePollTypePromise;
    var manageGetVotesPromise;

    var observerCallback = function () { };

    var pollData = {};
    var votesData = {};

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        manageServiceMock = {
            poll: {},

            registerPollObserver: function () { },
            getPoll: function () { },
            getVotes: function () { },
            updatePollType: function () { }
        };

        manageUpdatePollTypePromise = $q.defer();
        manageGetVotesPromise = $q.defer();
        spyOn(manageServiceMock, 'updatePollType').and.callFake(function () { return manageUpdatePollTypePromise.promise; });
        spyOn(manageServiceMock, 'getVotes').and.callFake(function () { return manageGetVotesPromise.promise; });
        spyOn(manageServiceMock, 'registerPollObserver').and.callFake(function (callback) { observerCallback = callback; });
        spyOn(manageServiceMock, 'getPoll').and.callFake(function () {
            observerCallback();
            return pollData;
        });

        routingServiceMock = { navigateToManagePage: function () { } };
        spyOn(routingServiceMock, 'navigateToManagePage');

        ngDialogMock = {
            open: function () { }
        };
        spyOn(ngDialogMock, 'open');

        $controller('ManagePollTypeController', {
            $scope: scope,
            ManageService: manageServiceMock,
            RoutingService: routingServiceMock,
            ngDialog: ngDialogMock
        });
    }));

    describe('Update Poll', function () {

        it('Makes service call to update poll type', function () {
            manageGetVotesPromise.resolve(votesData);

            scope.updatePoll();

            scope.$apply();

            expect(manageServiceMock.updatePollType).toHaveBeenCalled();
        });

        it('Makes service call to Navigate To Manage Page when update service call succeeds', function () {
            manageUpdatePollTypePromise.resolve();
            manageGetVotesPromise.resolve(votesData);


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage).toHaveBeenCalled();
        });

        it('Does not make service call to Navigate To Manage Page if delete service call fails', function () {
            manageUpdatePollTypePromise.reject();
            manageGetVotesPromise.resolve(votesData);


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage).not.toHaveBeenCalled();
        });

        it('Makes service call to see if there are any votes on the poll', function () {
            manageGetVotesPromise.resolve(votesData);


            scope.updatePoll();

            scope.$apply();
            expect(manageServiceMock.getVotes).toHaveBeenCalled();
        });

        it('Makes service call to Update Poll when getVotes service call succeeds', function () {
            manageGetVotesPromise.resolve(votesData);


            scope.updatePoll();


            scope.$apply();
            expect(manageServiceMock.updatePollType).toHaveBeenCalled();

        });

        it('Asks for confirmation if there are votes for the poll, the poll type is Points, and the max per vote has changed', function () {
            var poll = {
                MaxPerVote: 3,
                PollType: 'Points'
            };

            var getVotesData = {

                data: {
                    Votes: [
                        {
                            OptionId: 1,
                            VoteValue: 1,
                            VoterName: 'Bob'
                        }
                    ]
                }
            };

            manageGetVotesPromise.resolve(getVotesData);

            manageServiceMock.poll = poll;
            observerCallback();

            scope.poll.MaxPerVote = 7;
            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).toHaveBeenCalled();
        });

        it('Asks for confirmation if there are votes for the poll, the poll type is Points, and the max point have changed', function () {
            var poll = {
                MaxPoints: 10,
                PollType: 'Points'
            };

            var getVotesData = {

                data: {
                    Votes: [
                        {
                            OptionId: 1,
                            VoteValue: 1,
                            VoterName: 'Bob'
                        }
                    ]
                }
            };

            manageGetVotesPromise.resolve(getVotesData);

            manageServiceMock.poll = poll;
            observerCallback();

            scope.poll.MaxPoints = 8;
            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).toHaveBeenCalled();
        });

        it('Does not ask for confirmation if there are votes for the poll, the poll type is Points, but the max per vote has not changed', function () {
            var poll = {
                MaxPerVote: 3,
                PollType: 'Points'
            };

            var getVotesData = {

                data: {
                    Votes: [
                        {
                            OptionId: 1,
                            VoteValue: 1,
                            VoterName: 'Bob'
                        }
                    ]
                }
            };
            manageGetVotesPromise.resolve(getVotesData);

            manageServiceMock.poll = poll;
            observerCallback();


            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).not.toHaveBeenCalled();
        });

        it('Does not ask for confirmation if there are votes for the poll, the poll type is Points, but the max per vote has not changed', function () {
            var poll = {
                MaxPoints: 10,
                PollType: 'Points'
            };

            var getVotesData = {

                data: {
                    Votes: [
                        {
                            OptionId: 1,
                            VoteValue: 1,
                            VoterName: 'Bob'
                        }
                    ]
                }
            };

            manageGetVotesPromise.resolve(getVotesData);

            manageServiceMock.poll = poll;
            observerCallback();


            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).not.toHaveBeenCalled();
        });

        it('Asks for confirmation if there are votes for the poll, and the poll type has changed', function () {
            var poll = {
                PollType: 'Points'
            };

            var getVotesData = {

                data: {
                    Votes: [
                        {
                            OptionId: 1,
                            VoteValue: 1,
                            VoterName: 'Bob'
                        }
                    ]
                }
            };
            manageGetVotesPromise.resolve(getVotesData);

            manageServiceMock.poll = poll;
            observerCallback();

            scope.poll.PollType = 'Basic';
            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).toHaveBeenCalled();
        });

        it('Does not ask for confirmation if there are votes for the poll, but the poll type has not changed', function () {
            var poll = {
                PollType: 'Points'
            };

            var getVotesData = {

                data: {
                    Votes: [
                        {
                            OptionId: 1,
                            VoteValue: 1,
                            VoterName: 'Bob'
                        }
                    ]
                }

            };

            manageGetVotesPromise.resolve(getVotesData);

            manageServiceMock.poll = poll;
            observerCallback();

            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).not.toHaveBeenCalled();
        });
    });
});