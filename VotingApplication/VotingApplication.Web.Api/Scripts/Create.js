define('Create', ['jquery', 'knockout', 'Common', 'KnockoutExtensions'], function ($, ko, Common) {
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
            
            $.ajax({
                type: 'POST',
                url: '/api/poll',
                contentType: 'application/json',

                data: JSON.stringify({
                    Name: self.pollName() || undefined,
                    Creator: self.creatorName() || undefined,
                    Email: self.creatorEmail() || undefined,
                    TemplateId: self.templateId() || undefined,
                    VotingStrategy: self.strategy() || undefined,
                    MaxPoints: self.maxPoints() || undefined,
                    MaxPerVote: self.maxPerVote() || undefined,
                    InviteOnly: self.inviteOnly() || undefined,
                    AnonymousVoting: self.anonymousVoting() || undefined,
                    RequireAuth: self.requireAuth() || undefined,
                    Expires: self.expiry() || undefined,
                    ExpiryDate: new Date(self.expiryDate()) || undefined,
                    OptionAdding: self.optionAdding() || undefined
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
