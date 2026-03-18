using System;
using MarkingDesigner.Models;

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

    // シーケンスへの Job 追加
    public class AddJobToSequenceCommand : IEditCommand
    {
        private readonly SequenceViewModel _sequence;
        private readonly JobViewModel _job;
        private int _insertIndex;

        public AddJobToSequenceCommand(SequenceViewModel sequence, JobViewModel job, int? index = null)
        {
            _sequence = sequence;
            _job = job;
            _insertIndex = index ?? -1;
        }

        public void Execute()
        {
            if (_insertIndex < 0 || _insertIndex > _sequence.Jobs.Count)
                _insertIndex = _sequence.Jobs.Count;
            if (!_sequence.Jobs.Contains(_job))
                _sequence.Jobs.Insert(_insertIndex, _job);
        }

        public void Undo()
        {
            if (_insertIndex >= 0 && _insertIndex < _sequence.Jobs.Count)
                _sequence.Jobs.RemoveAt(_insertIndex);
        }
    }

    // シーケンスからの Job 削除
    public class RemoveJobFromSequenceCommand : IEditCommand
    {
        private readonly SequenceViewModel _sequence;
        private readonly JobViewModel _job;
        private int _oldIndex;

        public RemoveJobFromSequenceCommand(SequenceViewModel sequence, JobViewModel job)
        {
            _sequence = sequence;
            _job = job;
        }

        public void Execute()
        {
            _oldIndex = _sequence.Jobs.IndexOf(_job);
            if (_oldIndex >= 0)
                _sequence.Jobs.RemoveAt(_oldIndex);
        }

        public void Undo()
        {
            if (_oldIndex >= 0 && _oldIndex <= _sequence.Jobs.Count)
                _sequence.Jobs.Insert(_oldIndex, _job);
        }
    }
}

