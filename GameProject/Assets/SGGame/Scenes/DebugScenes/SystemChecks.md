# 基盤システムの確認

1. Unityのスクリプト読み込み完了を待つ。検証用PopupとAddressables設定はEditor側で未作成分だけ生成する。生成されない場合は `SGGame > Debug > Prepare System Checks` を実行する。
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

入力停止中でも検証用ボタンは操作できる。Popup中は背景メニューを操作しない。基盤側の独自非同期処理を強制停止する機能は追加していない。

検証用アセットのアドレスは `Debug/SystemCheckPopup`、生成先は同フォルダーの `DebugSystemCheckPopup.prefab`。既存のPrefab・Addressables登録は上書きしない。既存プロジェクトのPlay Mode Scriptが「Use Existing Build」の場合やプレイヤーで確認する場合は、Addressablesのコンテンツを通常の手順でビルドする。

サウンド・TitleSceneはこの確認の対象外。コンパイルや代替環境での検証だけでは、実入力・表示・Addressablesロードの成功を確定できないため、上記のPlay確認結果と区別する。
