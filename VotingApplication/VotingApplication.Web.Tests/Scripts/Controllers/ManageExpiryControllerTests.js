'use strict';

describe('ManageExpiryController', function () {

    beforeEach(module('GVA.Manage'));

    var scope;

    var manageServiceMock;
    var routingServiceMock;

    var manageUpdatePollExpiryPromise;

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        manageServiceMock = {
            poll: {},

            updatePollExpiry: function () { },
            registerPollObserver: function () { }
        };

        manageUpdatePollExpiryPromise = $q.defer();
        spyOn(manageServiceMock, 'updatePollExpiry').and.callFake(function () { return manageUpdatePollExpiryPromise.promise; });

        routingServiceMock = { navigateToManagePage: function () { } };
        spyOn(routingServiceMock, 'navigateToManagePage');

        $controller('ManageExpiryController', { $scope: scope, ManageService: manageServiceMock, RoutingService: routingServiceMock });
    }));

    describe('Update Poll', function () {

        it('Makes service call to update poll expiry date', function () {
            scope.updatePoll();

            expect(manageServiceMock.updatePollExpiry.calls.any()).toEqual(true);
        });

        it('Makes service call to Navigate To Manage Page when update service call succeeds', function () {
            manageUpdatePollExpiryPromise.resolve();


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });

        it('Does not make service call to Navigate To Manage Page if delete service call fails', function () {
            manageUpdatePollExpiryPromise.reject();


            scope.updatePoll();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(false);
        });
    });
});