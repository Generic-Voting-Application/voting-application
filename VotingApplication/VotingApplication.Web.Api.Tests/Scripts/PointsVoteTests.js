define(["PointsVote", "Common", "mockjax", "knockout"], function (PointsVote, Common, mockjax, ko) {
    describe("PointsVote", function () {

        //#region Setup
        var target;
        beforeEach(function () {
            // Setup test target with pollId and token
            target = new PointsVote("303", "515");
            target.initialise({ MaxPerVote: 5, MaxPoints: 10, Options: [] });

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

        it("pointsForOption expect Points object", function () {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }
            ]);
            target.pointsArray([
                { value: ko.observable(0) }, { value: ko.observable(3) }, { value: ko.observable(2) }
            ]);

            // act/assert
            expect(target.pointsForOption(1).value()).toEqual(3);
        });

        it("pointsRemaining expect Subtract All From Limit", function () {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }
            ]);
            target.pointsArray([
                { value: ko.observable(0) }, { value: ko.observable(3) }, { value: ko.observable(2) }
            ]);

            // act/assert
            expect(target.pointsRemaining()).toEqual(5);
        });

        it("percentageRemaining expect Calculate Percent Remain", function () {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }
            ]);
            target.pointsArray([
                { value: ko.observable(0) }, { value: ko.observable(4) }, { value: ko.observable(2) }
            ]);

            // act/assert
            expect(target.percentRemaining()).toEqual(40);
        });

        it("pollOptions with Added Option expect Add empty points", function () {
            // arrange
            target.initialise({MaxPerVote: 5, MaxPoints: 10, Options: [
                    { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }
                ]});
            target.pointsArray([
                { value: ko.observable(0) }, { value: ko.observable(3) }, { value: ko.observable(2) }
            ]);

            // act
            target.pollOptions.options.push({ Id: 25, Name: "Option-4" });

            // assert
            expect(target.pointsArray().map(function (p) { return p.value(); })).toEqual([0, 3, 2, 0]);
        });

        it("doVote with Token expect Post vote and notify", function (done) {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }
            ]);
            target.pointsArray([
                { value: ko.observable(0) }, { value: ko.observable(3) }, { value: ko.observable(2) }
            ]);

            var expectedVotes = [
                { OptionId: 17, VoteValue: 3, TokenGuid: "515" },
                { OptionId: 21, VoteValue: 2, TokenGuid: "515" }
            ];

            var posted = false;
            mockjax({
                type: "PUT", url: "/api/user/912/poll/303/vote",
                data: JSON.stringify(expectedVotes),
                response: function () { posted = true; }, responseText: {}
            });

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
            target = new PointsVote("303");

            // arrange
            target.pollOptions.options([{ Id: 13, Name: "Option-1" }]);
            target.pointsArray([{ value: ko.observable(1) }]);

            var posted = false;
            mockjax({
                type: "PUT", url: "/api/user/912/poll/303/vote",
                data: JSON.stringify([{ OptionId: 13, VoteValue: 1, TokenGuid: "616" }]),
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

        it("getVotes without Voted expect Clear points", function (done) {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }
            ]);
            target.pointsArray([
                { value: ko.observable(0) }, { value: ko.observable(3) }, { value: ko.observable(2) }
            ]);

            mockjax({
                type: "GET", url: "/api/user/912/poll/303/vote",
                responseText: []
            });

            // act
            target.getVotes("303", 912);

            // assert
            setTimeout(function () {
                expect(target.pointsArray().map(function (p) { return p.value(); })).toEqual([ 0, 0, 0]);
                done();
            }, 10);
        });

        it("getVotes with Voted expect Set highlighted", function (done) {
            // arrange
            target.pollOptions.options([
                { Id: 13, Name: "Option-1" }, { Id: 17, Name: "Option-2" }, { Id: 21, Name: "Option-3" }
            ]);
            target.pointsArray([
                { value: ko.observable(0) }, { value: ko.observable(3) }, { value: ko.observable(2) }
            ]);

            mockjax({
                type: "GET", url: "/api/user/912/poll/303/vote",
                responseText: [{ OptionId: 17, VoteValue: 1 }, { OptionId: 13, VoteValue: 2 }]
            });

            // act
            target.getVotes("303", 912);

            // assert
            setTimeout(function () {
                expect(target.pointsArray().map(function (p) { return p.value(); })).toEqual([2, 1, 0]);
                done();
            }, 10);
        });

        it("displayResults without Votes expect Draw empty chart", function () {
            // act
            target.displayResults([]);

            // assert
            expect(target.chartData()).toEqual([{ name: 'Votes', data: [] }]);
        });

        it("displayResults with Votes expect Draw grouped votes", function () {
            // arrange
            var data = [
                { OptionName: "One", VoteValue: 3, VoterName: "User-1" },
                { OptionName: "Two", VoteValue: 1, VoterName: "User-2" },
                { OptionName: "One", VoteValue: 2, VoterName: "User-2" }
            ];

            // act
            target.displayResults(data);

            // assert
            var expectedVotes = [{ name: 'Votes', data: [
                { Name: 'One', Sum: 5, Voters: ['User-1 (3)', 'User-2 (2)'] },
                { Name: 'Two', Sum: 1, Voters: ['User-2 (1)'] }
            ]}];
            expect(target.chartData()).toEqual(expectedVotes);
        });
    });
});

