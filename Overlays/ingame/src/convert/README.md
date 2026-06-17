> 🌐 **English** ・ [日本語](README-ja.md)

# Browser conversion / query helpers

Small browser-side utilities that adapt environment input for the overlay. Currently
just URL/query parsing used during scene boot.

## Contents

- [`windowUtils.ts`](windowUtils.ts) — `WindowUtils.GetQueryVariable(name)` reads a
  value from the page's `?query=` string. `IngameScene.preload` uses it to read the
  `backend` param (the backend host), defaulting to `localhost`.
