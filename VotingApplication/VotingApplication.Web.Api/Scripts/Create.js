require(['jquery', 'knockout', 'Common'], function ($, ko, Common) {
    function HomeViewModel() {
        var self = this;

        self.createPoll = function () {

        };
    }

    ko.applyBindings(new HomeViewModel());
});
