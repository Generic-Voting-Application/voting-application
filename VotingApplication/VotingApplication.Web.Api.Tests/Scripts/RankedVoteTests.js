define(["RankedVote", "Common", "mockjax", "knockout"], function (RankedVote, Common, mockjax, ko) {
    describe("RankedVote", function () {

        //#region Setup
        var target;
        beforeEach(function () {
            // Setup test target with pollId and token
            target = new RankedVote("303", "515");

            // Setup mockjax
            $.mockjaxSettings.responseTime = 1;
            mockjax.clear();

            // Spy on Common.currentUserId (only return user id for correct poll id)
            spyOn(Common, 'currentUserId').and.callFake(function (pollId) {
                if (pollId === "303") return 912;
                return 0;
            });

            // Spy on Common.sessionItem (only return token for correct key and poll id)
            spyOn(Common, 'sessionItem').and.callFake(function (sessionKey, pollId) {
                if (sessionKey === "token" && pollId === "303") return "616";
                return 0;
            })
        });
        //#endregion
        
        it("doVote with Token expect Post vote and notify", function (done) {
            // arrange

            var expectedVotes = [
                { OptionId: 17, VoteValue: 1, TokenGuid: "515" },
                { OptionId: 25, VoteValue: 2, TokenGuid: "515" }
            ];

            var posted = false;
            mockjax({
                type: "PUT", url: "/api/user/912/poll/303/vote",
                data: JSON.stringify(expectedVotes),
                response: function () { posted = true; }, responseText: {}
            });

            target.selectedOptions([{ Id: 17, Name: "Option-2" }, { Id: 25, Name: "Option-4" }]);

            spyOn(target, 'onVoted');

            // act
            target.doVote();

            // assert
            setTimeout(function () {
                expect(posted).toBe(true);
                expect(target.onVoted).toHaveBeenCalled();
                done();
            }, 10);
        });

        it("doVote without Token expect Get Token from Session and Post", function (done) {
            // arrange
            target = new RankedVote("303");

            var expectedVotes = [ { OptionId: 17, VoteValue: 1, TokenGuid: "616" } ];

            var posted = false;
            mockjax({
                type: "PUT", url: "/api/user/912/poll/303/vote",
                data: JSON.stringify(expectedVotes),
                response: function () { posted = true; }, responseText: {}
            });

            target.selectedOptions([{ Id: 17, Name: "Option-2" }]);

            spyOn(target, 'onVoted');

            // act
            target.doVote();

            // assert
            setTimeout(function () {
                expect(posted).toBe(true);
                expect(target.onVoted).toHaveBeenCalled();
                done();
            }, 10);
        });

        it("getVotes without Voted expect Clear selected", function (done) {
            // arrange
            target.pollOptions.options([{ Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }]);
            target.selectedOptions([{ Id: 13, Name: "Option-1" }, { Id: 21, Name: "Option-3" }]);
            target.selectedOptions([{ Id: 17, Name: "Option-2" }]);

            mockjax({
                type: "GET", url: "/api/user/912/poll/303/vote",
                responseText: []
            });

            // act
            target.getVotes("303", 912);

            // assert
            setTimeout(function () {
                // Neither should be highlighted
                expect(target.selectedOptions().length).toEqual(0);
                expect(target.remainOptions()).toEqual([{ Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }]);
                done();
            }, 10);
        });

        it("getVotes with Voted expect Set selected", function (done) {
            // arrange
            target.pollOptions.options([{ Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }]);
            target.selectedOptions([{ Id: 13, Name: "Option-1" }, { Id: 21, Name: "Option-3" }]);
            target.selectedOptions([{ Id: 17, Name: "Option-2" }]);

            mockjax({
                type: "GET", url: "/api/user/912/poll/303/vote",
                responseText: [{ OptionId: 13 }, { OptionId: 21 }]
            });

            // act
            target.getVotes("303", 912);

            // assert
            setTimeout(function () {
                // Neither should be highlighted
                expect(target.selectedOptions()).toEqual([{ Id: 13, Name: "Option-1" }, { Id: 21, Name: "Option-3" }]);
                expect(target.remainOptions()).toEqual([{ Id: 17, Name: "Option-2" }]);
                done();
            }, 10);
        });

        it("displayResults without Votes expect Draw empty chart", function () {
            // act
            target.displayResults([]);

            // assert
            expect(target.chartData()).toEqual([]);
        });

        it("displayResults with Votes expect Draw ranked vote result", function () {
            // arrange
            target.pollOptions.options([
                    { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" },
                    { Id: 21, Name: "Option-3" }, { Id: 25, Name: "Option-4" }
            ]);

            // User-1 votes Option 1-2-3-4
            // User-2 votes Option 3-1
            // User-3 votes Option 3-4-2-1
            // User-4 votes Option 2-4-1
            // User-5 votes Option 2-3
            var data = [
                { OptionId: 13, UserId: 1, VoterName: "User-1" },
                { OptionId: 17, UserId: 1, VoterName: "User-1" },
                { OptionId: 21, UserId: 1, VoterName: "User-1" },
                { OptionId: 25, UserId: 1, VoterName: "User-1" },
                { OptionId: 21, UserId: 2, VoterName: "User-2" },
                { OptionId: 13, UserId: 2, VoterName: "User-2" },
                { OptionId: 21, UserId: 3, VoterName: "User-3" },
                { OptionId: 25, UserId: 3, VoterName: "User-3" },
                { OptionId: 17, UserId: 3, VoterName: "User-3" },
                { OptionId: 13, UserId: 3, VoterName: "User-3" },
                { OptionId: 17, UserId: 4, VoterName: "User-4" },
                { OptionId: 25, UserId: 4, VoterName: "User-4" },
                { OptionId: 13, UserId: 4, VoterName: "User-4" },
                { OptionId: 17, UserId: 5, VoterName: "User-5" },
                { OptionId: 21, UserId: 5, VoterName: "User-5" }
            ];

            // act
            target.displayResults(data);

            // assert

            // Round 1: Option-1 (1), Option-2 (2), Option-3 (2)
            // Round 2: Option-2 (3), Option-3 (2)

            var option1 = { Name: "Option-1", Data: [
                { Name: 'Round 1', Sum: 1, Voters: ['User-1 (#1)'] },
                { Name: 'Round 2', Sum: 0, Voters: [] }
            ]};
            var option2 = { Name: "Option-2", Data: [
                { Name: 'Round 1', Sum: 2, Voters: ['User-4 (#1)', 'User-5 (#1)'] },
                { Name: 'Round 2', Sum: 3, Voters: ['User-1 (#2)', 'User-4 (#1)', 'User-5 (#1)'] }
            ]};
            var option3 = { Name: "Option-3", Data: [
                { Name: 'Round 1', Sum: 2, Voters: ['User-2 (#1)', 'User-3 (#1)'] },
                { Name: 'Round 2', Sum: 2, Voters: ['User-2 (#1)', 'User-3 (#1)'] }
            ]};
            var option4 = { Name: "Option-4", Data: [
                { Name: 'Round 1', Sum: 0, Voters: [] },
                { Name: 'Round 2', Sum: 0, Voters: [] }
            ]};

            expect(target.chartData()).toEqual([option1, option2, option3, option4]);
        });
    });
});

