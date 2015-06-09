'use strict';

describe('Create page Controller', function () {

    beforeEach(module('GVA.Manage'));


    var scope;

    var mockPollService;
    var mockRoutingService;
    var mockTokenService;

    var createPollPromise;
    var setTokenPromise;
    var setManageIdPromise;

    var newPollUUID = '9C884B24-9B76-4FF8-A074-36AEB1EC3920';
    var newTokenGuid = 'B14A90C0-078A-4996-B0FA-EA09A0EB6FFE';
    var newManageGuid = '7385e700-365f-4bd5-a680-36784fff0c95';

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        mockPollService = {
            createPoll: function () { }
        };

        createPollPromise = $q.defer();
        spyOn(mockPollService, 'createPoll').and.callFake(function () { return createPollPromise.promise; });

        mockRoutingService = {
            navigateToManagePage: function () { },
            navigateToVotePage : function() { }
        };
        spyOn(mockRoutingService, 'navigateToManagePage').and.callThrough();
        spyOn(mockRoutingService, 'navigateToVotePage').and.callThrough();

        mockTokenService = {
            setToken: function () { },
            setManageId : function() {}
        };
        setTokenPromise = $q.defer();
        spyOn(mockTokenService, 'setToken').and.callFake(function () { return setTokenPromise.promise; });

        setManageIdPromise = $q.defer();
        spyOn(mockTokenService, 'setManageId').and.callFake(function () { return setManageIdPromise.promise; });


        $controller('CreatePageController', {
            $scope: scope,
            PollService: mockPollService,
            RoutingService: mockRoutingService,
            TokenService: mockTokenService
        });
    }));

    describe('Create New Custom Poll', function () {

        it('Calls poll service to create new poll', function () {
            var question = 'Where shall we go?';

            scope.createCustomPoll(question);


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

            scope.createCustomPoll(question);

            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalled();
        });

        it('Given a successful creation of a poll, calls routing service with new ManageId', function () {
            var question = 'Where shall we go?';

            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                },
                ManageId: newManageGuid
            };
            var response = { data: data };
            createPollPromise.resolve(response);
            setTokenPromise.resolve();

            scope.createCustomPoll(question);


            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalledWith(newManageGuid);
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

            scope.createCustomPoll(question);


            scope.$apply();
            expect(mockTokenService.setToken).toHaveBeenCalled();
        });

        it('Given a successful creation of a poll, calls token service with the new creator token', function () {
            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                },
                ManageId: newManageGuid
            };
            var response = { data: data };
            createPollPromise.resolve(response);

            var question = 'Where shall we go?';

            scope.createCustomPoll(question);

            scope.$apply();
            expect(mockTokenService.setToken).toHaveBeenCalledWith(newPollUUID, newTokenGuid);
            expect(mockTokenService.setManageId).toHaveBeenCalledWith(newPollUUID, newManageGuid);
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

            scope.createCustomPoll(question);


            // Should not be called until the promise is resolved.
            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).not.toHaveBeenCalled();

            setTokenPromise.resolve();
            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalled();
        });
    });

    describe('Create New Quick Poll', function () {

        it('Calls poll service to create new poll', function () {
            var question = 'Where shall we go?';
            var choices = [{ Name: 'Choice 1' }];

            scope.createQuickPoll(question, choices);


            expect(mockPollService.createPoll).toHaveBeenCalled();
        });

        it('Given a successful creation of a poll, calls routing service to navigate to the poll page', function () {
            var question = 'Where shall we go?';
            var choices = [{ Name: 'Choice 1' }];

            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                }
            };
            var response = { data: data };
            createPollPromise.resolve(response);
            setTokenPromise.resolve();
            setManageIdPromise.resolve();

            scope.createQuickPoll(question, choices);

            scope.$apply();
            expect(mockRoutingService.navigateToVotePage).toHaveBeenCalled();
        });

        it('Given a successful creation of a poll, calls routing service with new UUID', function () {
            var question = 'Where shall we go?';
            var choices = [{ Name: 'Choice 1' }];

            var data = {
                UUID: newPollUUID,
                CreatorBallot: {
                    TokenGuid: newTokenGuid
                },
                ManageId: newManageGuid
            };
            var response = { data: data };
            createPollPromise.resolve(response);
            setTokenPromise.resolve();
            setManageIdPromise.resolve();

            scope.createQuickPoll(question, choices);


            scope.$apply();
            expect(mockRoutingService.navigateToVotePage).toHaveBeenCalledWith(newPollUUID);
        });
    });

});