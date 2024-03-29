module.exports = {
  mode: 'jit', 
  content: [
    '../Views/UmbracoCommerceCheckout/**/*.cshtml',
    './src/scripts/**/*.js'
  ],
  safelist: [
    {
      // Make configurable theme colors safe
      pattern: /(bg|text)-(red|orange|yellow|green|teal|blue|indigo|purple|pink)-500/,
      variants: ['hover'],
    }
  ],
  variants: {
    extend: {
      backgroundColor: ['responsive', 'hocus', 'scrolled', 'group-hover'],
      borderColor: ['responsive', 'hocus', 'scrolled', 'group-hover'],
      textColor: ['responsive', 'hocus', 'scrolled', 'group-hover'],
      margin: ['responsive', 'scrolled'],
      padding: ['responsive', 'scrolled']
    },
  },
  corePlugins: {
    container: false,
  },
  plugins: [
    require('./src/css/plugins/hocus'),
    require('./src/css/plugins/scrolled')
  ],
}
