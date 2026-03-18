using System.Collections.Generic;

namespace MarkingDesigner.ViewModels
{
    public interface IEditCommand
    {
        void Execute();
        void Undo();
    }

    public class EditHistoryManager : BindableBase
    {
        private readonly Stack<IEditCommand> _undoStack = new();
        private readonly Stack<IEditCommand> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Do(IEditCommand command)
        {
            if (command == null) return;
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            RaiseChanged();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
            RaiseChanged();
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = _redoStack.Pop();
            cmd.Execute();
            _undoStack.Push(cmd);
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            RaisePropertyChanged(nameof(CanUndo));
            RaisePropertyChanged(nameof(CanRedo));
        }
    }
}

