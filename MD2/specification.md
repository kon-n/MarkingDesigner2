## MarkingDesigner2 仕様書（ドラフト）

### 1. Job 構造（MarkingSheet 互換マッピング）

| MarkingSheet 項目 | 説明 | MD2 プロパティ | 型 | 備考 |
|-------------------|------|----------------|----|------|
| Charactor         | 刻印文字列 | `JobViewModel.Marking.Text` | string | 旧 MarkingDesigner の `Marks.Text` 相当 |
| Size              | 文字高さ [mm] | `JobViewModel.Height` | double | |
| Start X           | 開始点 X [mm] | `JobViewModel.StartX` | double | キャンバス左端基準 |
| Start Y           | 開始点 Y [mm] | `JobViewModel.StartY` | double | キャンバス下端基準（WPF 上は反転） |
| Pitch X           | 字間ピッチ X [mm] | `JobViewModel.PitchX` | double | |
| Pitch Y           | 字間ピッチ Y [mm] | `JobViewModel.PitchY` | double | |
| Speed             | マーキング速度 | `JobViewModel.Speed` | double | 100mm/s 単位で丸め済み |
| Direction         | 文字列の方向 (0–7) | `JobViewModel.RotationType` | int | 8方向を整数で管理 |
| Arc X             | 円弧中心 X | `JobViewModel.ArcCenterX` | double | |
| Arc Y             | 円弧中心 Y | `JobViewModel.ArcCenterY` | double | |
| Arc Pitch Degree  | 円弧のピッチ角度 | `JobViewModel.ArcAnglePitch` | double | |
| Arc AX            | 3点入力 A 点 X | `JobViewModel.ArcAx` | double | 新規追加 |
| Arc AY            | 3点入力 A 点 Y | `JobViewModel.ArcAy` | double | 新規追加 |
| Arc BX            | 3点入力 B 点 X | `JobViewModel.ArcBx` | double | 新規追加 |
| Arc BY            | 3点入力 B 点 Y | `JobViewModel.ArcBy` | double | 新規追加 |
| Arc CX            | 3点入力 C 点 X | `JobViewModel.ArcCx` | double | 新規追加 |
| Arc CY            | 3点入力 C 点 Y | `JobViewModel.ArcCy` | double | 新規追加 |
| Option            | 各種オプションコード | （未割当） | - | 今後ビットマップで展開予定 |
| Increment 設定    | 連番設定一式 | `MarkingData.IsIncrement`, `MarkingData.IncrementInfo` | bool, `IncrementInfo` | 旧 MarkingDesigner の `IncrementProperty` 相当 |
| Calendar 設定     | カレンダー設定一式 | `MarkingData.CalendarInfo` | `CalendarInfo` | 旧 MarkingDesigner の `DateTimeProperty` 簡易版 |

`IncrementInfo` / `CalendarInfo` の詳細仕様は、MarkingSheet のカウンタ／カレンダーテーブルを参照しながら今後拡張する。

---

### 1.2 Sequence 構造（ドラフト）

MarkingSheet の Sequence ページと互換をとるため、シーケンス自体にも以下のプロパティを持つ。

| 項目名 | 説明 | MD2 プロパティ | 型 | 備考 |
|--------|------|----------------|----|------|
| No. | シーケンス番号 | `SequenceViewModel.SequenceId` | int | 1～MaxSeq |
| シーケンス文字列 | シーケンス名／識別用文字列（最大36文字） | `SequenceViewModel.Title` | string | 文字数上限は UI／バリデーションで制御 |
| コメント | 任意コメント | （将来追加予定）`SequenceViewModel.Comment` | string | MarkingSheet のコメント列に相当 |

Sequence に紐づく Job の並び順・内容は、`SequenceViewModel.Jobs`（`ObservableCollection<JobViewModel>`）で保持する。

---

### 1.3 シーケンス文字列（コマンド）仕様（インプット用メモ）

