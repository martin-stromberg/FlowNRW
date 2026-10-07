---
name: Trans-NRW Mobil System
colors:
  surface: '#faf9fe'
  surface-dim: '#dad9df'
  surface-bright: '#faf9fe'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f4f3f8'
  surface-container: '#eeedf3'
  surface-container-high: '#e9e7ed'
  surface-container-highest: '#e3e2e7'
  on-surface: '#1a1b1f'
  on-surface-variant: '#414755'
  inverse-surface: '#2f3034'
  inverse-on-surface: '#f1f0f5'
  outline: '#717786'
  outline-variant: '#c1c6d7'
  surface-tint: '#005bc1'
  primary: '#0058bc'
  on-primary: '#ffffff'
  primary-container: '#0070eb'
  on-primary-container: '#fefcff'
  inverse-primary: '#adc6ff'
  secondary: '#006e28'
  on-secondary: '#ffffff'
  secondary-container: '#6ffb85'
  on-secondary-container: '#00732a'
  tertiary: '#894d00'
  on-tertiary: '#ffffff'
  tertiary-container: '#ac6300'
  on-tertiary-container: '#fffbff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d8e2ff'
  primary-fixed-dim: '#adc6ff'
  on-primary-fixed: '#001a41'
  on-primary-fixed-variant: '#004493'
  secondary-fixed: '#72fe88'
  secondary-fixed-dim: '#53e16f'
  on-secondary-fixed: '#002107'
  on-secondary-fixed-variant: '#00531c'
  tertiary-fixed: '#ffdcbf'
  tertiary-fixed-dim: '#ffb874'
  on-tertiary-fixed: '#2d1600'
  on-tertiary-fixed-variant: '#6a3b00'
  background: '#faf9fe'
  on-background: '#1a1b1f'
  surface-variant: '#e3e2e7'
typography:
  large-title:
    fontFamily: Inter
    fontSize: 34px
    fontWeight: '700'
    lineHeight: 41px
  large-title-mobile:
    fontFamily: Inter
    fontSize: 28px
    fontWeight: '700'
    lineHeight: 34px
  title-1:
    fontFamily: Inter
    fontSize: 28px
    fontWeight: '700'
    lineHeight: 34px
  title-2:
    fontFamily: Inter
    fontSize: 22px
    fontWeight: '700'
    lineHeight: 28px
  title-3:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 25px
  headline:
    fontFamily: Inter
    fontSize: 17px
    fontWeight: '600'
    lineHeight: 22px
  body:
    fontFamily: Inter
    fontSize: 17px
    fontWeight: '400'
    lineHeight: 22px
  callout:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 21px
  subhead:
    fontFamily: Inter
    fontSize: 15px
    fontWeight: '400'
    lineHeight: 20px
  footnote:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  caption-1:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
  caption-2:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '600'
    lineHeight: 13px
  live-time-primary:
    fontFamily: JetBrains Mono
    fontSize: 18px
    fontWeight: '700'
    lineHeight: 22px
  live-time-delay:
    fontFamily: JetBrains Mono
    fontSize: 14px
    fontWeight: '700'
    lineHeight: 18px
  platform-badge:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '700'
    lineHeight: 16px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  spacing-4xs: 2px
  spacing-3xs: 4px
  spacing-2xs: 8px
  spacing-xs: 12px
  spacing-sm: 16px
  spacing-md: 20px
  spacing-lg: 24px
  spacing-xl: 32px
  spacing-2xl: 40px
  screen-margin-phone: 16px
  screen-margin-pad: 24px
  safe-bottom-tab: 84px
  card-gutter: 12px
---

## Brand & Style
This design system provides an authentic, system-native iOS experience engineered specifically for local and nationwide German public transit (ÖPNV / Fernverkehr), with focused density and operational precision for North Rhine-Westphalia's multi-modal transit networks (VRR, VRS, WestfalenTarif, AVV).

### Philosophy & Character
- **System Integrity:** Adheres strictly to Apple Human Interface Guidelines (HIG), utilizing dynamic visual materials, tactile SF Pro typography scales, and native gesture physics.
- **Cognitive Clarity Under Pressure:** Commuters check transit apps on packed platforms, in blinding outdoor sunlight, or during frantic tight-connection transfers. The UI favors structural precision, micro-information density, and uncompromising glanceability over decorative noise.
- **Multi-Modal German Transit Authenticity:** Recognizable visual signatures for nationwide rail categories (RE/RB, S-Bahn, U-Bahn, Tram, Bus) balanced against native iOS controls to eliminate cognitive load.
- **Real-Time Operational Honesty:** Live delay statuses, platform alterations, track inversions, and cancellations are treated as mission-critical alerts requiring high-contrast semantic signaling.

