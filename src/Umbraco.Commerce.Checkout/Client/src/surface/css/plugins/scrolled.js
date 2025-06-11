const plugin = require('tailwindcss/plugin');

module.exports = plugin(function ({ addVariant, e }) {

    addVariant('scrolled', ({ modifySelectors, separator, container }) => {
        modifySelectors(({ selector }) => {
            return `.scrolled .${e(`scrolled${separator}`)}${selector.substr(1)}`;
        });
    });

});
