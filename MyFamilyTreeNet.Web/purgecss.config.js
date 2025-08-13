module.exports = {
  content: [
    './src/**/*.html',
    './src/**/*.ts',
    './src/**/*.js',
  ],
  css: ['./dist/**/*.css'],
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
  ],
  defaultExtractor: content => content.match(/[\w-/:]+(?<!:)/g) || []
};