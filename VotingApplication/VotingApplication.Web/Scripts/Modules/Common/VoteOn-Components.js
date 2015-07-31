(function () {
    'use strict';

    var themingProvider;

    angular
        .module('VoteOn-Components', ['ngMaterial', 'VoteOn-Common'])
        .config([
            '$provide',
            '$mdThemingProvider',
            function themeColorProvider($provide, $mdThemingProvider) {
                themingProvider = $mdThemingProvider;

                // Theme name is what the theme is called in the config
                // PaletteName is primary/accent/background
                // Hue is default/hue-1/hue-2 etc.
                function getThemeColor(themeName, paletteName, hue) {
                    hue = hue || 'default';

                    var theme = themingProvider._THEMES[themeName];
                    var themePaletteColorName = theme.colors[paletteName].name;
                    var themePaletteColorHue = theme.colors[paletteName].hues[hue];
                    var themePaletteColor = themingProvider._PALETTES[themePaletteColorName][themePaletteColorHue];

                    return {
                        value: themingProvider._rgba(themePaletteColor.value),
                        contrast: themingProvider._rgba(themePaletteColor.contrast)
                    };
                }

                $provide.provider('themeColor', function () {
                    return {
                        $get: function () {
                            return {
                                getThemeColor: getThemeColor
                            };
                        }
                    };
                });
            }]);
})();