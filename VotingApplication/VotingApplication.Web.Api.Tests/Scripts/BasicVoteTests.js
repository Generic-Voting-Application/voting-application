define(["BasicVote", "Common", "mockjax", "knockout"], function (BasicVote, Common, mockjax, ko) {
    describe("BasicVote", function () {

        //#region Setup
        var target;
        beforeEach(function () {
            // Setup test target with pollId and token
            target = new BasicVote("303", "515");

            // Setup mockjax
            $.mockjaxSettings.responseTime = 1;
            mockjax.clear();

            // Spy on Common.getVoterName
            spyOn(Common, 'getVoterName').and.callFake(function () {
                return "Bob";
                return 0;
            });

            // Spy on Common.getToken
            spyOn(Common, 'getToken').and.callFake(function (pollId) {
                if (pollId === "303") return "00000000-0000-0000-0000-000000000001";
                return 0;
            });
        });
        //#endregion

        it("doVote with Token expect Post vote and notify", function (done) {
            // arrange
            var posted = false;
            mockjax({
                type: "PUT", url: "/api/token/00000000-0000-0000-0000-000000000001/poll/303/vote",
                data: JSON.stringify([{ OptionId: 17, VoterName: "Bob" }]),
                response: function () { posted = true; }, responseText: {}
            });

            spyOn(target, 'onVoted');

            // act
            target.doVote({ Id: 17, VoterName:"Bob" });

            // assert
            setTimeout(function () {
                expect(posted).toBe(true);
                expect(target.onVoted).toHaveBeenCalled();
                done();
            }, 10);
        });

        it("doVote without Token expect Get Token from Session and Post", function (done) {
            // arrange
            target = new BasicVote("303");

            var posted = false;
            mockjax({
                type: "PUT", url: "/api/token/00000000-0000-0000-0000-000000000001/poll/303/vote",
                data: JSON.stringify([{ OptionId: 17, VoterName: "Bob" }]),
                response: function () { posted = true; }, responseText: {}
            });

            spyOn(target, 'onVoted');

            // act
            target.doVote({ Id: 17 });

            // assert
            setTimeout(function () {
                expect(posted).toBe(true);
                expect(target.onVoted).toHaveBeenCalled();
                done();
            }, 10);
        });

        it("getVotes without Voted expect Clear highlighted", function (done) {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1", highlight: ko.observable(false) },
                { Id: 17, Name: "Option-2", highlight: ko.observable(true) }
            ]);
            mockjax({
                type: "GET", url: "/api/token/00000000-0000-0000-0000-000000000001/poll/303/vote",
                responseText: []
            });

            // act
            target.getPreviousVotes("303", 912);

            // assert
            setTimeout(function () {
                // Neither should be highlighted
                expect(target.pollOptions.options()[0].highlight()).toBe(false);
                expect(target.pollOptions.options()[1].highlight()).toBe(false);
                done();
            }, 10);
        });

        it("getVotes with Voted expect Set highlighted", function (done) {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1", highlight: ko.observable(false) },
                { Id: 17, Name: "Option-2", highlight: ko.observable(true) }
            ]);
            mockjax({
                type: "GET", url: "/api/token/00000000-0000-0000-0000-000000000001/poll/303/vote",
                responseText: [{ OptionId: 13 }]
            });

            // act
            target.getPreviousVotes("303", 912);

            // assert
            setTimeout(function () {
                // Neither should be highlighted
                expect(target.pollOptions.options()[0].highlight()).toBe(true);
                expect(target.pollOptions.options()[1].highlight()).toBe(false);
                done();
            }, 10);
        });

        it("displayResults without Votes expect Draw empty chart", function () {
            // act
            target.displayResults([]);

            // assert
            expect(target.chartData()).toEqual([{ Data: [] }]);
        });

        it("displayResults with Votes expect Draw grouped votes", function () {
            // arrange
            var data = [
                { OptionName: "One", VoterName: "User-1" },
                { OptionName: "Two", VoterName: "User-2" },
                { OptionName: "One", VoterName: "Anonymous User" }
            ];

            // act
            target.displayResults(data);

            // assert
            var expectedVotes = [{ Data: [
                { Name: 'One', Sum: 2, Voters: ['User-1', 'Anonymous User'] },
                { Name: 'Two', Sum: 1, Voters: ['User-2'] }
            ]}];
            expect(target.chartData()).toEqual(expectedVotes);
        });

        it("displayResults with Votes expect Announce Winner", function () {
            // arrange
            var data = [
                { OptionName: "One", VoterName: "User-1" },
                { OptionName: "Two", VoterName: "User-2" },
                { OptionName: "One", VoterName: "Anonymous User" }
            ];

            spyOn(target.pollOptions, 'getWinners').and.returnValue(["O", "T"]);

            // act
            target.displayResults(data);

            // assert
            expect(target.winners()).toEqual(["O", "T"]);
        });
    });
});

