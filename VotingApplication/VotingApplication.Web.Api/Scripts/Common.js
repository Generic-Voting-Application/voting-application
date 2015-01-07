define(['jquery'], function () {

    // Checks if elements are collapsed
    (function () {

        $.fn.isCollapsed = function () {
            var $this = $(this).find('.accordion-body').filter(':visible');
            return $this ? !$this.parents().toArray().some(function (element) {
                return !$(element).hasClass('in');
            }) : false;
        };

        $.fn.collapseSection = function (collapse) {
            if (($(this).isCollapsed() && collapse == 'hide') || (!$(this).isCollapsed() && collapse == 'show')) {
                return;
            }

            $(this).find('.accordion-body').collapse(collapse);
        };
    })();

    function Common() {

    }

    Common.getPollId = function () {
        return routeId;
    };

    Common.getManageId = function () {
        return routeId;
    };

    Common.getToken = function () {
        return routeToken || Common.sessionItem("token");
    }

    Common.currentUserId = function () {
        return Common.sessionItem("id");
    };

    Common.currentUserName = function () {
        return Common.sessionItem("userName");
    };

    Common.sessionItem = function (sessionKey) {
        var localUserJSON = localStorage["user_" + Common.getPollId()];

        if (localUserJSON) {
            var localUser = $.parseJSON(localUserJSON);
            if (localUser.expires < Date.now()) {
                localStorage.removeItem("user_" + Common.getPollId());
            }
            else {
                return localUser[sessionKey];
            }
        }

        return undefined;
    }

    Common.loginUser = function (userData, userName) {
        //Expire in 6 hours
        var expiryTime = Date.now() + (6 * 60 * 60 * 1000);
        localStorage["user_" + Common.getPollId()] = JSON.stringify({ id: userData.UserId, userName: userName, expires: expiryTime, token: userData.TokenGuid });
    };

    Common.logoutUser = function () {
        localStorage.removeItem("user_" + Common.getPollId());
    }

    Common.keyIsEnter = function (key, callback) {
        if (key && key.keyCode == 13) {
            callback();
        }
    };

    Common.handleError = function (error) {

        var friendlyText;

        switch (error.status) {
            case 404:
                friendlyText = 'Double check the poll ID and ensure that you have logged in correctly'
                break;
        }

        var newError = '<div class="alert alert-danger">' +
        '<a href="#" class="close" data-dismiss="alert">&times;</a>' +
        '<strong>' + error.statusText + ' </strong>' +
        JSON.parse(error.responseText).Message +
        (friendlyText ? '<br/>' + friendlyText : '') +
        '</div>';

        $('#errorArea').append(newError);
    }

    return Common;
});