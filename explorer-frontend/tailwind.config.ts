module.exports = {
  darkMode: "class",
  content: [
    "./src/assets/**/*.{vue,js,css}",
    "./src/components/**/*.{vue,js}",
    "./src/layouts/**/*.vue",
    "./src/pages/**/*.vue",
    "./src/plugins/**/*.{js,ts}",
    "./nuxt.config.{js,ts}",
    "./src/app.vue"
  ],
  theme: {
    extend: {
      fontFamily: {
        "inter": ["Inter", "sans-serif"],
      }
    }
  },
  variants: {
    extend: {},
  },
  plugins: [],
};