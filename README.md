# 🎵 SoundSystem

**Unity用のサウンド管理システムです。**


&nbsp;
## ✅ 特徴

- ファイル名の指定のみで簡単再生  
- フェードイン・アウト、パンニングなどの各種パラメーターをメソッドチェーンで指定可能  
- BGM・SE・環境音など、分類ごとのボリュームコントロール対応  
- Resources / Addressables 両対応
- オーディオファイルの指定部分をループ可能（イントロループ）


&nbsp;
## 📦 導入方法

1. Unity メニューから `Window > Package Manager` を開く  
2. 左上の `＋` ボタンから **[Install package from git URL...]** を選択  
3. 以下のURLを入力してインストール（※依存：**[UniTask](https://github.com/Cysharp/UniTask)**）  

    ```
    https://github.com/Acfeel/SoundSystem.git
    ```

4. Unity メニュー `Tools > SoundSystem > Create Settings Asset` を選択してセットアップを完了


&nbsp;
## 🎧 使い方

### 🔊 オーディオファイルの配置

- **Addressablesを利用する場合**  
  ファイル名で登録（パスは不可）

- **Resourcesを利用する場合**  
  以下のように各フォルダに格納します：  
  `Resources/SoundSystem/Bgm/BGM.ogg`  
  `Resources/SoundSystem/Se/効果音.ogg`  
  `Resources/SoundSystem/Amb/環境音.ogg`  


&nbsp;
### ▶️ 再生

```csharp
using Acfeel.SoundSystem;
```
```csharp
SoundSystem.PlayBgm("ファイル名"); // BGM再生  
SoundSystem.PlaySe("ファイル名");  // 効果音再生  
SoundSystem.PlayAmb("ファイル名"); // 環境音再生
```

### ⏹ 停止
```csharp
var bgm = SoundSystem.PlayBgm("ファイル名"); // 再生時にキャッシュしておく。
bgm.Stop();
```

### 🔉 音量指定
```csharp
SoundSystem.PlaySe("ファイル名", 0.8f); // 8割のボリュームで効果音を再生します。
```

### ・プリロード
```csharp
// Addressables利用時
SoundSystem.LoadForAddressables("ファイル名");

// Resources利用時
SoundSystem.LoadBgm("ファイル名");
SoundSystem.LoadSe("ファイル名");
SoundSystem.LoadAmb("ファイル名");

// Resources利用時 全ファイル一括読み込み
SoundSystem.LoadAllForResources();
```

&nbsp;
## 🎧 サウンド効果

### ・フェードイン
```csharp
SoundSystem.PlayBgm("ファイル名").FadeIn(5f); // 5秒かけてBGMがフェードインします。
```

### ・フェードアウト
```csharp
var bgm = SoundSystem.PlayBgm("ファイル名"); // 再生時にキャッシュしておく。
bgm.FadeOut(5f); // 5秒かけてBGMがフェードアウトします。
```

### ・パン（左右の音量バランス）の設定
```csharp
// 右からのみ再生
SoundSystem.PlaySe("ファイル名").SetPan(1f);
```

### ・ピッチ（音の高さ）の設定
```csharp
// 2倍の高さで再生
SoundSystem.PlaySe("ファイル名").SetPitch(2f);
```

### ・遅延再生
```csharp
// 3秒後に再生開始
SoundSystem.PlaySe("ファイル名").Delay(3f);
```

### ・イントロループ
```csharp
// 5秒地点から10秒地点までをループ再生
SoundSystem.PlayBgm("ファイル名").SetIntroLoop(5f, 10f);

// BPM120で4秒地点から8拍分ループ再生
SoundSystem.PlayBgm("ファイル名").SetIntroLoopBpm(120f, 4f, 8f);
```

&nbsp;
## 🎧 その他の設定

### ・グローバルボリュームの設定
```csharp
// 全体の音量を50%に設定
SoundSystem.SetGlobalVolume(SoundType.Master, 0.5f);

// BGM全体の音量を50%に設定
SoundSystem.SetGlobalVolume(SoundType.Bgm, 0.5f);
```

### ・ミュート
```csharp
// 全てのサウンドをミュート
SoundSystem.SetMute(true);

// 全てのミュートを解除
SoundSystem.SetMute(false);
```

