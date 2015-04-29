'use strict';

describe('ManagePollTypeController', function () {

    beforeEach(module('GVA.Creation'));

    var scope;

    var manageServiceMock;
    var routingServiceMock;
    var ngDialogMock;

    var manageUpdatePollTypePromise;
    var manageGetVotesPromise;

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

        routingServiceMock = { navigateToManagePage: function () { } };
        spyOn(routingServiceMock, 'navigateToManagePage');

        ngDialogMock = {
            open: function () { }
        };

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
    });
});