'use strict';

describe('ManageService', function () {

    beforeEach(module('GVA.Manage'));
    beforeEach(module('GVA.Common'));

    var manageService;
    var httpBackend;

    beforeEach(inject(function (ManageService, $httpBackend) {
        manageService = ManageService;
        httpBackend = $httpBackend;
    }));

    afterEach(function () {
        httpBackend.verifyNoOutstandingExpectation();
        httpBackend.verifyNoOutstandingRequest();

        // http://stackoverflow.com/questions/27016235/angular-js-unit-testing-httpbackend-spec-has-no-expectations
        expect('Suppress SPEC HAS NO EXPECTATIONS').toBeDefined();
    });

    describe('Delete Voters', function () {

        it('Contains the correct Content-Type header', function () {

            var pollManageGuid = '091EA404-5AAE-4AA4-A5EF-E0625ACC33D4';
            var expectedUrl = '/api/manage/' + pollManageGuid + '/voters';
            var expectedData = { 'BallotDeleteRequests': [] };

            httpBackend.expect(
                    'DELETE',
                    expectedUrl,
                    expectedData,
                    function (headers) {
                        return headers['Content-Type'] === 'application/json; charset=utf-8';
                    }
                ).respond(200);

            manageService.deleteVoters(pollManageGuid, []);

            httpBackend.flush();
        });

        it('Creates correct DeleteBallotRequestModel', function () {

            var pollManageGuid = '091EA404-5AAE-4AA4-A5EF-E0625ACC33D4';
            var expectedUrl = '/api/manage/' + pollManageGuid + '/voters';

            var votersToDelete = [{
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
            },
                {
                    BallotManageGuid: '4F63A474-1136-4E8D-879C-881299A19207',
                    VoterName: 'Betty',
                    Votes: [
                        {
                            ChoiceNumber: 1,
                            ChoiceName: 'One',
                            Value: 1
                        },
                        {
                            ChoiceNumber: 3,
                            ChoiceName: 'Three',
                            Value: 1
                        }]
                }];

            var expectedRequestBody = {
                BallotDeleteRequests: [
                    {
                        BallotManageGuid: '275B1FF3-F37A-41F9-B91E-983F6D11429A',
                        VoteDeleteRequests: [{ ChoiceNumber: 1 }, { ChoiceNumber: 2 }]
                    },
                    {
                        BallotManageGuid: '4F63A474-1136-4E8D-879C-881299A19207',
                        VoteDeleteRequests: [{ ChoiceNumber: 1 }, { ChoiceNumber: 3 }]
                    }
                ]
            };


            httpBackend
                .expect(
                    'DELETE',
                    expectedUrl,
                    expectedRequestBody,
                    function (headers) {
                        return headers['Content-Type'] === 'application/json; charset=utf-8';
                    })
                .respond(200);

            manageService.deleteVoters(pollManageGuid, votersToDelete);

            httpBackend.flush();
        });
    });
});