'use strict';

describe('My Polls Controller', function () {

    beforeEach(module('GVA.Manage'));


    var scope;

    var mockPollService;
    var mockRoutingService;
    var mockTokenService;
    var mockAccountService;

    var createPollPromise;
    var getUserPollsPromise;
    var copyPollPromise;
    var setTokenPromise;

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
            setManageId: function () { }
        };
        setTokenPromise = $q.defer();
        spyOn(mockTokenService, 'setToken').and.callFake(function () { return setTokenPromise.promise; });

        mockAccountService = {
            registerAccountObserver: function () { }
        };
        spyOn(mockAccountService, 'registerAccountObserver').and.callThrough();

        $controller('MyPollsController', {
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