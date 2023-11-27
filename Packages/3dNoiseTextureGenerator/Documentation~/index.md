# 3D Noise Texture Generator

![](images/ntg00.png)

## 概要

3次元ノイズテクスチャアセットを生成するエディタ拡張です

## 特徴
* Unity用の3Dテクスチャアセットを生成
* 豊富な生成アルゴリズム
  * フラクタルノイズ（Perlin, Billow, RidgedMulti）
  * セル分割（Volonoi, Quadratic, Manhattan, Chebychev, Mincowsky）
  * 図形（CheckerBoard, Cylinders, Spheres）
* シームレス対応

## インストール

### Package Manager からのインストール

* Package Manager → Scoped Registries に以下を登録
    * URL: https://package.openupm.com
    * Scope: jp.co.bexide
* Package Manager → My Registries から以下を選択して Install
    * BxUni 3D Noise Texture Generator

## 使い方

### 起動

UnityEditorメニュー → BeXide → Noise Texture Generator

![](images/ntg01.png)

### 設定項目

#### Texture File Name

生成するテクスチャのファイル名を指定

#### Noise Type

生成するノイズの種類を以下から指定

* Perlin
* Billow
* RidgedMulti
* CheckerBoard
* Cylinders
* Spheres
* Cell

#### Common Parameters

| 共通パラメータ       |                  |
|-------------|------------------------------------------------------------------------|
| Grid Size   | 一辺の大きさ                                                                 |
| Random Seed | ランダムシード                                                                |
| Frequency   | [空間周波数](http://libnoise.sourceforge.net/glossary/index.html#frequency) |
| Is Seamless | シームレスにする                                                               |
| Is Inverse  | 値を反転                                                                   |
| Gradient    | ノイズの値を色にマッピング                                                          |

#### Fractal Parameters

| フラクタルノイズ系パラメータ ||
|--------------|---|
| Octaves      |[オクターブ数](http://libnoise.sourceforge.net/glossary/index.html#octave)|
| Lacunarity   |[オクターブ周波数逓倍率](http://libnoise.sourceforge.net/glossary/index.html#lacunarity)|
| Persistence  |[オクターブ振幅減衰率](http://libnoise.sourceforge.net/glossary/index.html#persistence)|
| Quality      |補間品質|

#### Cell Parameters

| Cell分割系パラメータ ||
|-------------|---|
| CellType    |セル分割アルゴリズム|

### テクスチャ生成と保存

「Build」ボタンを押すとテクスチャが生成され、プレビューが表示されます。

「Clear」ボタンを押すと生成されたテクスチャを破棄します。

テクスチャ生成後、「Save」ボタンを押すと生成したテクスチャをアセットとして保存します。

### 謝辞

* ノイズ生成には以下のライブラリを使用しています
  * https://libnoise.sourceforge.net/
  * https://github.com/rthome/SharpNoise


## お問い合わせ

* 不具合のご報告は GitHub の Issues へ
* その他お問い合わせは mailto:tech-info@bexide.co.jp へ

