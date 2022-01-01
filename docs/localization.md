# Localization
Localization files located at [explorer-frontend/localization](/explorer-frontend/localization)

Each folder there should match language code, logical lingual blocks defined in **localization/\[locale_code\]/index.ts**

## Adding new locale
1. Copy **localization/en** directory to **localization/\[locale_code\]**
2. Change each **JSON** files inside newly copied directory with translation. JSON format is {\"key\": \"value\"}, you should translate only values. There also can be symbols like {block} - this are placeholders, they are replaced at runtime to actual values
3. Add definition of new locale inside **nuxt.config.ts**
```
locales: {
    "en": "English",
    "ru": "Русский",
    // add here new locale in format "[locale_code]": "locale_display_name"
}
```
4. Add **png** locale icon to **public/images/locales/\[lang_code\].png**