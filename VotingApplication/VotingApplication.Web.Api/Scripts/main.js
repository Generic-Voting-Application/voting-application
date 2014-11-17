require.config({
    paths: {
        'jquery': 'Lib/jquery-2.1.1.min',
        'knockout': 'Lib/knockout-3.2.0',
        'bootstrap': 'Lib/bootstrap.min'
    },
    shim: {
        'jquery': {
            exports: '$'
        },
        'knockout': {
            deps: ['jquery'],
            exports: 'ko'
        }
    }
});