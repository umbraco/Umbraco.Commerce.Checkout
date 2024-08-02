module.exports = {
  mode: 'jit',
  content: [
    '../Views/UmbracoCommerceCheckout/**/*.cshtml',
    './src/surface/**/*.ts',
  ],
  safelist: [
    // Make configurable theme colors safe
    {
      pattern: /(bg|text)-black/,
      variants: ['hover'],
    },
    {
      pattern: /(bg|text)-(red|orange|yellow|green|teal|blue|indigo|purple|pink)-500/,
      variants: ['hover'],
    },
    {
      pattern: /(break-)|(flex)|(w-)/,
      variants: ['sm', 'md', 'lg'],
    },
  ],
  variants: {
    extend: {
      backgroundColor: ['responsive', 'hocus', 'scrolled', 'group-hover'],
      borderColor: ['responsive', 'hocus', 'scrolled', 'group-hover'],
      textColor: ['responsive', 'hocus', 'scrolled', 'group-hover'],
      margin: ['responsive', 'scrolled'],
      padding: ['responsive', 'scrolled'],
    },
  },
  corePlugins: {
    container: false,
  },
  plugins: [
    require('./src/surface/css/plugins/hocus'),
    require('./src/surface/css/plugins/scrolled'),
  ],
};
