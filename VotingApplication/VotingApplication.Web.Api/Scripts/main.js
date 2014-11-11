require.config({
    paths: {
        'jquery': 'jquery-1.10.2.min',
        'knockout': 'knockout-3.2.0'
    },
    shim: {
        'jquery': {
            exports: '$'
        },
        'knockout': {
            exports: 'ko'
        }
    }
});