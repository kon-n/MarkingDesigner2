using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkingDesigner.ViewModels
{
    public class SequenceViewModel : BindableBase
    {
        public int SequenceId { get; set; }
        public string Title { get; set; } = "";
        public ObservableCollection<SequenceJobEntry> Entries { get; } = new ObservableCollection<SequenceJobEntry>();
        public ObservableCollection<SequenceLinkSegment> LinkSegments { get; } = new ObservableCollection<SequenceLinkSegment>();

        private string _sequenceString = "";
        public string SequenceString
        {
            get => _sequenceString;
            private set => SetProperty(ref _sequenceString, value);
        }

        private readonly Dictionary<SequenceJobEntry, JobViewModel?> _entryJobs = new Dictionary<SequenceJobEntry, JobViewModel?>();
        private readonly Dictionary<JobViewModel, int> _jobSubscriptions = new Dictionary<JobViewModel, int>();

        public SequenceViewModel()
        {
            Entries.CollectionChanged += OnEntriesChanged;
        }

        private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<SequenceJobEntry>())
                    AttachEntry(item);
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<SequenceJobEntry>())
                    DetachEntry(item);
            }

            RebuildAll();
        }

        private void OnEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not SequenceJobEntry entry)
                return;

            if (e.PropertyName == nameof(SequenceJobEntry.Job))
            {
                var oldJob = _entryJobs.TryGetValue(entry, out var prev) ? prev : null;
                if (!ReferenceEquals(oldJob, entry.Job))
                {
                    if (oldJob != null) DetachJob(oldJob);
                    if (entry.Job != null) AttachJob(entry.Job);
                    _entryJobs[entry] = entry.Job;
                }
            }

            if (e.PropertyName == nameof(SequenceJobEntry.Role) ||
                e.PropertyName == nameof(SequenceJobEntry.Job) ||
                e.PropertyName == nameof(SequenceJobEntry.SymbolSizeV) ||
                e.PropertyName == nameof(SequenceJobEntry.TwoDSourceJobs) ||
                e.PropertyName == nameof(SequenceJobEntry.UiTwoDSrcLabel))
            {
                RebuildAll();
            }
        }

        private void OnJobPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(JobViewModel.StartX) ||
                e.PropertyName == nameof(JobViewModel.StartY) ||
                e.PropertyName == nameof(JobViewModel.EndX) ||
                e.PropertyName == nameof(JobViewModel.EndY))
            {
                RebuildLinkSegments();
            }
        }

        private void AttachEntry(SequenceJobEntry entry)
        {
            entry.PropertyChanged += OnEntryPropertyChanged;
            _entryJobs[entry] = entry.Job;
            if (entry.Job != null)
                AttachJob(entry.Job);
        }

        private void DetachEntry(SequenceJobEntry entry)
        {
            entry.PropertyChanged -= OnEntryPropertyChanged;
            if (_entryJobs.TryGetValue(entry, out var job) && job != null)
                DetachJob(job);
            _entryJobs.Remove(entry);
        }

        private void AttachJob(JobViewModel job)
        {
            if (_jobSubscriptions.TryGetValue(job, out int count))
            {
                _jobSubscriptions[job] = count + 1;
                return;
            }

            _jobSubscriptions[job] = 1;
            job.PropertyChanged += OnJobPropertyChanged;
        }

        private void DetachJob(JobViewModel job)
        {
            if (!_jobSubscriptions.TryGetValue(job, out int count))
                return;

            if (count <= 1)
            {
                _jobSubscriptions.Remove(job);
                job.PropertyChanged -= OnJobPropertyChanged;
            }
            else
            {
                _jobSubscriptions[job] = count - 1;
            }
        }

        private void RebuildAll()
        {
            RebuildSequenceString();
            RebuildLinkSegments();
        }

        private void RebuildSequenceString()
        {
            var parts = Entries.Select(BuildCommandToken).Where(x => !string.IsNullOrWhiteSpace(x));
            SequenceString = string.Join("/", parts);
        }

        private static readonly HashSet<SequenceJobRole> LinkTargetCommands = new HashSet<SequenceJobRole>
        {
            SequenceJobRole.MarkingExecute,      // J
            SequenceJobRole.CharacterReference,  // C
            SequenceJobRole.PositionReference,   // P
            SequenceJobRole.CircleMove,          // A
            SequenceJobRole.DataMatrix,          // D
            SequenceJobRole.TextJoin,            // F
            SequenceJobRole.Home,                // H
            SequenceJobRole.SystemInfo,          // I
            SequenceJobRole.MoveToStart,         // M
            SequenceJobRole.QrCode,              // Q
            SequenceJobRole.LineMove,            // S
            SequenceJobRole.MicroQr,             // U
        };

        private void RebuildLinkSegments()
        {
            LinkSegments.Clear();
            for (int i = 0; i < Entries.Count - 1; i++)
            {
                var current = Entries[i];
                var next = Entries[i + 1];
                if (!LinkTargetCommands.Contains(current.Role))
                    continue;

                if (!TryGetRepresentativePoint(current, out double x1, out double y1))
                    continue;
                if (!TryGetRepresentativePoint(next, out double x2, out double y2))
                    continue;

                LinkSegments.Add(new SequenceLinkSegment
                {
                    StartX = x1,
                    StartY = y1,
                    EndX = x2,
                    EndY = y2
                });
            }
        }

        private static bool TryGetRepresentativePoint(SequenceJobEntry entry, out double x, out double y)
        {
            if (entry.Job != null)
            {
                x = entry.Job.StartX;
                y = entry.Job.StartY;
                return true;
            }

            if (entry.Role == SequenceJobRole.Home)
            {
                x = 0;
                y = 0;
                return true;
            }

            x = 0;
            y = 0;
            return false;
        }

        private static string BuildCommandToken(SequenceJobEntry entry)
        {
            string cmd = ToCommandLetter(entry.Role);
            if (string.IsNullOrEmpty(cmd))
                return string.Empty;

            if (entry.Role == SequenceJobRole.QrCode ||
                entry.Role == SequenceJobRole.DataMatrix ||
                entry.Role == SequenceJobRole.MicroQr)
            {
                var payload = BuildSourcePart(cmd, entry.TwoDSourceJobs.Select(j => j.JobId));
                return $"V{entry.SymbolSizeV}/{payload}";
            }

            if (entry.Role == SequenceJobRole.TextJoin)
                return BuildSourcePart(cmd, entry.TwoDSourceJobs.Select(j => j.JobId));

            if (entry.Job != null)
                return $"{cmd}{entry.Job.JobId}";

            return cmd;
        }

        private static string BuildSourcePart(string cmd, IEnumerable<int> sourceIds)
        {
            var ids = sourceIds.Distinct().ToList();
            if (ids.Count == 0)
                return cmd;
            if (ids.Count == 1)
                return $"{cmd}{ids[0]}";

            var sb = new StringBuilder();
            sb.Append(cmd).Append('#');
            for (int i = 0; i < ids.Count; i++)
            {
                if (i > 0) sb.Append('-');
                sb.Append(ids[i]);
            }
            sb.Append('#');
            return sb.ToString();
        }

        private static string ToCommandLetter(SequenceJobRole role)
        {
            return role switch
            {
                SequenceJobRole.CircleMove => "A",
                SequenceJobRole.OptimizeMove => "B",
                SequenceJobRole.CharacterReference => "C",
                SequenceJobRole.DataMatrix => "D",
                SequenceJobRole.SelectPen => "E",
                SequenceJobRole.TextJoin => "F",
                SequenceJobRole.OutputMarkedString => "G",
                SequenceJobRole.Home => "H",
                SequenceJobRole.SystemInfo => "I",
                SequenceJobRole.MarkingExecute => "J",
                SequenceJobRole.IncrementDisable => "K",
                SequenceJobRole.MoveToStart => "M",
                SequenceJobRole.FoldDigits => "N",
                SequenceJobRole.SelectFont => "O",
                SequenceJobRole.PositionReference => "P",
                SequenceJobRole.QrCode => "Q",
                SequenceJobRole.Reader => "R",
                SequenceJobRole.LineMove => "S",
                SequenceJobRole.OverwriteCount => "T",
                SequenceJobRole.MicroQr => "U",
                SequenceJobRole.Pause => "W",
                _ => string.Empty
            };
        }
    }
}