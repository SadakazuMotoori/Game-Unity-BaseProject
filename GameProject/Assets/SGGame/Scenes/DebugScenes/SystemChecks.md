# 基盤システムの確認

1. 検証機能を使う場合だけ、Unityのスクリプト読み込み完了後に `SGGame > Debug > Prepare System Checks` を実行する。検証用PopupとAddressables設定は未作成分だけ生成する。Editor起動時の自動生成は行わない。
2. `Assets/SGGame/Scenes/BootScene.unity` を開いてPlayする。`IS_PRODUCT` は無効にする。
3. 自動遷移したDebugTopSceneで、先頭の「システムチェック」をSpaceまたはEnterで開く。パネルの操作ボタンはマウスで使用する。

| 確認項目 | 操作 | 期待結果 |
| --- | --- | --- |
| 起動 | パネルを開く | 常駐Scopeが初期化済みで、必須Managerがそれぞれ1個。GAME_DEBUG時はDebugSystemManagerも含む。 |
| 入力停止の重複 | A取得 → B取得 → A解除 → B解除 | Aだけ解除しても停止中。Bも解除すると受付中に戻る。方向キー等の受付回数が停止中に増えない。逆順でも確認する。 |
| シーン遷移 | A/Bを解除し「デバッグシーン再読み込み」 | フェード後にパネルが再表示され、常駐Managerの同一性・遷移後も独立した停止要求が残ること・最後の解除をOK表示する。 |
| Popup正常終了 | 「Popupを開く」→右側の「閉じる」をSpace、ゲームパッドの決定、またはクリック | Popupが消え、選択ID `(1, close)` が表示される。 |
| Popup破棄 | 「Popupを開く」→「表示中のPopupを破棄」 | Popupが消え、未選択で終了と表示される。再び開けることも確認する。 |
| パネル終了 | A/Bを保持したまま「メニューへ戻る」 | このパネルの停止要求が解除され、元のメニューを操作できる。 |

入力停止中でも検証用ボタンは操作できる。Popup表示中は背景メニューへの入力を受け付けない。基盤側の独自非同期処理を強制停止する機能は追加していない。

`UISelectableGroup` / `UISelectable` は入力停止と前面Popupの判定を共通で使用する。独自のUIが直接 `UIAction` を読む場合は、先に `IWindowManager.Instance.CanReceiveInput(transform)` を確認する。Popupを重ねた場合は表示完了した最前面だけが操作を受け付け、前面が切り替わったフレームの入力は次の画面へ渡さない。初期選択など、コードによる `CurrentSelected` の設定は入力停止中でも行える。

検証用アセットのアドレスは `Debug/SystemCheckPopup`、生成先は同フォルダーの `DebugSystemCheckPopup.prefab`。既存のPrefab・Addressables登録は上書きしない。既存プロジェクトのPlay Mode Scriptが「Use Existing Build」の場合やプレイヤーで確認する場合は、Addressablesのコンテンツを通常の手順でビルドする。

`IS_PRODUCT` を有効にして通常の `Build` / `Build And Run` を実行すると、`DebugScenes/` 以下のシーンと `Debug System Checks` のAddressablesグループを除外する。シーン一覧はビルドに渡す配列だけを変更し、グループの設定はビルド終了時に元へ戻す。失敗時も同じ復元処理を行う。

製品ビルドでは古い検証用バンドルの混入を防ぐため、Addressablesの生成済みプレイヤーコンテンツを削除して再構築する。製品ビルド後に「Use Existing Build」で検証する場合は、Addressablesを開発用に再ビルドする。通常の「Use Asset Database」でのPlay確認はそのまま利用できる。

独自のEditor/CIビルドスクリプトは `SGGames.Game.Develop.Editor.DebugSystemCheckBuild.BuildPlayer(options)` を呼ぶ。戻り値は `BuildReport`。`IS_PRODUCT` で `BuildPipeline.BuildPlayer` を直接呼ぶ処理は、除外漏れを防ぐためビルド開始時にエラーで停止する。対象PlayerSettings、`extraScriptingDefines`、アクティブなBuildProfileの追加定義から `IS_PRODUCT` を判定する。

サウンド・TitleSceneはこの確認の対象外。コンパイルや代替環境での検証だけでは、実入力・表示・Addressablesロードの成功を確定できないため、上記のPlay確認結果と区別する。