## Colors
The palette balances Apple System standards with standard German VDV/ÖPNV transit conventions.

### iOS System Base
- **Primary Accent (`#007AFF`):** Apple System Blue. Used for interactive states, primary active tab items, CTAs, navigation affordances, and selected routes.
- **Secondary (`#34C759`):** Apple System Green. Used for on-time live departure indicators (`+0`), punctuality confirmations, and validated digital tickets.
- **Tertiary (`#FF9500`):** Apple System Amber/Orange. Used for minor/moderate delays (`+1` to `+5`), route alerts, and capacity warnings.
- **System Destructive (`#FF3B30`):** Apple System Red. Used for heavy delays (`>+5`), cancellations, strike alerts (*Streik*), and invalid trip segments.

### Modal Line Badges & Brand Palette
Transit lines utilize high-contrast, standardized German transit classification colors:
- **Regionalbahn / Regional-Express (DB/RRX):** `#BA1B1D` (Deep Crimson) with `#FFFFFF` text.
- **S-Bahn:** `#008D43` (Stammstrecke S-Bahn Forest Green) with `#FFFFFF` text.
- **U-Bahn / Stadtbahn:** `#005A9C` (Classic U-Bahn Blue) with `#FFFFFF` text.
- **Tram / Straßenbahn:** `#E35205` (Signal Orange/Red) with `#FFFFFF` text.
- **Bus / Schnellbus (SB):** `#6F2C91` (Transit Purple) with `#FFFFFF` text.
- **Fähre (Water Transit):** `#009AA6` (Aqua Teal) with `#FFFFFF` text.

### Neutral Layering & Adaptive Surfaces
- **System Background (Light):** `#F2F2F7` (Secondary System Background) for view grouping; `#FFFFFF` for primary cards.
- **System Background (Dark):** `#000000` (Pure Black for OLED efficiency); `#1C1C1E` for primary grouped surface cards; `#2C2C2E` for elevated sub-cards.
- **Labels & Separators:** Dynamic Apple System Grays (`#000000` primary text at 100% opacity; `#3C3C43` secondary text at 60% opacity; `#3C3C43` tertiary at 30% opacity; separator line `#3C3C43` at 29% light / `#545458` at 60% dark).

## Typography
This typography system mirrors Apple's SF Pro hierarchy using dynamic web alternates (`Inter` for functional grotesque system tracking and `JetBrains Mono` for tabular real-time departure boards and delay indicators).

### Tabular Numbers & Monospace Digits
- **Live Board Alignment:** Departure schedules, real-time minute countdowns (`In 4 Min.`, `14:32`), platform identifiers (`Gleis 4 C-E`), and delays (`+12`) must always render with tabular numbers (`tnum` OpenType feature or `JetBrains Mono`) to prevent layout jittering during live polling refreshes.
- **Cancelled Indicators:** Cancelled departures (*Fahrt fällt aus*) render in red using `live-time-primary` with a horizontal `line-through` stroke, accompanied by a `caption-2` badge specifying alternative connections.
- **Track Changes:** Modified platforms (`Gleis 2 statt 5`) utilize high-contrast bold callouts with directional pill accents.

## Layout & Spacing
The layout relies on a strict iOS-native rhythm that accommodates standard safe areas, floating dynamic blur chrome, and high-density departure boards.

### iOS Spatial Mechanics
- **Horizontal Screen Margins:** Fixed 16px safe gutters on phone screens; 24px inset margins on iPads and split-views.
- **Vertical Stack Cadence:** Spacing follows an 8pt base grid (with half-increments of 4px and 12px for compact transit listings).
- **Safe Area Insets:** Top clearance conforms to standard navigation bar heights (44px standard, 96px collapsed large title). Bottom clearance retains an 84px content inset to clear the persistent frosted tab bar and Home Indicator bar.
- **Card-to-Card Gutters:** 12px vertical spacing between route summary cards prevents claustrophobia while displaying up to 4 full itinerary cards on a single standard iPhone viewport.

## Elevation & Depth
Depth adheres to native iOS principles: content travels behind translucent materials, and elevation is dictated by frosted dynamic blurs and grouped surface layers rather than heavy drop shadows.

