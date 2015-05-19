'use strict';

describe('ManageChoiceController', function () {

    beforeEach(module('GVA.Creation'));

    var scope;
    var manageServiceMock;
    var getChoicesPromise;
    var updateChoicesPromise;


    var routingServiceMock;


    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();
        scope.voters = [];
        scope.votersToRemove = [];

        manageServiceMock = {
            registerPollObserver: function () { },
            getChoices: function () { },
            updateChoices: function () { }
        };
        getChoicesPromise = $q.defer();
        updateChoicesPromise = $q.defer();
        spyOn(manageServiceMock, 'getChoices').and.callFake(function () { return getChoicesPromise.promise; });
        spyOn(manageServiceMock, 'updateChoices').and.callFake(function () { return updateChoicesPromise.promise; });

        routingServiceMock = { navigateToManagePage: function () { } };
        spyOn(routingServiceMock, 'navigateToManagePage');


        $controller('ManageChoiceController', { $scope: scope, ManageService: manageServiceMock, RoutingService: routingServiceMock });
    }));

    it('Loads Choices from service', function () {
        var getChoicesResponse = [{
            ChoiceNumber: 1,
            Name: 'First Choice',
            Description: null
        },
        {
            ChoiceNumber: 2,
            Name: 'Second Choice',
            Description: 'This is the second option'
        }];

        getChoicesPromise.resolve(getChoicesResponse);

        scope.$apply();
        expect(scope.choices).toBe(getChoicesResponse);
    });

    describe('Remove Choice', function () {

        it('Removes the choice', function () {

            var option1 = {
                ChoiceNumber: 1,
                Name: 'First Choice',
                Description: null
            };

            var option2 = {
                ChoiceNumber: 2,
                Name: 'Second Choice',
                Description: 'This is the second option'
            };

            scope.choices = [option1, option2];


            scope.removeChoice(option1);


            expect(scope.choices).toEqual([option2]);
        });

    });

    describe('Return Without Save', function () {

        it('Does not make service call to delete voters', function () {
            scope.returnWithoutSave();

            expect(manageServiceMock.updateChoices.calls.any()).toEqual(false);
        });

        it('Makes service call to navigateToManagePage', function () {
            scope.returnWithoutSave();

            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });
    });

    describe('Save Choices And Return', function () {

        it('Makes service call to delete voters', function () {
            scope.saveChoicesAndReturn();

            expect(manageServiceMock.updateChoices.calls.any()).toEqual(true);
        });

        it('Makes service call to Navigate To Manage Page when delete service call succeeds', function () {
            updateChoicesPromise.resolve();


            scope.saveChoicesAndReturn();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });

        it('Does not make service call to Navigate To Manage Page if delete service call fails', function () {
            updateChoicesPromise.reject();


            scope.saveChoicesAndReturn();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(false);
        });
    });
});