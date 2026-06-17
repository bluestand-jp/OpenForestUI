> 🌐 **English** ・ [日本語](README-ja.md)

# Jest transformers

Custom Jest transformers wired in via the `jest.transform` map in [`../../package.json`](../../package.json). They let tests import stylesheets and binary assets without crashing.

## Contents

| File | Role |
| --- | --- |
| `cssTransform.js` | Turns `*.css` imports into an empty `module.exports = {}` so style imports are inert in tests. |
| `fileTransform.js` | Turns asset imports into their filename string; for `*.svg` it also exports a `ReactComponent` stub (mirroring SVGR). |
