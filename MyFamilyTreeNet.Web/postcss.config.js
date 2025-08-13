const purgecss = require('@fullhuman/postcss-purgecss');

module.exports = {
  plugins: [
    // Only run PurgeCSS in production
    ...(process.env['NODE_ENV'] === 'production' ? [
      purgecss({
        content: [
          './src/**/*.html',
          './src/**/*.ts',
          './src/**/*.js',
        ],
        safelist: [
          // Bootstrap classes that might be added dynamically
          /^btn/,
          /^alert/,
          /^modal/,
          /^dropdown/,
          /^nav/,
          /^card/,
          /^form/,
          /^input/,
          /^is-/,
          /^was-/,
          /^has-/,
          /^show/,
          /^hide/,
          /^fade/,
          /^collapse/,
          /^active/,
          /^disabled/,
          // Font Awesome icons
          /^fa/,
          /^fas/,
          /^far/,
          /^fab/,
          // Angular Material
          /^mat-/,
          /^cdk-/,
          // D3.js related classes
          /^node/,
          /^link/,
          // Custom utility classes
          /^text-/,
          /^bg-/,
          /^border/,
          /^rounded/,
          /^shadow/,
          /^p-/,
          /^m-/,
          /^w-/,
          /^h-/,
          /^d-/,
          /^flex/,
          /^justify/,
          /^align/,
          // Animation classes
          /^spin/,
          /^bounce/,
          /^pulse/,
          // Loading states
          /^spinner/,
          /^loading/,
          // Responsive classes
          /^sm:/,
          /^md:/,
          /^lg:/,
          /^xl:/,
          /^2xl:/,
        ],
        defaultExtractor: content => content.match(/[\w-/:]+(?<!:)/g) || []
      })
    ] : [])
  ]
};