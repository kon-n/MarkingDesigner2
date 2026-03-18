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