シーケンス文字列は、以下の 1 文字コマンド＋サブコマンド／引数で構成される。  
※ここでは MarkingSheet からのインプット内容をそのまま記録し、正式な構文定義は後続で整備する。

| コマンド | 名称 | 仕様概要 | 備考 | サンプル |
|----------|------|----------|------|----------|
| A | 円 | 指定した J 番号の開始点座標と円弧中心点から円移動 |  | `A1` |
| B | 時短 | ペン移動の最適化【VM1250のみ】 |  | `F#B/1-2#` |
| C | キャラクタ | 指定 J 番号の文字列引用 |  | `P1/C2` |
| D | Data Matrix | 指定 J 番号を Data Matrix 化して刻印 |  | `P1/V4/D2` |
| E | 使用ペン指定 | マルチペンの使用するペン番号を指定【VM1250のみ】 |  | `E1/P1/F1` |
| F | 文字列 | 指定 J 番号の文字列引用、連結可能 |  | `P1/F2` |
| G | マーキング文字列出力 | マーキングした文字列をシリアル出力 |  | `J1/G` |
| H | 原点復帰 | 原点復帰実行 |  | `H` |
| I | システム情報取得 | 指定したアドレスのシステム情報を刻印 |  |  |
| J | ジョブ番号 | 指定 J 番号を刻印実行 |  | `J1` |
| K | インクリメント無効 | 指定 J 番号のインクリメント無効 |  | `P1/F#K/1-3#` |
| L | － | （未使用） |  |  |
| M | ムーブ | 指定 J 番号の開始点に移動 |  | `M1/J2` |
| N | 桁数 | マルチペンの折り返しの桁数を指定【VM1250のみ】 |  | `F#N1/1-2#` |
| O | フォント指定 | フォントの指定 |  | `O1/J1/O2/J2` |
| P | パラメータ | 指定 J 番号のパラメータ引用（キャラクタ以外） |  | `P1/F1` |
| Q | QRcode | 指定 J 番号を QRcode 化して刻印 |  | `P1/V4/Q1` |
| R | リーダー | リーダーで読み取り実行【検証機能】 |  |  |
| S | 直線 | 指定した J 番号の開始点座標からピッチ座標まで直線移動 |  | `S1` |
| T | 上書き回数 | 指定した回数、刻印を上書きする【検証機能】 |  |  |
| U | マイクロQRcode | 指定 J 番号を microQRcode 化して刻印 |  | `P1/V1/U2` |
| V | シンボルサイズ指定 | 2 次元コードのサイズ指定 |  | `P1/V4/D1` |
| W | 一時停止 | 動作の一時停止 |  | `J1/W/J2` |
| X | － | （未使用） |  |  |
| Y | － | （未使用） |  |  |
| Z | － | （未使用） |  |  |

記号の役割:

- `/` … コマンド・サブコマンドの区切り。
- `#` … サブコマンドの開始・終了。
- `＋` … マクロコード。`[ ) > RS ● GS ▲ GS ■ RS EOT ]` を付与するマクロ。  
  例: `Ｄ# + ● /▲-■#`, `P1/V6/D#+5/1-3#`

※ 正式な BNF／パーサ仕様は、MarkingSheet 実装（Form1.vb）を精査しつつ後続で追記する。

---

### 2. 編集履歴と Undo/Redo 仕様（ドラフト）

- **目的**: Job 内の変更および Sequence 内の構成変更（Job の追加・削除・順序変更）を、ユーザー操作単位で Undo/Redo 可能にする。

#### 2.1 層構造

- **UI 層**: WPF / MVVM。  
  - `MainViewModel.UndoCommand` / `RedoCommand` から履歴マネージャを呼び出す。
