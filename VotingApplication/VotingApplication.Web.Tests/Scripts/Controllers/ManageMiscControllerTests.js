'use strict';

describe('Manage Misc Controller', function () {

    beforeEach(module('GVA.Manage'));

    var scope;

    var mockManageService;
    var mockNavigationService;

    var manageUpdatePollMiscPromise;

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();
        scope.poll = {};


        mockManageService = {
            poll: {},
            registerPollObserver: function () { },
            updatePollMisc: function () { }
        };
        manageUpdatePollMiscPromise = $q.defer();
        spyOn(mockManageService, 'updatePollMisc').and.callFake(function () { return manageUpdatePollMiscPromise.promise; });


        mockNavigationService = {
            navigateToManagePage: function () { }
        };
        spyOn(mockNavigationService, 'navigateToManagePage').and.callThrough();

        $controller('ManageMiscController', { $scope: scope, ManageService: mockManageService, RoutingService: mockNavigationService });
    }));

    describe('Update Poll', function () {

        it('Makes service call to Navigate To Manage Page when update service call succeeds', function () {
            manageUpdatePollMiscPromise.resolve();


            scope.updatePoll();


            scope.$apply();
            expect(mockNavigationService.navigateToManagePage).toHaveBeenCalled();
        });

        it('Does not make service call to Navigate To Manage Page if update service call fails', function () {
            manageUpdatePollMiscPromise.reject();


            scope.updatePoll();


            scope.$apply();
            expect(mockNavigationService.navigateToManagePage).not.toHaveBeenCalled();
        });

    });

});