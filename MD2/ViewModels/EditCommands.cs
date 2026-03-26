using System;

namespace MarkingDesigner.ViewModels
{
    // 単一 Job プロパティ変更
    public class ChangeJobPropertyCommand<T> : IEditCommand
    {
        private readonly JobViewModel _job;
        private readonly Action<JobViewModel, T> _setter;
        private readonly T _oldValue;
        private readonly T _newValue;

        public ChangeJobPropertyCommand(JobViewModel job, T oldValue, T newValue, Action<JobViewModel, T> setter)
        {
            _job = job;
            _oldValue = oldValue;
            _newValue = newValue;
            _setter = setter;
        }

        public void Execute() => _setter(_job, _newValue);
        public void Undo() => _setter(_job, _oldValue);
    }

    // シーケンスへのエントリ追加
    public class AddJobToSequenceCommand : IEditCommand
    {
        private readonly SequenceViewModel _sequence;
        private readonly SequenceJobEntry _entry;
        private int _insertIndex;

        public AddJobToSequenceCommand(SequenceViewModel sequence, SequenceJobEntry entry, int? index = null)
        {
            _sequence = sequence;
            _entry = entry;
            _insertIndex = index ?? -1;
        }

        public void Execute()
        {
            if (_insertIndex < 0 || _insertIndex > _sequence.Entries.Count)
                _insertIndex = _sequence.Entries.Count;
            _sequence.Entries.Insert(_insertIndex, _entry);
        }

        public void Undo()
        {
            if (_insertIndex >= 0 && _insertIndex < _sequence.Entries.Count)
                _sequence.Entries.RemoveAt(_insertIndex);
        }
    }

    // シーケンスからのエントリ削除
    public class RemoveJobFromSequenceCommand : IEditCommand
    {
        private readonly SequenceViewModel _sequence;
        private readonly SequenceJobEntry _entry;
        private int _oldIndex;

        public RemoveJobFromSequenceCommand(SequenceViewModel sequence, SequenceJobEntry entry)
        {
            _sequence = sequence;
            _entry = entry;
        }

        public void Execute()
        {
            _oldIndex = _sequence.Entries.IndexOf(_entry);
            if (_oldIndex >= 0)
                _sequence.Entries.RemoveAt(_oldIndex);
        }

        public void Undo()
        {
            if (_oldIndex >= 0 && _oldIndex <= _sequence.Entries.Count)
                _sequence.Entries.Insert(_oldIndex, _entry);
        }
    }

    // シーケンス内の順序移動
    public class MoveSequenceEntryCommand : IEditCommand
    {
        private readonly SequenceViewModel _sequence;
        private readonly int _fromIndex;
        private readonly int _toIndex;

        public MoveSequenceEntryCommand(SequenceViewModel sequence, int fromIndex, int toIndex)
        {
            _sequence = sequence;
            _fromIndex = fromIndex;
            _toIndex = toIndex;
        }

        public void Execute()
        {
            var e = _sequence.Entries[_fromIndex];
            _sequence.Entries.RemoveAt(_fromIndex);
            _sequence.Entries.Insert(_toIndex, e);
        }

        public void Undo()
        {
            var e = _sequence.Entries[_toIndex];
            _sequence.Entries.RemoveAt(_toIndex);
            _sequence.Entries.Insert(_fromIndex, e);
        }
    }
}
