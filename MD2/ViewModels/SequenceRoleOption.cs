namespace MarkingDesigner.ViewModels
{
    /// <summary>UI 用: 役割列挙子と表示ラベルの組。</summary>
    public sealed class SequenceRoleOption
    {
        public SequenceRoleOption(SequenceJobRole role, string label)
        {
            Role = role;
            Label = label;
        }

        public SequenceJobRole Role { get; }
        public string Label { get; }
    }
}