- **履歴管理層**: メモリ内のコマンドスタック。  
  - `EditHistoryManager`  
    - `Do(IEditCommand command)` … 操作実行＋ Undo スタックに積む。Redo スタックをクリア。  
    - `Undo()` / `Redo()` … 直近コマンドの `Undo()` / `Execute()` を呼び分ける。  
    - `CanUndo` / `CanRedo` … メニューの有効・無効判定用。
- **ドメイン操作層**: Job / Sequence 編集用のコマンドオブジェクト。  
  - `IEditCommand` … `Execute()` と `Undo()` を持つ。

#### 2.2 対象となる編集操作（第1段階）

- **Sequence 内の変更**
  - `AddJobToSequenceCommand`  
    - 役割: `SequenceViewModel.Jobs` への `JobViewModel` 追加。  
    - Undo 時: 追加時のインデックス位置から削除して元に戻す。
  - `RemoveJobFromSequenceCommand`  
    - 役割: 現在のシーケンスから `JobViewModel` を削除。  
    - Undo 時: 削除前のインデックスに再挿入。

- **Job 内の変更**
  - 設計のみ実装済み: `ChangeJobPropertyCommand<T>`  
    - 役割: 任意の `JobViewModel` プロパティ（例: `StartX`, `StartY`, `Height` など）の「旧値／新値」を保持し、  
      `Execute()` で新値を適用、`Undo()` で旧値に戻す。  
    - 実運用では、ドラッグや編集確定時に `History.Do(new ChangeJobPropertyCommand<...>(...))` を呼ぶことで Undo 対象とする。

#### 2.3 保存・SQLite との関係

- Undo/Redo は**純粋にメモリ内の履歴**で実装し、SQLite などの永続層には依存しない。  
  - 理由: クリックやドラッグのたびに DB を触らず、応答速度を優先するため。
- 永続化（プロジェクト保存）や将来的な編集履歴ログは、別途 SQLite 層で扱う想定とし、  
  **Undo/Redo のたびに永続層を更新しない**ことを原則とする。

---

### 3. 装置情報（DeviceSpec）構造（ドラフト）

- **目的**: MarkingSheet の `SelectModel` 構造と互換のある装置情報を保持し、  
  MD2 内での範囲チェックや速度制約、ドットピッチ計算などに利用する。

#### 3.1 MarkingSheet 側のモデル構造（引用）

MarkingSheet の `SelectModel` 配列は、CSV から次の項目を読み込んでいる（`Form1.vb` より抜粋）。

- `Name` … 機種名
- `AreaX` … 加工範囲 X [mm]
- `AreaY` … 加工範囲 Y [mm]
- `MaxJob` … ジョブ最大数
- `MaxSeq` … シーケンス最大数
- `Z_Axis` … Z 軸関連設定
- `MoveInc` … 分解能（2DC ドットピッチ、推奨分解能）[mm]
- `MaxSpd` … 速度最大値（Job マーキング速度、2DC 速度、ドット速度）[mm/s]
- `MaxMrkMovSpd` … 開始点 & ピッチ移動の最大速度
- `MaxHomEscSpd` … 原点脱出速度の最大値
- `MaxHomDctSpd` … 原点検出速度の最大値
- `MaxHomStpSpd` … 原点停止速度の最大値

#### 3.2 MD2 側で保持する装置情報（DeviceSpec 案）

