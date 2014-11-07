function AdminViewModel() {
    var self = this;

    self.votes = ko.observableArray();
    self.options = ko.observableArray();
    self.selectedDeleteOptionId = null;

    $(document).ready(function () {
        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/vote",

                success: function () {
                    $("#reset-votes").attr('disabled', 'disabled');
                    $("#reset-votes").text("Votes were reset");
                    self.populateVotes();
                }
            });
        }

        self.deleteVote = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/vote?id=' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateVotes();
                }
            });
        }

        self.populateVotes = function () {
            $.ajax({
                type: 'GET',
                url: "/api/vote",

                success: function (data) {
                    //Replace contents of self.votes with 'data'
                    self.votes(data);
                }
            });
        }

        self.populateOptions = function () {
            $.ajax({
                type: 'GET',
                url: "/api/option",

                success: function (data) {
                    self.options(data);
                }
            });
        }

        self.deleteOption = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/option?id=' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateOptions();
                }
            });
        }

        $("#reset-votes").click(self.resetVotes);
        self.populateVotes();
        self.populateOptions();
    });
}

ko.applyBindings(new AdminViewModel());
