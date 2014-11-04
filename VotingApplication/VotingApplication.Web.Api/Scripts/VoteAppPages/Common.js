function getJsonFromUrl() {
    var query = location.search.substr(1);
    var result = {};
    query.split("&").forEach(function (part) {
        var item = part.split("=");
        result[item[0]] = decodeURIComponent(item[1]);
    });
    return result;
}

function getSessionId() {
    var windowArgs = getJsonFromUrl();
    sessionId = windowArgs['session'];
    return sessionId;
}

$(document).ready(function () {
    var sessionId = getSessionId();

    $("#HomeLink").attr('href', '/?session=' + sessionId);
    $("#ResultLink").attr('href', '/Result?session=' + sessionId);
});