| 項目名 | 説明 | 型 | MarkingSheet 対応 |
|--------|------|----|--------------------|
| Device | 装置種別（VM2030, VM5030 など） | `Devices` | 接続ダイアログの機種名 |
| Name | モデル名（文字列表示用） | string | `SelectModel.Name` |
| RangeXMm | 加工範囲 X [mm] | double | `SelectModel.AreaX` |
| RangeYMm | 加工範囲 Y [mm] | double | `SelectModel.AreaY` |
| MaxJob | ジョブ最大数 | int | `SelectModel.MaxJob` |
| MaxSeq | シーケンス最大数 | int | `SelectModel.MaxSeq` |
| MoveIncMm | 推奨分解能 / 2DC ドットピッチ [mm] | double | `SelectModel.MoveInc` |
| MaxMarkSpeed | マーキング速度の最大値 [mm/s] | int | `SelectModel.MaxSpd` |
| MaxMarkMoveSpeed | 開始点 & ピッチ移動の最大速度 [mm/s] | int | `SelectModel.MaxMrkMovSpd` |
| MaxHomeEscapeSpeed | 原点脱出速度の最大値 [mm/s] | int | `SelectModel.MaxHomEscSpd` |
| MaxHomeDetectSpeed | 原点検出速度の最大値 [mm/s] | int | `SelectModel.MaxHomDctSpd` |
| MaxHomeStopSpeed | 原点停止速度の最大値 [mm/s] | int | `SelectModel.MaxHomStpSpd` |
| OriginPosition | 原点検出設置位置 (0〜3) | int | MarkingSheet の Unit/Initial 設定から後日マッピング |
| DotPitchMm | ドット刻印ピッチ [mm] | double | MarkingSheet の 2DC/Dot ページ仕様から後日マッピング |

#### 3.3 DeviceViewModel との関係（設計方針）

- `DeviceViewModel` では、現在選択中の装置に対して `DeviceSpec` を参照する。
  - `RangeX` / `RangeY` は `DeviceSpec.RangeXMm` / `RangeYMm` から取得。
  - 将来的に、原点位置・速度制約・ドットピッチなども `DeviceSpec` 経由で UI / 検証に反映する。
- `DeviceSpec` の具体的な値は、MarkingSheet における初期設定ファイル（モデルリスト）を参照して決定する。

#### 3.4 代表機種の加工範囲（メモ）

現時点で把握している各機種の加工範囲は次の通り（単位はすべて mm）。

| 機種     | RangeX[mm] | RangeY[mm] |
|----------|------------|------------|
| VM6040   | 100        | 100        |
| VM2020   | 60         | 30         |
| VM1200   | 80         | 30         |
| VM2140   | 30         | 15         |
| VM3710A  | 100        | 100        |
| VM3730A  | 100        | 100        |
| VM3770A  | 100        | 100        |
| VM1210AS1| 160        | 30         |
| VM1250   | 80         | 30         |
| VM-M1    | 60         | 30         |

※ 上記は MarkingSheet / 実機仕様から共有された値であり、`DeviceSpec` 実装時の初期値として利用する。

---

### 4. 装置 EEPROM マップ（BR24G32）インプット

装置が保持する BR24G32 EEPROM のアドレスと情報の対応は次の通り。  
※ここでは既存仕様からのインプットをそのまま記録し、MD2 でどこまで参照するかは後続で決定する。

