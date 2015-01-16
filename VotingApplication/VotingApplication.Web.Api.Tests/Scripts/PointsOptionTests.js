define(["PointsOption", "knockout"], function (PointsOption, ko) {
    describe("PointsOption", function () {

        //#region Setup
        var maxPerVoteObservable = ko.observable(5),
            pointsRemainingObservable = ko.observable(10);
        var target;
        beforeEach(function () {
            // Setup test target with pollId and token
            target = new PointsOption(maxPerVoteObservable, pointsRemainingObservable);
        });
        //#endregion

        it("allocationText expect votes and maximum", function () {
            // arrange
            target.value(3);

            // act/assert
            expect(target.allocationText()).toEqual("3/5");
        });

        it("canIncrease with Less Than Max expect Allowed", function () {
            // arrange
            target.value(4);
            pointsRemainingObservable(1);

            // act/assert
            expect(target.canIncrease()).toBe(true);
        });

        it("canIncrease with Max Per Vote expect Disallowed", function () {
            // arrange
            target.value(5);
            pointsRemainingObservable(1);

            // act/assert
            expect(target.canIncrease()).toBe(false);
        });

        it("canIncrease with None Remaining expect Disallowed", function () {
            // arrange
            target.value(0);
            pointsRemainingObservable(0);

            // act/assert
            expect(target.canIncrease()).toBe(false);
        });

        it("canDecrease with More Than Zero expect Allowed", function () {
            // arrange
            target.value(1);

            // act/assert
            expect(target.canDecrease()).toBe(true);
        });

        it("canDecrease with Zero expect Disallowed", function () {
            // arrange
            target.value(0);

            // act/assert
            expect(target.canDecrease()).toBe(false);
        });

        it("decreaseVote expect Subtract One", function () {
            // arrange
            target.value(4);

            // act
            target.decreaseVote();

            // assert
            expect(target.value()).toEqual(3);
        });

        it("increaseVote expect Add One", function () {
            // arrange
            target.value(2);

            // act
            target.increaseVote();

            // assert
            expect(target.value()).toEqual(3);
        });


    });
});

