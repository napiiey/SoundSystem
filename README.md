# 🎵 SoundSystem

**Unity用のサウンド管理システムです。**


&nbsp;
## ✅ 特徴

- ファイル名の指定のみで簡単再生  
- フェードイン・アウト、パンニングなどの各種パラメーターを**メソッドチェーン**で指定可能  
- BGM・SE・環境音など、分類ごとの**ボリュームコントロール**対応  
- Resources / Addressables 両対応
- オーディオファイルの指定部分をループ可能（イントロループ）


&nbsp;
## 📦 導入方法

1. Unity メニューから `Window > Package Manager` を開く  
2. 左上の `＋` ボタンから **[Install package from git URL...]** を選択  
3. 以下のURLを入力してインストール（※依存：**[UniTask](https://github.com/Cysharp/UniTask)**）  

    ```
    https://github.com/napiiey/SoundSystem.git
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

### ・フェードイン
```csharp
SoundSystem.PlayBgm("ファイル名").FadeIn(5f); // 5秒かけてBGMがフェードインします。
```

### ・フェードアウト
```csharp
var bgm = SoundSystem.PlayBgm("ファイル名"); // 再生時にキャッシュしておく。
bgm.FadeOut(5f); // 5秒かけてBGMがフェードアウトします。
```