| アドレス | 定義名 | 内容 |
|----------|--------|------|
| 0x0000 | `EEP_CPU_BOARD_VER_ADDRESS` | CPUボードバージョン |
| 0x0010 | `EEP_SERIAL_ADDRESS` | シリアル番号 |
| 0x0020 | `EEP_PRODUCT_NO_ADDRESS` | 製造番号 |
| 0x0030 | `EEP_PRODUCT_DATE_ADDRESS` | 製造日 |
| 0x0040 | `EEP_DESTINATION_ADDRESS` | 出荷先 |
| 0x0050 | `EEP_COMMENT_ADDRESS` | サービスコード、コメント |
| 0x0060 | `EEP_EXPAND_BOARD_ADDRESS` | 外部接続ボードのタイプ（PIO、モーターなど） |
| 0x0070 | `EEP_MODEL_ADDRESS` | 機器名（機構部名 MM1など） |
| 0x0080 | `EEP_OPTION_ADDRESS` | オプション(円弧、カレンダーなど） |
| 0x0090 | `EEP_PENTYPE_ADDRESS` | ペンタイプ |
| 0x00A0 | `EEP_ZTYPE_ADDRESS` | Z軸タイプ(検討、予約) |
| 0x00B0 | `EEP_MOTTYPE_ADDRESS` | モータタイプ(検討、予約) |
| 0x00C0 | - | - |
| 0x00D0 | - | - |
| 0x00E0 | - | - |
| 0x00F0 | `EEP_DEBUG_ADDRESS` | デバッグ設定(検討、予約) |
| 0x0100 | `EEP_PRG_VER_ADDRESS` | プログラムバージョン番号 |
| 0x0110 | `EEP_PRG_BGS_ADDRESS` | プログラムバージョン番号 |
| 0x0120 | `EEP_PRG_TIM_ADDRESS` | 更新日時 |
| 0x0130 | `EEP_FONTA_VER_ADDRESS` | フォントAバージョン番号(BGS番号) |
| 0x0140 | `EEP_FONTA_TIM_ADDRESS` | 更新日時 |
| 0x0150 | `EEP_FONTB_VER_ADDRESS` | フォントBバージョン番号(BGS番号) |
| 0x0160 | `EEP_FONTB_TIM_ADDRESS` | 更新日時 |
| 0x0170 | `EEP_FONTC_VER_ADDRESS` | フォントCバージョン番号(BGS番号) |
| 0x0180 | `EEP_FONTC_TIM_ADDRESS` | 更新日時 |
| 0x0190 | `EEP_FONTD_VER_ADDRESS` | フォントDバージョン番号(BGS番号) |
| 0x01A0 | `EEP_FONTD_TIM_ADDRESS` | 更新日時 |
| 0x01B0 | `EEP_FONTE_VER_ADDRESS` | フォントEバージョン番号(BGS番号) |
| 0x01C0 | `EEP_FONTE_TIM_ADDRESS` | 更新日時 |
| 0x01D0 | `EEP_FONTF_VER_ADDRESS` | フォントFバージョン番号(BGS番号) |
| 0x01E0 | `EEP_FONTF_TIM_ADDRESS` | 更新日時 |
| 0x01F0 | - | - |
| 0x0200 | `EEP_MKCTRL_SERIASL_ADDRESS` | 制御部シリアル番号(製造番号...) |
| 0x0210 | `EEP_MKHEAD_SERIASL_ADDRESS` | 機構部シリアル番号(製造番号...) |
| 0x0220 | `EEP_MKPEN_SERIASL_ADDRESS` | ペンシリアル番号(製造番号...) |
| 0x0230 | `EEP_MKHEAD_COMMENT_ADDRESS` | 制御部コメント(修理履歴...) |
| 0x0240 | `EEP_MKCTRL_COMMENT_ADDRESS` | 機構部コメント(修理履歴...) |
| 0x0250 | `EEP_MKPEN_COMMENT_ADDRESS` | ペンコメント(修理履歴...) |
| 0x0260 | `EEP_MARKER_NO_ADDRESS` | ユニット番号(同一工場内で管理するため) |
| 0x0270 | `EEP_SCI0_TARGET_ADDRESS` | シリアル通信(SCI0)接続機器名 RS-232C |
| 0x0280 | `EEP_SCI1_TARGET_ADDRESS` | シリアル通信(SCI1)接続機器名 USB |
| 0x0290 | `EEP_SCI2_TARGET_ADDRESS` | シリアル通信(SCI2)接続機器名 JTAG |
| 0x02A0 | `EEP_SCI3_TARGET_ADDRESS` | シリアル通信(SCI3)接続機器名 Ethernet |
| 0x02B0 | `EEP_SCI8_TARGET_ADDRESS` | シリアル通信(SCI8)接続機器名 WLAN |
| 0x02C0 | - | - |
| 0x02D0 | - | - |
| 0x02E0 | - | - |
| 0x02F0 | - | - |
| 0x0300 | `EEP_ORIGINE_POS_ADDRESS` | 原点位置 0:左手前,1:左奥,2:右奥,3:右手前 |
| 0x0310 | - | - |
| 0x0320 | - | - |
| 0x0330 | - | - |
| 0x0340 | - | - |
| 0x0350 | - | - |
| 0x0360 | - | - |
| 0x0370 | - | - |
| 0x0380 | `EEP_COMM_METHOD_ADDRESS` | 通信方式 0:RS232C,1:USB,2:Ethernet,3:WiFi |
| 0x0390 | `EEP_COMM_MONITOR_ADDRESS` | モニタ出力先：0:RS232C,1:USB |
| 0x03A0 | `EEP_COMM_BOAUD_ADDRESS` | シリアルボーレート 0:9600、1:19200、2:38400、3:57600、4:115200 |
| 0x03B0 | - | - |
| 0x03C0 | - | - |
| 0x03D0 | - | - |
| 0x03E0 | - | - |
| 0x03F0 | - | - |
| 0x0500 | `EEP_ETHERNET_HOST_ADDRESS` | Ethernetホスト(自機)IPアドレス |
| 0x0510 | `EEP_ETHERNET_CLIENT_ADDRESS` | Ethernetクライアント(接続先)IPアドレス |
| 0x0520 | `EEP_ETHERNET_GATEWAY_ADDRESS` | ゲートウェイIPアドレス |
| 0x0530 | `EEP_ETHERNET_PORT_ADDRESS` | Ethernetポート: 9000 |
| 0x0540 | `EEP_WLAN_HOST_ADDRESS` | WLANホスト(自機)IPアドレス |
| 0x0550 | `EEP_WLAN_CLIENT_ADDRESS` | WLANクライアント(接続先)IPアドレス |
| 0x0560 | `EEP_WLAN_GATEWAY_ADDRESS` | ゲートウェイIPアドレス |
| 0x0570 | `EEP_WLAN_PORT_ADDRESS` | WiFiポート: 9000 |
| 0x0580 | `EEP_WLAN_AP_ADRESS` | アクセスポイント名 |
| 0x0590 | `EEP_WLAN_PA_ADDRESS` | 接続暗号方式 |
| 0x05A0 | `EEP_WLAN_PA_ID_ADDRESS` | 接続暗号方式 ID |
| 0x05B0 | `EEP_WLAN_PA_PW_ADDRESS` | 接続暗号方式 PW |
| 0x0600 | `EEP_SPD_MIN_HOM_ADDRESS` | s: 原点復帰 初速(mm/min) |
| 0x0610 | `EEP_SPD_MAX_HOM_ADDRESS` | e: 原点復帰 移動速度(mm/min) |
| 0x0620 | `EEP_SPD_OFS_HOM_ADDRESS` | f: 原点復帰 オフセット速度(mm/min) |
| 0x0630 | `EEP_SPD_DRV_HOM_ADDRESS` | v: 原点復帰 位置決め速度(mm/min) |
| 0x0640 | `EEP_SPD_DRV2_HOM_ADDRESS` | l: 原点復帰 ドライブ速度(mm/min) |
| 0x0650 | `EEP_SPD_ACC_HOM_ADDRESS` | a: 原点復帰 加速時間(ms） |
| 0x0660 | `EEP_SPD_DEC_HOM_ADDRESS` | d: 原点復帰 減速時間(ms） |
| 0x0670 | `EEP_SPD_STR_MRK_ADDRESS` | s: 刻印初速(mm/min) |
| 0x0680 | `EEP_SPD_STR2_MRK_ADDRESS` | m: 刻印初速（開始点・ピッチ移動） |
| 0x0690 | `EEP_SPD_STR3_MRK_ADDRESS` | o: 刻印初速(2DC/DOT) |
| 0x06A0 | `EEP_SPD_MAX_MRK_ADDRESS` | e: 刻印 最高速 |
| 0x06B0 | `EEP_SPD_ACC_MRK_ADDRESS` | a: 刻印 加減速時間 |
| 0x06C0 | `EEP_SPD_ACC2_MRK_ADDRESS` | b: 刻印 加減速時間(移動) |
| 0x06D0 | `EEP_SPD_ACC3_MRK_ADDRESS` | c: 刻印 加減速時間(2DC/DOT) |
| 0x06E0 | `EEP_SPD_DRV_MRK_ADDRESS` | v: 刻印 ドライブ速度 |
| 0x06F0 | `EEP_SPD_DRV2_MRK_ADDRESS` | l: 刻印 ドライブ速度(移動) |
| 0x0700 | `EEP_SPD_DRV3_MRK_ADDRESS` | 2dc: 刻印 ドライブ速度(2DC) |
| 0x0710 | `EEP_SPD_DRV4_MRK_ADDRESS` | dot: 刻印 ドライブ速度(DOT) |

---

### 5. カレンダー対応表（ドラフト）

カレンダーマーキングでは、日時情報を「フォーマット文字列」または「カレンダーテーブル」に基づき展開する。  
ここではまず、MD2 側で想定する **フォーマット → 意味** の対応表を定義する。

#### 5.1 基本トークン対応

| トークン | 意味 | 例 | 備考 |
|----------|------|----|------|
| `%Y` | 西暦4桁 | 2026 | 0000〜9999 |
| `%y` | 西暦下2桁 | 26 | 00〜99 |
| `%m` | 月（2桁） | 03 | 01〜12 |
| `%d` | 日（2桁） | 18 | 01〜31 |
| `%H` | 時（24h, 2桁） | 09 | 00〜23 |
| `%M` | 分（2桁） | 45 | 00〜59 |
| `%S` | 秒（2桁） | 30 | 00〜59 |
| `%w` | 曜日番号 | 0 | 0:日〜6:土（案） |
| `%W` | 週番号 | 12 | 仕様要検討（週の起点など） |

※ 実際のトークンセット・表記は MarkingSheet 実装のカレンダーテーブル定義に合わせて後日調整する。

#### 5.2 MD2 側での扱い

- `MarkingData.CalendarInfo.Format` に上記トークンを含むフォーマット文字列を設定し、  
  実行時に現在日時（または指定日時）から文字列を生成する。
- カレンダーテーブル方式（年・月・日ごとに置換ルールを持つ形式）は、MarkingSheet の仕様を参照しつつ、  
  必要になったタイミングで `CalendarInfo` を拡張し、別表として定義する。

---

### 6. インクリメント仕様（ドラフト）

インクリメントは、文字列の一部を「連番」として扱う機能であり、桁数や進数・文字種に応じて複数のカウントモードを持つ。

#### 6.1 基本モード

| モード | 説明 | 例 |
|--------|------|----|
| 10進数カウント | 通常の 0〜9 の 10 進数としてカウントする。 | `0001` → `0002` → ... |
| 16進数カウント | 0〜9, A〜F の 16 進数としてカウントする。 | `0F` → `10`、`A0` → `A1` → ... → `AF` |
| 英字のみカウント | A〜Z の 26 進数としてカウントし、多桁展開に対応。 | `A`→`B`→...→`Z`→`AA`→...→`ZZ`→`AAA`→...→`ZZZ` |
| 混在モード | 1桁目のみ 10進数(0〜9)、2桁目以降は 16進数(0〜9,A〜F)でカウント。 | `09`→`0A`→...→`0F`→`10`→`11`→... |

#### 6.2 MD2 側での表現（案）

- `IncrementInfo` に以下のようなモード種別を持たせる想定：
  - `IncrementDisplay`（既存）: 数値 / 16進 / 英字 などの表示種別。
  - 追加で、桁ごとの進数モードを持つか、もしくは「プリセットとして混在モード」を選択可能にする。
- MarkingSheet の既存仕様に合わせて、桁数・ゼロ埋め・ヘッダ/フッタとの組み合わせを詳細定義し、  
  必要に応じて `IncrementInfo` に項目を追加していく。



