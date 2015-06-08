'use strict';

describe('Registered Dashboard Controller', function () {

    beforeEach(module('GVA.Creation'));


    var scope;

    var mockPollService;
    var mockRoutingService;
    var mockTokenService;
    var mockAccountService;

    var createPollPromise;
    var getUserPollsPromise;
    var copyPollPromise;
    var setTokenPromise;
    var setManageIdPromise;

    var newPollUUID = '9C884B24-9B76-4FF8-A074-36AEB1EC3920';
    var newTokenGuid = 'B14A90C0-078A-4996-B0FA-EA09A0EB6FFE';

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();

        mockPollService = {
            createPoll: function () { },
            getUserPolls: function () { },
            copyPoll: function () { }
        };
        createPollPromise = $q.defer();
        getUserPollsPromise = $q.defer();
        copyPollPromise = $q.defer();
        spyOn(mockPollService, 'createPoll').and.callFake(function () { return createPollPromise.promise; });
        spyOn(mockPollService, 'getUserPolls').and.callFake(function () { return getUserPollsPromise.promise; });
        spyOn(mockPollService, 'copyPoll').and.callFake(function () { return copyPollPromise.promise; });

        mockRoutingService = {
            navigateToManagePage: function () { }
        };
        spyOn(mockRoutingService, 'navigateToManagePage').and.callThrough();

        mockTokenService = {
            setToken: function () { },
            setManageId: function() { }
        };
        setTokenPromise = $q.defer();
        setManageIdPromise = $q.defer();
        spyOn(mockTokenService, 'setToken').and.callFake(function () { return setTokenPromise.promise; });
        spyOn(mockTokenService, 'setManageId').and.callFake(function () { return setManageIdPromise.promise; });

        mockAccountService = {
            registerAccountObserver: function () { }
        };
        spyOn(mockAccountService, 'registerAccountObserver').and.callThrough();

        $controller('RegisteredDashboardController', {
            $scope: scope,
            PollService: mockPollService,
            RoutingService: mockRoutingService,
            TokenService: mockTokenService,
            AccountService: mockAccountService
        });
    }));

    it('Loads user polls', function () {
        var userPollData = [
            {
                Name: 'Poll1',
                Creator: 'Some dude',
                CreatedDate: new Date(2015, 4, 29)
            },
            {
                Name: 'Poll2',
                Creator: 'Some dude',
                CreatedDate: new Date(2015, 4, 30)
            }
        ];
        var response = { data: userPollData };

        expect(scope.userPolls).toEqual({});

        getUserPollsPromise.resolve(response);
        scope.$apply();
        expect(scope.userPolls).toEqual(userPollData);
    });

    describe('Create Poll', function () {

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

    describe('Copy Poll', function () {

        it('Calls poll service', function () {
            var pollId = '07A2FD4E-71CC-42AF-84DC-504398289FC6';

            scope.copyPoll(pollId);

            expect(mockPollService.copyPoll).toHaveBeenCalled();
        });

        it('Given a successful copy of a poll, calls routing service to navigate to the manage page', function () {
            var pollId = '07A2FD4E-71CC-42AF-84DC-504398289FC6';

            var copiedPollData = { data: { NewManageId: 'E6DF016D-F10D-4E1C-9DB8-68D4073CD5F4' } };

            copyPollPromise.resolve(copiedPollData);
            setTokenPromise.resolve();

            scope.copyPoll(pollId);


            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalled();
        });

        it('Given a successful copy of a poll, calls routing service with the new manage id', function () {
            var pollId = '07A2FD4E-71CC-42AF-84DC-504398289FC6';
            var manageId = 'E6DF016D-F10D-4E1C-9DB8-68D4073CD5F4';
            var copiedPollData = { data: { NewManageId: manageId } };

            copyPollPromise.resolve(copiedPollData);
            setTokenPromise.resolve();

            scope.copyPoll(pollId);


            scope.$apply();
            expect(mockRoutingService.navigateToManagePage).toHaveBeenCalledWith(manageId);
        });

        it('Given a successful copy of a poll, calls token service to save the returned ballot token', function () {
            var pollId = '07A2FD4E-71CC-42AF-84DC-504398289FC6';
            var manageId = 'E6DF016D-F10D-4E1C-9DB8-68D4073CD5F4';

            var newPollId = '96D5AEAB-B409-4279-8097-6D3FAAB7DD43';
            var creatorBallotToken = 'F6118B77-C037-4D19-96F1-A91E84C42990';
            var copiedPollData = {
                data: {
                    NewPollId: newPollId,
                    NewManageId: manageId,
                    CreatorBallotToken: creatorBallotToken
                }
            };

            copyPollPromise.resolve(copiedPollData);
            setTokenPromise.resolve();

            scope.copyPoll(pollId);


            scope.$apply();
            expect(mockTokenService.setToken).toHaveBeenCalledWith(newPollId, creatorBallotToken);
        });

        it('Given a successful copy of a poll, calls token service with the returned ballot token', function () {
            var pollId = '07A2FD4E-71CC-42AF-84DC-504398289FC6';
            var manageId = 'E6DF016D-F10D-4E1C-9DB8-68D4073CD5F4';
            var creatorBallotToken = 'F6118B77-C037-4D19-96F1-A91E84C42990';
            var copiedPollData = {
                data: {
                    NewManageId: manageId,
                    CreatorBallotToken: creatorBallotToken
                }
            };

            copyPollPromise.resolve(copiedPollData);

            scope.copyPoll(pollId);


            scope.$apply();
            expect(mockTokenService.setToken).toHaveBeenCalled();
        });
    });

    describe('Get User Polls', function () {

        it('Calls poll service', function () {

            scope.getUserPolls();

            expect(mockPollService.getUserPolls).toHaveBeenCalled();
        });

        it('Given a successful retrieval of a user\'s poll, sets userPolls', function () {

            var userPollData = [
            {
                Name: 'Poll1',
                Creator: 'Some dude',
                CreatedDate: new Date(2015, 4, 29)
            },
            {
                Name: 'Poll2',
                Creator: 'Some dude',
                CreatedDate: new Date(2015, 4, 30)
            }
            ];

            var response = { data: userPollData };

            getUserPollsPromise.resolve(response);

            scope.getUserPolls();


            scope.$apply();
            expect(scope.userPolls).toEqual(userPollData);
        });
    });

});