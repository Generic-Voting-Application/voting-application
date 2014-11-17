require(['jquery', 'knockout', 'Common'], function ($, ko, Common) {
    function AdminViewModel() {
        var self = this;
        var manageId = 0;

        self.votes = ko.observableArray();
        self.options = ko.observableArray();
        self.selectedDeleteOptionId = null;

        self.resetVotes = function () {
            $.ajax({
                type: 'DELETE',
                url: "/api/manage/" + manageId + "/vote",

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
                url: '/api/manage/' + manageId + '/vote?id=' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateVotes();
                }
            });
        }

        self.deleteOption = function (data, event) {
            $.ajax({
                type: 'DELETE',
                url: '/api/manage/' + manageId + '/option/' + data.Id,
                contentType: 'application/json',

                success: function () {
                    self.populateOptions();
                    self.populateVotes();
                }
            });
        }

        self.populateOptions = function () {
            $.ajax({
                type: 'GET',
                url: 'api/manage/' + manageId + '/option',

                success: function (data) {
                    self.options(data);
                }
            })
        }

        self.populateVotes = function () {
            $.ajax({
                type: 'GET',
                url: "/api/manage/" + manageId + "/vote",

                success: function (data) {
                    //Replace contents of self.votes with 'data'
                    self.votes(data);
                }
            });
        }

        $(document).ready(function () {
            manageId = Common.getManageId();

            self.populateVotes();
            self.populateOptions();
        });
    }

    ko.applyBindings(new AdminViewModel());
});