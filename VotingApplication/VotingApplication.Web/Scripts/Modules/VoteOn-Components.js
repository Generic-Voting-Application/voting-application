﻿(function () {
    'use strict';

    var themingProvider;

    angular
        .module('VoteOn-Components', ['ngMaterial', 'VoteOn-Common'])
        .config(function themeColorProvider($provide, $mdThemingProvider) {
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

                return themingProvider._rgba(themePaletteColor.value);
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
        });
})();