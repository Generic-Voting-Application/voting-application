define(['jquery', 'knockout', 'datetimepicker', 'moment', 'Common', 'jqueryUI'], function ($, ko, datetimepicker, moment, Common) {
    return function CreateViewModel() {
        var self = this;

        self.selectedStrategy = ko.observable();
        self.expires = ko.observable(false);

        self.pollName = ko.observable("");
        self.creatorName = ko.observable("");
        self.creatorEmail = ko.observable("");

        self.createPoll = function () {
            //Clear out previous error messages
            $('text').remove('.error-message');

            var inputs = $("#poll-create-form div");
            for (var i = 0; i < inputs.length; i++) {
                self.validateField(inputs[i]);
            }

            if (!$("#poll-create-form")[0].checkValidity()) {
                return;
            }

            $("#poll-create-btn").attr('disabled', 'disabled');
            $("#poll-create-btn").text('Creating...');

            var templateId = $("#template").val();
            var strategy = $("#voting-strategy").val();
            var maxPoints = $("#max-points").val() || 7;
            var maxPerVote = $("#max-per-vote").val() || 3;
            var inviteOnly = $('#invite-only').is(':checked');
            var anonymousVoting = $('#anonymous-voting').is(':checked');
            var requireAuth = $('#require-auth').is(':checked');
            var expiry = $('#expiry').is(':checked');
            var expiryDate = expiry ? new Date($('#expiry-date').val()) : null;
            var optionAdding = $('#option-adding').is(':checked');

            if (expiryDate == 'Invalid Date' && expiry) {
                expiryDate = new Date();
                expiryDate.setMinutes(expiryDate.getMinutes() + 30);
            }

            $.ajax({
                type: 'POST',
                url: '/api/poll',
                contentType: 'application/json',
                beforeSend: function (header) {
                    header.setRequestHeader("Authorization", "Bearer " + sessionStorage['creator_token']);
                },

                data: JSON.stringify({
                    Name: self.pollName(),
                    Creator: self.creatorName(),
                    Email: self.creatorEmail(),
                    templateId: templateId,
                    VotingStrategy: strategy,
                    MaxPoints: maxPoints,
                    MaxPerVote: maxPerVote,
                    InviteOnly: inviteOnly,
                    AnonymousVoting: anonymousVoting,
                    RequireAuth: requireAuth,
                    Expires: expiry,
                    ExpiryDate: expiryDate,
                    OptionAdding: optionAdding
                }),

                success: function (data) {
                    window.location.href = "/Manage/Index/" + data.ManageID;
                }
            });
        };

        self.validateField = function (field) {
            var inputField = $(field).find('input')[0];

            if (!inputField) {
                return;
            }

            if (!inputField.checkValidity()) {
                $(inputField).addClass('error');
                var errorMessage = inputField.validationMessage;
                $(field).append('<text class="error-message">' + errorMessage + '</text>');
            }
            else {
                $(inputField).removeClass('error');
            }
        };

        $(document).ready(function () {
            var defaultExpiryDate = moment().add(30, 'minutes');
            $('#expiry-date').datetimepicker({ defaultDate: defaultExpiryDate, minDate: moment() });

            // Select first tab
            $('#tabBar li a:first').tab('show')

            Common.setupTooltips();
        });

        ko.applyBindings(this);
    }
});
