define(["PollOptions", "mockjax", "knockout"], function (PollOptions, mockjax, ko) {
    describe("PollOptions", function () {

        //#region Setup
        var target;
        beforeEach(function () {
            // Setup test target
            target = new PollOptions(303);

            // Setup mockjax
            $.mockjaxSettings.responseTime = 1;
            mockjax.clear();
        });
        //#endregion

        it("initialise with pollData expect options and option-adding setting", function () {
            // arrange
            var pollData = {
                Options: [{ Id: 13, Name: "Original-1" }, { Id: 17, Name: "Original-2" }],
                OptionAdding: true
            };
            expect(target.optionAdding()).toBe(false);

            // act
            target.initialise(pollData);

            // assert
            expect(target.options().map(function (o) { return o.Name; }).join())
                    .toEqual("Original-1,Original-2");
            expect(target.optionAdding()).toBe(true);
        });

        it("initialise with pollData expect Add highlight property", function () {
            // arrange
            var pollData = {
                Options: [{ Id: 13, Name: "Original-1" }, { Id: 17, Name: "Original-2" }],
                OptionAdding: true
            };

            // act
            target.initialise(pollData);

            // assert
            expect(target.options()[0].highlight()).toBe(false);
            expect(target.options()[1].highlight()).toBe(false);
        });

        it("addOption without Name expect Ignore", function (done) {
            // arrange
            target.newName("");

            // Mock the ajax post
            var posted = false;
            mockjax({
                type: "POST", url: "/api/poll/303/option",
                response: function () { posted = true; }, responseText: {}
            });

            spyOn(target, 'refreshOptions');

            // act
            target.addOption();

            // assert
            setTimeout(function () {
                expect(posted).toBe(false);
                expect(target.refreshOptions.calls.count()).toEqual(0);
                done();
            }, 10);
        });

        it("addOption with Name expect Post option and add to list", function (done) {
            // arrange
            target.newName("test-name");
            target.newInfo("test-info");
            target.newDescription("test-desc");

            // Mock the ajax post
            var posted = false;
            mockjax({
                type: "POST", url: "/api/poll/303/option",
                data: JSON.stringify({ Name: "test-name", Description: "test-desc", Info: "test-info" }),
                response: function () { posted = true; }, responseText: {}
            });

            spyOn(target, 'refreshOptions');

            // act
            target.addOption();

            // assert
            setTimeout(function () {
                expect(posted).toBe(true);
                expect(target.refreshOptions).toHaveBeenCalled();

                expect(target.newName()).toEqual("");
                expect(target.newInfo()).toEqual("");
                expect(target.newDescription()).toEqual("");
                done();
            }, 10);
        });

        it("refreshOptions with New Options expect Append new options", function (done) {
            // arrange
            target.options([{ Id: 13, Name: "Original-1" }, { Id: 17, Name: "Original-2" }]);

            mockjax({
                type: "GET", url: "/api/poll/303/option",
                responseText: [
                    { Id: 13, Name: "Original-1" }, { Id: 17, Name: "Original-2" },
                    { Id: 21, Name: "New-1" }, { Id: 25, Name: "New-2" }
                ]
            });

            // act
            target.refreshOptions();

            // assert
            setTimeout(function () {
                expect(target.options().map(function (o) { return o.Name; }).join())
                    .toEqual("Original-1,Original-2,New-1,New-2");

                done();
            }, 10);
        });

        it("refreshOptions with New Option expect Preserve higlighting", function (done) {
            // arrange
            target.options([
                { Id: 13, Name: "Original-1", highlight: ko.observable(false) },
                { Id: 17, Name: "Original-2", highlight: ko.observable(true) }
            ]);

            mockjax({
                type: "GET", url: "/api/poll/303/option",
                responseText: [
                    { Id: 13, Name: "Original-1" }, { Id: 17, Name: "Original-2" },
                    { Id: 21, Name: "New-1" }
                ]
            });

            // act
            target.refreshOptions();

            // assert
            setTimeout(function () {
                expect(target.options()[0].highlight()).toBe(false);
                expect(target.options()[1].highlight()).toBe(true);
                expect(target.options()[2].highlight()).toBe(false);

                done();
            }, 10);
        });

        it("getWinners with No Groups expect No Winner", function () {
            // act
            var result = target.getWinners([]);

            // assert
            expect(result).toEqual([]);
        });

        it("getWinners with Clear Winner expect Return Winner Name", function () {
            // arrange
            var groupedVotes = [
                { Name: "Option-1", Sum: 3 },
                { Name: "Option-2", Sum: 8 },
                { Name: "Option-3", Sum: 5 }
            ];

            // act
            var result = target.getWinners(groupedVotes);

            // assert
            expect(result).toEqual([ "Option-2" ]);
        });

        it("getWinners with Joint Winners expect Return Winners Names", function () {
            // arrange
            var groupedVotes = [
                { Name: "Option-1", Sum: 3 },
                { Name: "Option-2", Sum: 5 },
                { Name: "Option-3", Sum: 5 }
            ];

            // act
            var result = target.getWinners(groupedVotes);

            // assert
            expect(result).toEqual(["Option-2", "Option-3"]);
        });

        it("getWinners with Callback expect Get Winner from Callback data", function () {
            // arrange
            var groupedVotes = [
                { testName: "Option-1", testSum: 3 },
                { testName: "Option-2", testSum: 8 },
                { testName: "Option-3", testSum: 5 }
            ];

            var callbackFn = function (group) {
                return { Name: group.testName, Sum: group.testSum };
            };

            // act
            var result = target.getWinners(groupedVotes, callbackFn);

            // assert
            expect(result).toEqual(["Option-2"]);
        });

    });
});

