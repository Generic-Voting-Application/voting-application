require.config({
    baseUrl: "../../votingapplication.web.api/scripts",
    paths: {
        'mockjax': '../../votingapplication.web.api.tests/scripts/lib/jquery.mockjax'
    },
    shim: {
        'mockjax': {
            deps: ['jquery'],
            exports: '$.mockjax'
        }
    }
});
