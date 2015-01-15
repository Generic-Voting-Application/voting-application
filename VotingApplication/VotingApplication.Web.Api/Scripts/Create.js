define('Create', ['jquery', 'knockout', 'Common', 'KnockoutExtensions'], function ($, ko, Common) {
    return function CreateViewModel() {
        var self = this;

        // Basic Poll options
        self.pollName = ko.observable("");
        self.creatorName = ko.observable("");
        self.creatorEmail = ko.observable("");

        // Advanced Poll options
        self.templateId = ko.observable("");
        self.strategy = ko.observable("");
        self.maxPoints = ko.observable(7);
        self.maxPerVote = ko.observable(3);
        self.inviteOnly = ko.observable(false);
        self.anonymousVoting = ko.observable(false);
        self.requireAuth = ko.observable(false);
        self.expiry = ko.observable(false);
        self.expiryDate = ko.observable("");
        self.optionAdding = ko.observable(false);

        self.creatingPoll = ko.observable(false);

        self.createPoll = function () {
            if (!self.validateForm()) return;
            self.creatingPoll(true);
            
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
                    TemplateId: self.templateId(),
                    VotingStrategy: self.strategy(),
                    MaxPoints: self.maxPoints(),
                    MaxPerVote: self.maxPerVote(),
                    InviteOnly: self.inviteOnly(),
                    AnonymousVoting: self.anonymousVoting(),
                    RequireAuth: self.requireAuth(),
                    Expires: self.expiry(),
                    ExpiryDate: new Date(self.expiryDate()),
                    OptionAdding: self.optionAdding()
                }),

                success: function (data) {
                    self.navigateToManage(data.ManageID);
                },

                error: Common.handleError
            });
        };

        self.navigateToManage = function (manageId) {
            window.location.href = "/Manage/Index/" + manageId;
        };

        self.validateForm = function () {
            //Clear out previous error messages
            $('text').remove('.error-message');

            var inputs = $("#poll-create-form .form-group");
            for (var i = 0; i < inputs.length; i++) {
                validateField(inputs[i]);
            }

            return $("#poll-create-form")[0].checkValidity();
        };

        var validateField = function (field) {
            if ($(field).is(':visible')) {
                var $inputField = $(field).find('input');
                var inputField = $inputField[0];

                if (!inputField) {
                    return;
                }

                if ($inputField.attr('date') !== undefined) {
                    // Validation of date fields
                    if (isNaN(Date.parse(self.expiryDate()))) {
                        inputField.setCustomValidity("Please enter a valid date");
                    } else {
                        inputField.setCustomValidity("");
                    }
                }

                if (!inputField.checkValidity()) {
                    $inputField.addClass('error');
                    var errorMessage = inputField.validationMessage;
                    $(field).append('<text class="error-message">' + errorMessage + '</text>');
                }
                else {
                    $inputField.removeClass('error');
                }
            }
        };

        self.initialise = function () {
            Common.setupTooltips();

            ko.applyBindings(this);
        };
    }
});
