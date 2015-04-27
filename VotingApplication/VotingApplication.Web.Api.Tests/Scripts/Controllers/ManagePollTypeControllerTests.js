'use strict';

describe('ManagePollTypeController', function () {

    beforeEach(module('GVA.Creation'));

    var scope;

    var manageServiceMock;
    var routingServiceMock;
    var ngDialogMock;

    var manageUpdatePollTypePromise;

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        manageServiceMock = {
            poll: {},

            registerPollObserver: function () { },
            getPoll: function () { },
            getVotes: function (pollId, callback) { callback({}); },
            updatePollType: function () { }
        };

        manageUpdatePollTypePromise = $q.defer();
        spyOn(manageServiceMock, 'updatePollType').and.callFake(function () { return manageUpdatePollTypePromise.promise; });

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
            scope.updatePoll();

            expect(manageServiceMock.updatePollType.calls.any()).toEqual(true);
        });

        it('Makes service call to Navigate To Manage Page when update service call succeeds', function () {
            manageUpdatePollTypePromise.resolve();


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });

        it('Does not make service call to Navigate To Manage Page if delete service call fails', function () {
            manageUpdatePollTypePromise.reject();


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(false);
        });
    });
});