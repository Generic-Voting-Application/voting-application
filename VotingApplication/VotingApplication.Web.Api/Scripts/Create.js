define('Create', ['jquery', 'knockout', 'Common', 'Validation', 'KnockoutExtensions'], function ($, ko, Common, Validation) {
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
            if (!Validation.validateForm($("#poll-create-form"))) return;
            self.creatingPoll(true);
            
            $.ajax({
                type: 'POST',
                url: '/api/poll',
                contentType: 'application/json',
                beforeSend: function (header) {
                    header.setRequestHeader("Authorization", "Bearer " + sessionStorage['creator_token']);
                },

                // This is needed to ensure that values not given are not set on the JSON object
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
                    self.navigateToManage(data.ManageId);
                },

                error: Common.handleError
            });
        };

        self.navigateToManage = function (manageId) {
            window.location.href = "/Manage/Index/" + manageId;
        };

        self.initialise = function () {
            Common.setupTooltips();

            ko.applyBindings(this);
        };
    }
});
