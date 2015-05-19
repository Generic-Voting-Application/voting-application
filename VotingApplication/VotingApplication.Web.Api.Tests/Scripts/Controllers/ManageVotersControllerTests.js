'use strict';

describe('ManageVotersController', function () {

    beforeEach(module('GVA.Creation'));

    var scope;
    var manageServiceMock;
    var manageGetVotersPromise;
    var manageDeleteVotersPromise;


    var routingServiceMock;


    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();
        scope.voters = [];
        scope.votersToRemove = [];

        manageServiceMock = {
            getVoters: function () { },
            deleteVoters: function () { }
        };
        manageGetVotersPromise = $q.defer();
        manageDeleteVotersPromise = $q.defer();
        spyOn(manageServiceMock, 'getVoters').and.callFake(function () { return manageGetVotersPromise.promise; });
        spyOn(manageServiceMock, 'deleteVoters').and.callFake(function () { return manageDeleteVotersPromise.promise; });

        routingServiceMock = { navigateToManagePage: function () { } };
        spyOn(routingServiceMock, 'navigateToManagePage');


        $controller('ManageVotersController', { $scope: scope, ManageService: manageServiceMock, RoutingService: routingServiceMock });
    }));

    it('Loads Voters from service', function () {
        var getVotersResponse = [
            {
                BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                VoterName: 'Derek',
                Votes: [
                    {
                        ChoiceNumber: 1,
                        ChoiceName: 'One',
                        Value: 5
                    },
                    {
                        ChoiceNumber: 2,
                        ChoiceName: 'Two',
                        Value: 1
                    }
                ]

            }
        ];

        manageGetVotersPromise.resolve(getVotersResponse);
        scope.$apply();

        expect(scope.voters).toBe(getVotersResponse);
    });

    it('Voters To Remove is empty on load', function () {
        expect(scope.votersToRemove).toEqual([]);
    });

    describe('Remove All Votes', function () {

        it('Adds all ballots and votes to be removed to VotersToRemove', function () {
            var voters = [
                {
                    BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                    VoterName: 'Derek',
                    Votes: [
                        {
                            ChoiceNumber: 1,
                            ChoiceName: 'One',
                            Value: 5
                        },
                        {
                            ChoiceNumber: 2,
                            ChoiceName: 'Two',
                            Value: 1
                        }
                    ]

                },
                {
                    BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                    VoterName: 'Barbara',
                    Votes: [
                        {
                            ChoiceNumber: 3,
                            ChoiceName: 'Three',
                            Value: 2
                        },
                        {
                            ChoiceNumber: 7,
                            ChoiceName: 'Seven',
                            Value: 0
                        }
                    ]

                }
            ];
            scope.voters = voters;
            var expectedVotersToRemove = voters.slice(0);


            scope.removeAllVotes();


            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });

        it('Removes all ballots and votes from Voters', function () {
            scope.voters = [
                {
                    BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                    VoterName: 'Derek',
                    Votes: [
                        {
                            ChoiceNumber: 1,
                            ChoiceName: 'One',
                            Value: 5
                        },
                        {
                            ChoiceNumber: 2,
                            ChoiceName: 'Two',
                            Value: 1
                        }
                    ]

                },
                {
                    BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                    VoterName: 'Barbara',
                    Votes: [
                        {
                            ChoiceNumber: 3,
                            ChoiceName: 'Three',
                            Value: 2
                        },
                        {
                            ChoiceNumber: 7,
                            ChoiceName: 'Seven',
                            Value: 0
                        }
                    ]

                }
            ];

            scope.removeAllVotes();

            expect(scope.voters).toEqual([]);
        });

        it('Given existing ballots and votes to remove the remaining ballots and votes are added to VotersToRemove', function () {
            var voters = [
                {
                    BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                    VoterName: 'Derek',
                    Votes: [
                        {
                            ChoiceNumber: 1,
                            ChoiceName: 'One',
                            Value: 5
                        },
                        {
                            ChoiceNumber: 2,
                            ChoiceName: 'Two',
                            Value: 1
                        }
                    ]

                },
                {
                    BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                    VoterName: 'Barbara',
                    Votes: [
                        {
                            ChoiceNumber: 3,
                            ChoiceName: 'Three',
                            Value: 2
                        },
                        {
                            ChoiceNumber: 7,
                            ChoiceName: 'Seven',
                            Value: 0
                        }
                    ]

                }
            ];

            var existingVotersToRemove = [
                {
                    BallotManageGuid: '7E763711-412A-4B91-859D-E598DE58FCF2',
                    VoterName: 'Roger',
                    Votes: [
                        {
                            ChoiceNumber: 3,
                            ChoiceName: 'Three',
                            Value: 3
                        }
                    ]
                }
            ];

            var expectedVotersToRemove = existingVotersToRemove.concat(voters);

            scope.voters = voters;
            scope.votersToRemove = existingVotersToRemove;

            scope.removeAllVotes();

            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });

        it('Given existing ballots and votes to remove, it does not duplicate ballots and votes to remove', function () {
            var voter1 = {
                BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                VoterName: 'Derek',
                Votes: [
                    {
                        ChoiceNumber: 1,
                        ChoiceName: 'One',
                        Value: 5
                    },
                    {
                        ChoiceNumber: 2,
                        ChoiceName: 'Two',
                        Value: 1
                    }
                ]

            };
            var voter2 = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [
                    {
                        ChoiceNumber: 3,
                        ChoiceName: 'Three',
                        Value: 2
                    },
                    {
                        ChoiceNumber: 7,
                        ChoiceName: 'Seven',
                        Value: 0
                    }
                ]

            };

            var voters = [voter1, voter2];
            var existingVotersToRemove = [voter1];

            scope.voters = voters;
            scope.votersToRemove = existingVotersToRemove;

            var expectedVotersToRemove = voters.slice(0);


            scope.removeAllVotes();


            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });

        it('Given a ballot partially added for removal the remaining votes are added to VotersToRemove', function () {
            var voterManageGuid = 'D0F070A6-596A-4350-A3B3-ED542525D871';
            var voterName = 'Barbara';
            var voterVote1 = {
                ChoiceNumber: 3,
                ChoiceName: 'Three',
                Value: 2
            };
            var voterVote2 = {
                ChoiceNumber: 7,
                ChoiceName: 'Seven',
                Value: 0
            };

            var voters = [
                {
                    BallotManageGuid: voterManageGuid,
                    VoterName: voterName,
                    Votes: [voterVote2]

                }
            ];

            var existingVotersToRemove = [
                {
                    BallotManageGuid: voterManageGuid,
                    VoterName: voterName,
                    Votes: [voterVote1]
                }
            ];

            var expectedVotersToRemove = [
                {
                    BallotManageGuid: voterManageGuid,
                    VoterName: voterName,
                    Votes: [voterVote1, voterVote2]
                }
            ];

            scope.voters = voters;
            scope.votersToRemove = existingVotersToRemove;


            scope.removeAllVotes();


            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });
    });

    describe('Remove Ballot', function () {

        it('Adds ballot and votes to VotersToRemove', function () {
            var ballotToRemove = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [
                {
                    ChoiceNumber: 3,
                    ChoiceName: 'Three',
                    Value: 2
                },
                {
                    ChoiceNumber: 7,
                    ChoiceName: 'Seven',
                    Value: 0
                }]
            };
            var expectedVotersToRemove = [ballotToRemove];

            scope.voters = [ballotToRemove];
            scope.votersToRemove = [];


            scope.removeBallot(ballotToRemove);


            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });

        it('Removes ballot and votes from Voters', function () {
            var ballotToRemove = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [
                {
                    ChoiceNumber: 3,
                    ChoiceName: 'Three',
                    Value: 2
                },
                {
                    ChoiceNumber: 7,
                    ChoiceName: 'Seven',
                    Value: 0
                }]
            };
            var voters = [ballotToRemove];

            scope.voters = voters;
            scope.votersToRemove = [];


            scope.removeBallot(ballotToRemove);


            expect(scope.voters).toEqual([]);
        });

        it('Does not affect other ballots in Voters', function () {
            var ballotToRemove = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [
                {
                    ChoiceNumber: 3,
                    ChoiceName: 'Three',
                    Value: 2
                },
                {
                    ChoiceNumber: 7,
                    ChoiceName: 'Seven',
                    Value: 0
                }]
            };
            var unaffectedBallot = {
                BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                VoterName: 'Derek',
                Votes: [
                {
                    ChoiceNumber: 1,
                    ChoiceName: 'One',
                    Value: 5
                },
                {
                    ChoiceNumber: 2,
                    ChoiceName: 'Two',
                    Value: 1
                }]
            };
            var voters = [ballotToRemove, unaffectedBallot];

            scope.voters = voters;
            scope.votersToRemove = [];


            scope.removeBallot(ballotToRemove);


            expect(scope.voters).toEqual([unaffectedBallot]);
        });

        it('Given votes added for removal, removing the ballot does not duplicate votes', function () {
            var ballotToRemove = {
                BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                VoterName: 'Derek',
                Votes: [
                {
                    ChoiceNumber: 12,
                    ChoiceName: 'Twelve',
                    Value: 12
                }]
            };
            var votesAlreadyAddedForRemoval = {
                BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                VoterName: 'Derek',
                Votes: [
                {
                    ChoiceNumber: 1,
                    ChoiceName: 'One',
                    Value: 5
                },
                {
                    ChoiceNumber: 2,
                    ChoiceName: 'Two',
                    Value: 1
                }]
            };
            var voters = [ballotToRemove];

            var expectedVotersToRemove = [
                {
                    BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                    VoterName: 'Derek',
                    Votes: [
                        {
                            ChoiceNumber: 1,
                            ChoiceName: 'One',
                            Value: 5
                        },
                        {
                            ChoiceNumber: 2,
                            ChoiceName: 'Two',
                            Value: 1
                        },
                        {
                            ChoiceNumber: 12,
                            ChoiceName: 'Twelve',
                            Value: 12
                        }
                    ]
                }
            ];

            scope.voters = voters;
            scope.votersToRemove = [votesAlreadyAddedForRemoval];


            scope.removeBallot(ballotToRemove);


            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });
    });

    describe('Remove Vote', function () {

        it('Given no votes to remove, adds ballot and vote to VotersToRemove', function () {
            var voteToRemove = {
                ChoiceNumber: 3,
                ChoiceName: 'Three',
                Value: 2
            };
            var ballot = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [voteToRemove]
            };

            var expectedVotersToRemove = [{
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [voteToRemove]
            }];

            scope.voters = [ballot];
            scope.votersToRemove = [];


            scope.removeVote(voteToRemove, ballot);


            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });

        it('Removes vote from Voters', function () {
            var voteToRemove = {
                ChoiceNumber: 3,
                ChoiceName: 'Three',
                Value: 2
            };
            var voteToRemain = {
                ChoiceNumber: 2,
                ChoiceName: 'Two',
                Value: 1
            };

            var ballot = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [voteToRemove, voteToRemain]
            };
            var expectedVotersToRemove = [
            {

                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [voteToRemain]

            }];

            scope.voters = [ballot];
            scope.votersToRemove = [];


            scope.removeVote(voteToRemove, ballot);


            expect(scope.voters).toEqual(expectedVotersToRemove);
        });

        it('Removes ballot from Voters if it is the last vote in the ballot', function () {
            var vote = {
                ChoiceNumber: 3,
                ChoiceName: 'Three',
                Value: 2
            };

            var ballot = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [vote]
            };

            scope.voters = [ballot];
            scope.votersToRemove = [];


            scope.removeVote(vote, ballot);


            expect(scope.voters).toEqual([]);
        });

        it('Adds vote to ballot in VotersToRemove', function () {
            var voteToRemove = {
                ChoiceNumber: 3,
                ChoiceName: 'Three',
                Value: 2
            };
            var ballot = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [
                    voteToRemove,
                    {
                        ChoiceNumber: 56,
                        ChoiceName: 'Fifty-six',
                        Value: 23
                    }]
            };

            var existingBallot = {
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [
                {
                    ChoiceNumber: 1,
                    ChoiceName: 'One',
                    Value: 5
                }]
            };

            var expectedVotersToRemove = [{
                BallotManageGuid: 'D0F070A6-596A-4350-A3B3-ED542525D871',
                VoterName: 'Barbara',
                Votes: [
                    {
                        ChoiceNumber: 1,
                        ChoiceName: 'One',
                        Value: 5
                    },
                voteToRemove]
            }];

            scope.voters = [ballot];
            scope.votersToRemove = [existingBallot];

            scope.removeVote(voteToRemove, ballot);


            expect(scope.votersToRemove).toEqual(expectedVotersToRemove);
        });
    });

    describe('Return Without Delete', function () {

        it('Does not make service call to delete voters', function () {
            scope.returnWithoutDelete();

            expect(manageServiceMock.deleteVoters.calls.any()).toEqual(false);
        });

        it('Makes service call to navigateToManagePage', function () {
            scope.returnWithoutDelete();

            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });
    });

    describe('ConfirmDeleteAndReturn', function () {

        it('Makes service call to delete voters', function () {
            scope.confirmDeleteAndReturn();

            expect(manageServiceMock.deleteVoters.calls.any()).toEqual(true);
        });

        it('Makes service call to Navigate To Manage Page when delete service call succeeds', function () {
            manageDeleteVotersPromise.resolve();


            scope.confirmDeleteAndReturn();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(true);
        });

        it('Does not make service call to Navigate To Manage Page if delete service call fails', function () {
            manageDeleteVotersPromise.reject();


            scope.confirmDeleteAndReturn();


            scope.$apply();
            expect(routingServiceMock.navigateToManagePage.calls.any()).toEqual(false);
        });
    });
});