### Materials & Translucency
- **Navigation Bar & Tab Bar:** Backdrop blur (`UIBlurEffectStyleSystemChromeMaterial` equivalent: 20px blur radius, 80% saturation boost, 75% fill opacity). Map layers and route traces remain subtly visible beneath scrolling bars.
- **Departure & Trip Cards:** Solid opaque surface layers (`#FFFFFF` in light mode, `#1C1C1E` in dark mode) paired with micro-ambient border strokes (`rgba(0, 0, 0, 0.04)` light / `rgba(255, 255, 255, 0.08)` dark) rather than heavy drop shadows.
- **Floating Modals & Station Sheets:** iOS Apple Maps-style half/full detent bottom sheets elevated using a subtle diffused ambient shadow: `offset-y: 10px`, `blur: 30px`, `color: rgba(0, 0, 0, 0.12)`.
- **Sub-tier Cards (Inner Transfers / Steps):** Nested within primary cards on a secondary grouped surface (`#F2F2F7` light / `#2C2C2E` dark) with 0px shadow for clear parent-child encapsulation.

## Shapes
Corner geometry uses continuous squircle curvature (`continuous` corner smoothing) consistent with iOS Human Interface Guidelines.

### Radius Assignments
- **Primary Container & Trip Cards:** 16px to 20px radius (`rounded-lg` to `rounded-xl`).
- **Transit Line Badges (RE, S-Bahn, U-Bahn):** 6px to 8px radius for compact legibility, maintaining standard transport icon ratios.
- **Live Status Pills & Platform Chips:** Full capsule / pill shape (`rounded-full` / 9999px) for departure countdowns and delay warnings.
- **Segmented Controls & Input Shells:** 10px continuous radius.
- **Interactive Action Buttons (Primary CTAs):** 12px continuous radius or full pill configuration for floating map controls.

## Components

### 1. Transit Line Badges & Badges
- **Form:** Compact, solid-fill rectangles with 6px rounded corners and high-contrast bold identifiers.
- **Styles:**
  - *Regional-Express / Bahn:* Solid crimson `#BA1B1D` with white text (e.g., `RE 1`, `RB 33`).
  - *S-Bahn:* Solid green `#008D43` with white text or circled `S` glyph (e.g., `S 6`, `S 11`).
  - *U-Bahn / Stadtbahn:* Solid `#005A9C` blue badge with white bold text (e.g., `U 79`, `U 35`).
  - *Tram:* Solid `#E35205` orange pill or box (e.g., `107`, `Tram 701`).
  - *Bus:* Solid `#6F2C91` purple box with white text (e.g., `SB 50`, `Bus 752`).

### 2. Live Departure Rows & Trip Cards
- **Card Surface:** 16px radius, background grouped layer, 16px internal padding.
- **Header:** Line badge pinned top-left, destination station in `headline` style (truncating tail), right-aligned scheduled time (`live-time-primary`) alongside the live real-time delay pill:
  - *On-time:* Green text (`+0`) or emerald live beacon.
  - *Delayed:* Amber/Red pill (`+7`) with scheduled time struck through.
  - *Cancelled:* Heavy strike-through red line across scheduled time and platform, paired with `Fahrt fällt aus` badge.
- **Sub-info:** Track indicator (`Gleis 4`), route transfer count (`2 Umstiege`), and real-time occupancy pictogram (1–3 human figures representing low, medium, or high load).

### 3. iOS Segmented Controls
- **Style:** Native iOS sliding segmented tab with inset background (`#767680` at 12% opacity) and 8px inner pill container (`#FFFFFF` in light, `#636366` in dark).
- **Options:** 
  - Transit mode toggles: `Alle`, `Bahn & S-Bahn`, `U-Bahn & Tram`, `Bus`.
  - Departure vs. Arrival switches: `Abfahrt`, `Ankunft`.

### 4. iOS Bottom Tab Bar & Interactive Floating Controls
- **Tab Bar:** Frosted dynamic blur with 5 standard tabs: `Verbindungen` (Route arrow), `Abfahrten` (Clock/Station icon), `Karten` (Map pin), `Tickets` (Ticket wallet), `Favoriten` (Star). Selected state tinted in `#007AFF`.
- **Search & Input Bars:** Native inset search bar with dynamic search glass icon, clear button (`xmark.circle.fill`), and microphone or QR-scanner access buttons.
- **Transfer Step Lists:** Vertical transit line progress rails connecting stops with 3px solid colored modal strokes, hollow transfer circles, and animated live vehicle location dots.