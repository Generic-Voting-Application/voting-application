'use strict';

describe('Unregistered Dashboard Controller', function () {

    beforeEach(module('GVA.Creation'));


    var scope;

    var mockPollService;
    var mockRoutingService;
    var mockTokenService;

    var createPollPromise;
    var setTokenPromise;

    var newPollUUID = '9C884B24-9B76-4FF8-A074-36AEB1EC3920';
    var newTokenGuid = 'B14A90C0-078A-4996-B0FA-EA09A0EB6FFE';

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        mockPollService = {
            createPoll: function () { }
        };
        createPollPromise = $q.defer();
        spyOn(mockPollService, 'createPoll').and.callFake(function () { return createPollPromise.promise; });

        mockRoutingService = {
            navigateToManagePage: function () { }
        };
        spyOn(mockRoutingService, 'navigateToManagePage').and.callThrough();

        mockTokenService = {
            setToken: function () { }
        };
        setTokenPromise = $q.defer();
        spyOn(mockTokenService, 'setToken').and.callFake(function () { return setTokenPromise.promise; });


        $controller('UnregisteredDashboardController', {
            $scope: scope,
            PollService: mockPollService,
            RoutingService: mockRoutingService,
            TokenService: mockTokenService
        });
    }));

    describe('Create New Poll', function () {

        it('Calls poll service to create new poll', function () {
            var question = 'Where shall we go?';

            scope.createPoll(question);


            expect(mockPollService.createPoll).toHaveBeenCalled();
        });

        it('Given a successful creation of a poll, calls routing service to navigate to the manage page', function () {
            var question = 'Where shall we go?';

            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                }
            };
            var response = { data: data };
            createPollPromise.resolve(response);
            setTokenPromise.resolve();

            scope.createPoll(question);

            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalled();
        });

        it('Given a successful creation of a poll, calls routing service with new ManageId', function () {
            var manageGuid = '0F4F3122-B786-4F5A-B2F0-808A67B916FA';

            var question = 'Where shall we go?';

            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                },
                ManageId: manageGuid
            };
            var response = { data: data };
            createPollPromise.resolve(response);
            setTokenPromise.resolve();

            scope.createPoll(question);


            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalledWith(manageGuid);
        });

        it('Given a successful creation of a poll, calls token service to set the token', function () {
            var question = 'Where shall we go?';
            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                }
            };
            var response = { data: data };
            createPollPromise.resolve(response);

            scope.createPoll(question);


            scope.$apply();
            expect(mockTokenService.setToken).toHaveBeenCalled();
        });

        it('Given a successful creation of a poll, calls token service with the new creator token', function () {
            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                }
            };
            var response = { data: data };
            createPollPromise.resolve(response);

            var question = 'Where shall we go?';

            scope.createPoll(question);

            scope.$apply();
            expect(mockTokenService.setToken).toHaveBeenCalledWith(newPollUUID, newTokenGuid);
        });

        it('Given a successful creation of a poll, calls routing service only once token service has finished', function () {
            var question = 'Where shall we go?';

            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                }
            };
            var response = { data: data };
            createPollPromise.resolve(response);

            scope.createPoll(question);


            // Should not be called until the promise is resolved.
            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).not.toHaveBeenCalled();

            setTokenPromise.resolve();
            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalled();
        });
    });

});