define('Create', ['jquery', 'knockout', 'KnockoutExtensions'], function ($, ko) {
    return function CreateViewModel() {
        var self = this;

        self.templates = ko.observableArray();

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

            var expiryDate = self.expiry() ? new Date(self.expiryDate()) : null;
            if (expiryDate === 'Invalid Date' && self.expiry()) {
                // Default to a valid date
                expiryDate = new Date();
                expiryDate.setMinutes(expiryDate.getMinutes() + 30);
            }

            $.ajax({
                type: 'POST',
                url: '/api/poll',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: self.pollName(),
                    Creator: self.creatorName(),
                    Email: self.creatorEmail(),
                    templateId: self.templateId(),
                    VotingStrategy: self.strategy(),
                    MaxPoints: self.maxPoints(),
                    MaxPerVote: self.maxPerVote(),
                    InviteOnly: self.inviteOnly(),
                    AnonymousVoting: self.anonymousVoting(),
                    RequireAuth: self.requireAuth(),
                    Expires: self.expiry(),
                    ExpiryDate: expiryDate,
                    OptionAdding: self.optionAdding()
                }),

                success: function (data) {
                    window.location.href = "/Manage/Index/" + data.ManageID;
                }
            });
        };

        self.populateTemplates = function () {
            $.ajax({
                type: 'GET',
                url: '/api/template',

                success: function (data) {
                    self.templates(data);
                }
            });
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

        var setupTooltips = function() {
            $(document).tooltip

            function showOrHideElement(show, element) {
                element.next(".tip").toggle(show);
            }

            var tooltipTargets = $(".help-message");

            for (var i = 0; i < tooltipTargets.length; i++)
            {
                var $hoverTarget = $(tooltipTargets[i]);

                var hideElement = showOrHideElement.bind(null, false, $hoverTarget);
                var showElement = showOrHideElement.bind(null, true, $hoverTarget);
                ko.utils.registerEventHandler($hoverTarget, "mouseover", showElement);
                ko.utils.registerEventHandler($hoverTarget, "mouseout", hideElement);
                hideElement();
            }
        }

        self.initialise = function () {
            self.populateTemplates();
            setupTooltips();

            ko.applyBindings(this);
        };
    }
});
