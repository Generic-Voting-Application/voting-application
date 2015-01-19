define('Validation', ['jquery'], function ($) {

    return new function Validation() {
        var self = this;

        self.validateForm = function (form) {
            //Clear out previous error messages
            $('text').remove('.error-message');

            var inputs = form.find(".form-group");
            for (var i = 0; i < inputs.length; i++) {
                self.validateField(inputs[i]);
            }

            return form[0].checkValidity();
        };

        self.validateField = function (field) {
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

    };
});
