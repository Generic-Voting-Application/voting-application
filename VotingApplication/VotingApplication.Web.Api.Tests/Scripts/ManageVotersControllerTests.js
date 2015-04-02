describe("ManageVotersController Tests", function () {

    beforeEach(module("GVA.Creation"));

    var scope;
    var manageServiceMock;

    var manageGetVotersPromise;

    beforeEach(inject(function ($rootScope, $q, $controller) {

        scope = $rootScope.$new();
        scope.voters = [];
        scope.votersToRemove = [];
        manageServiceMock = {
            getVoters: function () { }
        };


        manageGetVotersPromise = $q.defer();

        spyOn(manageServiceMock, 'getVoters').and.callFake(function () { return manageGetVotersPromise.promise; });

        $controller("ManageVotersController", { $scope: scope, ManageService: manageServiceMock });
    }));

    it("Loads Voters from service", function () {
        var getVotersResponse = [
            {
                BallotManageGuid: "275B1FF3-F37A-41F9-B91E-983F6D11429A",
                VoterName: "Derek",
                Votes: [
                    {
                        OptionNumber: 1,
                        OptionName: "One",
                        Value: 5
                    },
                    {
                        OptionNumber: 2,
                        OptionName: "Two",
                        Value: 1
                    }
                ]

            }
        ];

        manageGetVotersPromise.resolve(getVotersResponse);
        scope.$apply();

        expect(scope.voters).toBe(getVotersResponse);
    });

    it("Voters To Remove is empty on load", function () {
        expect(scope.votersToRemove).toEqual([]);
    });


    describe("Remove All Votes Tests", function () {
        it("Remove All Votes adds all ballots and votes to be removed to VotersToRemove", function () {
            var voters = [
                {
                    BallotManageGuid: "275B1FF3-F37A-41F9-B91E-983F6D11429A",
                    VoterName: "Derek",
                    Votes: [
                        {
                            OptionNumber: 1,
                            OptionName: "One",
                            Value: 5
                        },
                        {
                            OptionNumber: 2,
                            OptionName: "Two",
                            Value: 1
                        }
                    ]

                },
                {
                    BallotManageGuid: "D0F070A6-596A-4350-A3B3-ED542525D871",
                    VoterName: "Barbara",
                    Votes: [
                        {
                            OptionNumber: 3,
                            OptionName: "Three",
                            Value: 2
                        },
                        {
                            OptionNumber: 7,
                            OptionName: "Seven",
                            Value: 0
                        }
                    ]

                }
            ];
            scope.voters = voters;

            scope.removeAllVotes();

            expect(scope.votersToRemove).toEqual(voters);
        });

        it("Remove All Votes removes all ballots and votes from Voters", function () {
            scope.voters = [
                {
                    BallotManageGuid: "275B1FF3-F37A-41F9-B91E-983F6D11429A",
                    VoterName: "Derek",
                    Votes: [
                        {
                            OptionNumber: 1,
                            OptionName: "One",
                            Value: 5
                        },
                        {
                            OptionNumber: 2,
                            OptionName: "Two",
                            Value: 1
                        }
                    ]

                },
                {
                    BallotManageGuid: "D0F070A6-596A-4350-A3B3-ED542525D871",
                    VoterName: "Barbara",
                    Votes: [
                        {
                            OptionNumber: 3,
                            OptionName: "Three",
                            Value: 2
                        },
                        {
                            OptionNumber: 7,
                            OptionName: "Seven",
                            Value: 0
                        }
                    ]

                }
            ];

            scope.removeAllVotes();

            expect(scope.voters).toEqual([]);
        });

        it("Remove All Votes with existing ballots and votes to remove, adds remaining ballots and votes", function () {
            var voters = [
                {
                    BallotManageGuid: "275B1FF3-F37A-41F9-B91E-983F6D11429A",
                    VoterName: "Derek",
                    Votes: [
                        {
                            OptionNumber: 1,
                            OptionName: "One",
                            Value: 5
                        },
                        {
                            OptionNumber: 2,
                            OptionName: "Two",
                            Value: 1
                        }
                    ]

                },
                {
                    BallotManageGuid: "D0F070A6-596A-4350-A3B3-ED542525D871",
                    VoterName: "Barbara",
                    Votes: [
                        {
                            OptionNumber: 3,
                            OptionName: "Three",
                            Value: 2
                        },
                        {
                            OptionNumber: 7,
                            OptionName: "Seven",
                            Value: 0
                        }
                    ]

                }
            ];

            var existingVotersToRemove = [
                {
                    BallotManageGuid: "7E763711-412A-4B91-859D-E598DE58FCF2",
                    VoterName: "Roger",
                    Votes: [
                        {
                            OptionNumber: 3,
                            OptionName: "Three",
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

        it("Remove All Votes with existing ballots and votes to remove does not duplicate ballots and votes to remove", function () {
            var voter1 = {
                BallotManageGuid: "275B1FF3-F37A-41F9-B91E-983F6D11429A",
                VoterName: "Derek",
                Votes: [
                    {
                        OptionNumber: 1,
                        OptionName: "One",
                        Value: 5
                    },
                    {
                        OptionNumber: 2,
                        OptionName: "Two",
                        Value: 1
                    }
                ]

            };
            var voter2 = {
                BallotManageGuid: "D0F070A6-596A-4350-A3B3-ED542525D871",
                VoterName: "Barbara",
                Votes: [
                    {
                        OptionNumber: 3,
                        OptionName: "Three",
                        Value: 2
                    },
                    {
                        OptionNumber: 7,
                        OptionName: "Seven",
                        Value: 0
                    }
                ]

            };

            var voters = [voter1, voter2];
            var existingVotersToRemove = [voter1];

            scope.voters = voters;
            scope.votersToRemove = existingVotersToRemove;

            scope.removeAllVotes();

            expect(scope.votersToRemove).toEqual(voters);
        });

        it("Remove All Votes with a ballot partially added for removal, adds remaining votes to VotersToRemove", function () {
            var voterManageGuid = "D0F070A6-596A-4350-A3B3-ED542525D871";
            var voterName = "Barbara";
            var voterVote1 = {
                OptionNumber: 3,
                OptionName: "Three",
                Value: 2
            };
            var voterVote2 = {
                OptionNumber: 7,
                OptionName: "Seven",
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
});