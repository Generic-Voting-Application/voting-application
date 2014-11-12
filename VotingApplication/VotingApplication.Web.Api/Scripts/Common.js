﻿define([], function () {
    function Common() {

    }

    Common.getJsonFromUrl = function () {
        var query = location.search.substr(1);
        var result = {};
        query.split("&").forEach(function (part) {
            var item = part.split("=");
            result[item[0]] = decodeURIComponent(item[1]);
        });
        return result;
    }

    Common.getSessionId = function () {
        var windowArgs = Common.getJsonFromUrl();
        sessionId = windowArgs['session'];
        return sessionId;
    }

    Common.currentUserId = function () {
        var localUserJSON = localStorage["userId"];

        if (localUserJSON) {
            var localUser = $.parseJSON(localUserJSON);
            if (localUser.expires < Date.now()) {
                localStorage.removeItem("userId");
            }
            else {
                return localUser.id;
            }
        }

        return undefined;
    }

    return Common;
});