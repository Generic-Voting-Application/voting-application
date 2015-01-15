define('PollOptions', ['knockout'], function (ko) {
    return function PollOptions(pollId) {
        self = this;
        self.options = ko.observableArray();
        self.optionAdding = ko.observable();

        self.newName = ko.observable("");
        self.newInfo = ko.observable("");
        self.newDescription = ko.observable("");

        self.addOption = function () {
            //Don't submit without an entry in the name field
            if (self.newName() === "") return;

            $.ajax({
                type: 'POST',
                url: '/api/poll/' + pollId + '/option',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: self.newName(),
                    Description: self.newDescription(),
                    Info: self.newInfo()
                }),

                success: function () {
                    refreshOptions();
                }
            });

            self.newName("");
            self.newInfo("");
            self.newDescription("");
        };

        var mapOption = function (option) {
            option.highlight = ko.observable(false);
            return option;
        };

        var refreshOptions = function () {
            $.ajax({
                type: 'GET',
                url: "/api/poll/" + pollId + "/option",

                success: function (data) {
                    self.options(data.map(mapOption));
                }
            });
        }

        self.initialise = function (pollData) {
            self.options(pollData.Options.map(mapOption));
            self.optionAdding(pollData.OptionAdding);
        };
    };
});