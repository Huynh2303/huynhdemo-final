// tailwind-config.js
tailwind.config = {
    darkMode: "class",
    theme: {
        extend: {
            colors: {
                "on-error": "#ffffff",
                "surface": "#f5faff",
                "on-secondary": "#ffffff",
                "tertiary-fixed-dim": "#ffb876",
                "surface-variant": "#dee3e8",
                "on-secondary-fixed": "#1a1c1c",
                "surface-dim": "#d6dae0",
                "primary-container": "#00aeef",
                "surface-container-lowest": "#ffffff",
                "error-container": "#ffdad6",
                "surface-container-high": "#e4e9ee",
                "on-tertiary-fixed-variant": "#6b3b00",
                "primary": "#00658d",
                "on-tertiary-fixed": "#2d1600",
                "secondary-fixed-dim": "#c6c6c7",
                "tertiary": "#8d4f00",
                "surface-container-highest": "#dee3e8",
                "on-primary": "#ffffff",
                "inverse-surface": "#2c3135",
                "inverse-on-surface": "#ecf1f7",
                "primary-fixed-dim": "#82cfff",
                "on-primary-container": "#003e58",
                "on-secondary-fixed-variant": "#454747",
                "surface-container-low": "#eff4fa",
                "outline-variant": "#bdc8d1",
                "on-tertiary": "#ffffff",
                "primary-fixed": "#c6e7ff",
                "secondary-fixed": "#e2e2e2",
                "surface-bright": "#f5faff",
                "error": "#ba1a1a",
                "secondary": "#5d5f5f",
                "on-surface-variant": "#3e4850",
                "tertiary-fixed": "#ffdcc0",
                "inverse-primary": "#82cfff",
                "outline": "#6e7881",
                "surface-container": "#eaeef4",
                "surface-tint": "#00658d",
                "on-tertiary-container": "#572f00",
                "on-primary-fixed-variant": "#004c6b",
                "on-secondary-container": "#616363",
                "on-error-container": "#93000a",
                "background": "#f5faff",
                "tertiary-container": "#ea8c21",
                "on-background": "#171c20",
                "on-surface": "#171c20",
                "secondary-container": "#dfe0e0",
                "on-primary-fixed": "#001e2d"
            },
            borderRadius: {
                DEFAULT: "0.25rem",
                lg: "0.5rem",
                xl: "0.75rem",
                full: "9999px"
            },
            spacing: {
                unit: "4px",
                lg: "24px",
                gutter: "16px",
                xl: "48px",
                md: "16px",
                sm: "8px",
                xs: "4px",
                "container-max": "1200px"
            },
            fontFamily: {
                "body-sm": ["Be Vietnam Pro"],
                "body-base": ["Be Vietnam Pro"],
                "price-lg": ["Be Vietnam Pro"],
                "display-lg": ["Be Vietnam Pro"],
                "headline-md": ["Be Vietnam Pro"],
                "label-bold": ["Be Vietnam Pro"]
            },
            fontSize: {
                "body-sm": ["14px", { lineHeight: "1.5", fontWeight: "400" }],
                "body-base": ["16px", { lineHeight: "1.5", fontWeight: "400" }],
                "price-lg": ["24px", { lineHeight: "1", fontWeight: "700" }],
                "display-lg": ["32px", { lineHeight: "1.2", fontWeight: "700" }],
                "headline-md": ["20px", { lineHeight: "1.4", fontWeight: "600" }],
                "label-bold": ["12px", { lineHeight: "1.2", letterSpacing: "0.02em", fontWeight: "700" }]
            }
        }
    }
};