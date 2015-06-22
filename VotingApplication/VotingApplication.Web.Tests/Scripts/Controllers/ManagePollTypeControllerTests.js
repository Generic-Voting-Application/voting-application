'use strict';

describe('ManagePollTypeController', function () {

    beforeEach(module('GVA.Manage'));

    var scope;

    var manageServiceMock;
    var routingServiceMock;
    var ngDialogMock;

    var manageUpdatePollTypePromise;
    var manageGetPollTypePromise;

    var observerCallback = function () { };

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        manageServiceMock = {
            getPollType: function () { },
            updatePollType: function () { }
        };

        manageUpdatePollTypePromise = $q.defer();
        manageGetPollTypePromise = $q.defer();
        spyOn(manageServiceMock, 'updatePollType').and.callFake(function () { return manageUpdatePollTypePromise.promise; });
        spyOn(manageServiceMock, 'getPollType').and.callFake(function () { return manageGetPollTypePromise.promise; });

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
            manageGetPollTypePromise.resolve({});

            scope.updatePoll();

            scope.$apply();

            expect(manageServiceMock.updatePollType).toHaveBeenCalled();
        });

        it('Makes service call to Navigate To Manage Page when update service call succeeds', function () {
            manageUpdatePollTypePromise.resolve();
            manageGetPollTypePromise.resolve({});


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage).toHaveBeenCalled();
        });

        it('Does not make service call to Navigate To Manage Page if delete service call fails', function () {
            manageUpdatePollTypePromise.reject();
            manageGetPollTypePromise.resolve({});


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage).not.toHaveBeenCalled();
        });

        it('Makes service call to see if there are any votes on the poll', function () {
            manageGetPollTypePromise.resolve({});


            scope.updatePoll();

            scope.$apply();
            expect(manageServiceMock.getPollType).toHaveBeenCalled();
        });

        it('Makes service call to Update Poll when getVotes service call succeeds', function () {
            manageGetPollTypePromise.resolve({});


            scope.updatePoll();


            scope.$apply();
            expect(manageServiceMock.updatePollType).toHaveBeenCalled();

        });

        it('Asks for confirmation if there are votes for the poll, the poll type is Points, and the max per vote has changed', function () {
            manageGetPollTypePromise.resolve({
                MaxPerVote: 3,
                PollType: 'Points',
                PollHasVotes: true
            });

            observerCallback();

            scope.MaxPerVote = 7;
            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).toHaveBeenCalled();
        });

        it('Asks for confirmation if there are votes for the poll, the poll type is Points, and the max points have changed', function () {

            manageGetPollTypePromise.resolve({
                MaxPerVote: 10,
                PollType: 'Points',
                PollHasVotes: true
            });

            observerCallback();

            scope.MaxPoints = 8;
            scope.updatePoll();

            scope.$apply();
            expect(ngDialogMock.open).toHaveBeenCalled();
        });

        it('Does not ask for confirmation if there are votes for the poll, the poll type is Points, but the max per vote has not changed', function () {

            manageGetPollTypePromise.resolve({
                MaxPerVote: 3,
                PollType: 'Points',
                PollHasVotes: false
            });

            observerCallback();


            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).not.toHaveBeenCalled();
        });

        it('Does not ask for confirmation if there are votes for the poll, the poll type is Points, but the max points have not changed', function () {

            manageGetPollTypePromise.resolve({
                MaxPoints: 10,
                PollType: 'Points',
                PollHasVotes: false
            });

            observerCallback();


            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).not.toHaveBeenCalled();
        });

        it('Asks for confirmation if there are votes for the poll, and the poll type has changed', function () {
            manageGetPollTypePromise.resolve({
                PollType: 'Points',
                PollHasVotes: true
            });
            observerCallback();

            scope.PollType = 'Basic';
            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).toHaveBeenCalled();
        });

        it('Does not ask for confirmation if there are votes for the poll, but the poll type has not changed', function () {

            manageGetPollTypePromise.resolve({
                MaxPoints: 7,
                MaxPerVote: 3,
                PollType: 'Points',
                PollHasVotes: true
            });

            observerCallback();

            scope.updatePoll();


            scope.$apply();
            expect(ngDialogMock.open).not.toHaveBeenCalled();
        });
    });
});