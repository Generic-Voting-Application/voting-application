'use strict';

describe('ManageOptionController', function () {

    beforeEach(module('GVA.Creation'));

    var scope;
    var manageServiceMock;
    var getOptionsPromise;
    var updateOptionsPromise;


    var routingServiceMock;


    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();
        scope.voters = [];
        scope.votersToRemove = [];

        manageServiceMock = {
            registerPollObserver: function () { },
            getOptions: function () { },
            updateOptions: function () { }
        };
        getOptionsPromise = $q.defer();
        updateOptionsPromise = $q.defer();
        spyOn(manageServiceMock, 'getOptions').and.callFake(function () { return getOptionsPromise.promise; });
        spyOn(manageServiceMock, 'updateOptions').and.callFake(function () { return updateOptionsPromise.promise; });

        routingServiceMock = { navigateToManagePage: function () { } };
        spyOn(routingServiceMock, 'navigateToManagePage');


        $controller('ManageOptionController', { $scope: scope, ManageService: manageServiceMock, RoutingService: routingServiceMock });
    }));

    it('Loads Options from service', function () {
        var getOptionsResponse = [{
            OptionNumber: 1,
            Name: 'First Option',
            Description: null
        },
        {
            OptionNumber: 2,
            Name: 'Second Option',
            Description: 'This is the second option'
        }];

        getOptionsPromise.resolve(getOptionsResponse);

        scope.$apply();
        expect(scope.options).toBe(getOptionsResponse);
    });

    describe('Remove Option', function () {

        it('Removes the option', function () {

            var option1 = {
                OptionNumber: 1,
                Name: 'First Option',
                Description: null
            };

            var option2 = {
                OptionNumber: 2,
                Name: 'Second Option',
                Description: 'This is the second option'
            };

            scope.options = [option1, option2];


            scope.removeOption(option1);


            expect(scope.options).toEqual([option2]);
        });

    });

    describe('Return Without Save', function () {

        it('Does not make service call to delete voters', function () {
            scope.returnWithoutSave();

            expect(manageServiceMock.updateOptions.calls.any()).toEqual(false);
        });

        it('Makes service call to navigateToManagePage', function () {
            scope.returnWithoutSave();

            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });
    });

    describe('Save Options And Return', function () {

        it('Makes service call to delete voters', function () {
            scope.saveOptionsAndReturn();

            expect(manageServiceMock.updateOptions.calls.any()).toEqual(true);
        });

        it('Makes service call to Navigate To Manage Page when delete service call succeeds', function () {
            updateOptionsPromise.resolve();


            scope.saveOptionsAndReturn();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });

        it('Does not make service call to Navigate To Manage Page if delete service call fails', function () {
            updateOptionsPromise.reject();


            scope.saveOptionsAndReturn();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(false);
        });
    });
});