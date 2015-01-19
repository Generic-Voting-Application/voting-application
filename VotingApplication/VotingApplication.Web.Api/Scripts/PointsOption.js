define('PointsOption', ['knockout'], function (ko) {
    return function PointsOption(maxPerVoteObservable, pointsRemainingObservable) {
        var self = this;

        self.value = ko.observable(0);

        self.allocationText = ko.computed(function () {
            return self.value() + '/' + maxPerVoteObservable();
        });

        self.canIncrease = ko.computed(function () {
            return self.value() < maxPerVoteObservable() && pointsRemainingObservable() > 0;
        });
        self.canDecrease = ko.computed(function () {
            return self.value() > 0;
        });

        self.decreaseVote = function () {
            self.value(self.value() - 1);
        }

        self.increaseVote = function (data, event) {
            self.value(self.value() + 1);
        }
    };
});