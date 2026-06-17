> 🌐 **English** ・ [日本語](README-ja.md)

# "Europe" layout styles

LESS stylesheets for the draft overlay, imported as CSS modules (`import css from './style/index.less'`) by the components in [`../`](../). `index.less` is the aggregator; the rest are partials it `@import`s. The class names exported here (`css.Overlay`, `css.Timer`, `css.Pick`, animation-state classes, etc.) are referenced directly in the JSX.

## Contents

| File | Role |
| --- | --- |
| `index.less` | Entry stylesheet: base `body`/box-model, middle-box/logo/patch layout, and `@import`s the partials below. |
| `variables.less` | `:root` CSS custom properties — box dimensions, crops, and team/timer/font colors (team colors are overridden at runtime from config). |
| `fonts.less` | `@font-face` declarations for the fonts in [`../../assets/fonts/`](../../assets/fonts/). |
| `animation.less` | Staged draft-reveal keyframes/state classes (`TheAbsoluteVoid`, `AnimationHidden`, `AnimationTimer`, `AnimationBansPick`, ...) sequenced by `Overlay.jsx`. |
| `timer.less` | Center draft-timer styling (blue/red active-side backgrounds, digits). |
| `team.less` | Per-team block layout (picks column, name, score, coach). |
| `picks.less` | Pick-slot styling (splash crop, summoner spells, active state, player name). |
| `bans.less` | Ban-row styling and the center ban spacer. |
