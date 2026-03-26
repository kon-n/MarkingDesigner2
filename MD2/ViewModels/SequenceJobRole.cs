namespace MarkingDesigner.ViewModels
{
    /// <summary>
    /// シーケンス文字列で挿入可能なコマンド種別。
    /// </summary>
    public enum SequenceJobRole
    {
        // Job 参照系
        PositionReference,   // P
        CharacterReference,  // C
        MarkingExecute,      // J
        TextJoin,            // F
        QrCode,              // Q
        DataMatrix,          // D
        MicroQr,             // U

        // その他
        CircleMove,          // A
        OptimizeMove,        // B
        SelectPen,           // E
        OutputMarkedString,  // G
        Home,                // H
        SystemInfo,          // I
        IncrementDisable,    // K
        MoveToStart,         // M
        FoldDigits,          // N
        SelectFont,          // O
        Reader,              // R
        LineMove,            // S
        OverwriteCount,      // T
        Pause                // W
    }
}
