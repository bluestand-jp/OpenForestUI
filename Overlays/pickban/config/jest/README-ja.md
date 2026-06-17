> 🌐 [English](README.md) ・ **日本語**

# Jest トランスフォーマ

[`../../package.json`](../../package.json) の `jest.transform` マップ経由で組み込まれるカスタム Jest トランスフォーマ。テストがスタイルシートやバイナリアセットをクラッシュせずにインポートできるようにする。

## Contents

| File | Role |
| --- | --- |
| `cssTransform.js` | `*.css` のインポートを空の `module.exports = {}` に変換し、テスト中のスタイルインポートを無害化する。 |
| `fileTransform.js` | アセットのインポートをそのファイル名文字列に変換する。`*.svg` の場合は `ReactComponent` スタブも併せてエクスポートする（SVGR を模倣）。 |
