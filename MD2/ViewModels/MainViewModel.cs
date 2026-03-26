using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MarkingDesigner.Models;

namespace MarkingDesigner.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public DeviceViewModel Device { get; } = new DeviceViewModel();
        public ObservableCollection<SequenceViewModel> Sequences { get; } = new ObservableCollection<SequenceViewModel>();
        public ObservableCollection<JobViewModel> AllJobs { get; } = new ObservableCollection<JobViewModel>();

        public EditHistoryManager History { get; } = new EditHistoryManager();

        private SequenceViewModel _currentSequence = null!;
        public SequenceViewModel CurrentSequence
        {
            get => _currentSequence;
            set
            {
                if (SetProperty(ref _currentSequence, value))
                {
                    SelectedSequenceEntry = null;
                    ClearJobSelection();
                }
            }
        }

        private SequenceJobEntry? _selectedSequenceEntry;
        public SequenceJobEntry? SelectedSequenceEntry
        {
            get => _selectedSequenceEntry;
            set
            {
                if (!SetProperty(ref _selectedSequenceEntry, value))
                    return;
                _suppressJobSync = true;
                try
                {
                    if (_selectedJob != null)
                        _selectedJob.IsSelected = false;
                    _selectedJob = value?.Job;
                    if (_selectedJob != null)
                        _selectedJob.IsSelected = true;
                    RaisePropertyChanged(nameof(SelectedJob));
                }
                finally
                {
                    _suppressJobSync = false;
                }
                (DeleteJobCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                (MoveSequenceEntryUpCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                (MoveSequenceEntryDownCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        private JobViewModel? _selectedJob;
        private bool _suppressJobSync;

        public JobViewModel? SelectedJob
        {
            get => _selectedJob;
            set
            {
                if (_suppressJobSync)
                {
                    SetProperty(ref _selectedJob, value);
                    return;
                }
                if (_selectedJob != null)
                    _selectedJob.IsSelected = false;
                if (!SetProperty(ref _selectedJob, value))
                    return;
                if (_selectedJob != null)
                    _selectedJob.IsSelected = true;

                if (CurrentSequence != null && value != null)
                {
                    var entry = CurrentSequence.Entries.FirstOrDefault(e => e.Job == value);
                    if (_selectedSequenceEntry != entry)
                    {
                        _selectedSequenceEntry = entry;
                        RaisePropertyChanged(nameof(SelectedSequenceEntry));
                    }
                }
                else if (value == null)
                {
                    if (_selectedSequenceEntry != null)
                    {
                        _selectedSequenceEntry = null;
                        RaisePropertyChanged(nameof(SelectedSequenceEntry));
                    }
                }
                (DeleteJobCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                (MoveSequenceEntryUpCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                (MoveSequenceEntryDownCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        private void ClearJobSelection()
        {
            _suppressJobSync = true;
            try
            {
                if (_selectedJob != null)
                    _selectedJob.IsSelected = false;
                _selectedJob = null;
                _selectedSequenceEntry = null;
                RaisePropertyChanged(nameof(SelectedJob));
                RaisePropertyChanged(nameof(SelectedSequenceEntry));
            }
            finally
            {
                _suppressJobSync = false;
            }
        }

        private JobViewModel? _jobToAdd;
        public JobViewModel? JobToAdd
        {
            get => _jobToAdd;
            set => SetProperty(ref _jobToAdd, value);
        }

        private SequenceJobRole _roleToAdd = SequenceJobRole.MarkingExecute;
        public SequenceJobRole RoleToAdd
        {
            get => _roleToAdd;
            set
            {
                if (SetProperty(ref _roleToAdd, value))
                {
                    if (TryMapTwoDKindFromRole(_roleToAdd, out var mapped))
                        TwoDSymbolKindToAdd = mapped;
                    RaisePropertyChanged(nameof(RoleToAddIsTwoD));
                    RaisePropertyChanged(nameof(RoleToAddSupportsSourceJobs));
                    RaisePropertyChanged(nameof(RoleToAddRequiresPrimaryJob));
                    RaisePropertyChanged(nameof(RoleToAddNeedsJobPicker));
                }
            }
        }

        public bool RoleToAddIsTwoD =>
            RoleToAdd == SequenceJobRole.QrCode ||
            RoleToAdd == SequenceJobRole.DataMatrix ||
            RoleToAdd == SequenceJobRole.MicroQr;

        public bool RoleToAddSupportsSourceJobs => RoleToAddIsTwoD || RoleToAdd == SequenceJobRole.TextJoin;
        public bool RoleToAddRequiresPrimaryJob =>
            RoleToAdd == SequenceJobRole.MarkingExecute ||
            RoleToAdd == SequenceJobRole.CharacterReference ||
            RoleToAdd == SequenceJobRole.PositionReference;

        public bool RoleToAddNeedsJobPicker => RoleToAddRequiresPrimaryJob || RoleToAddSupportsSourceJobs;

        public ObservableCollection<JobViewModel> TwoDSourceJobsToAdd { get; } = new ObservableCollection<JobViewModel>();

        public ObservableCollection<TwoDSymbolOption> TwoDSymbolOptions { get; }

        private TwoDSymbolKind _twoDSymbolKindToAdd = TwoDSymbolKind.QrCode;
        public TwoDSymbolKind TwoDSymbolKindToAdd
        {
            get => _twoDSymbolKindToAdd;
            set => SetProperty(ref _twoDSymbolKindToAdd, value);
        }

        private int _symbolSizeVToAdd = 4;
        public int SymbolSizeVToAdd
        {
            get => _symbolSizeVToAdd;
            set => SetProperty(ref _symbolSizeVToAdd, Math.Clamp(value, 1, 99));
        }

        public ObservableCollection<SequenceRoleOption> SequenceRoleOptions { get; }

        private double _zoomScale = 1.0;
        public double ZoomScale
        {
            get => _zoomScale;
            set => SetProperty(ref _zoomScale, value);
        }

        public ObservableCollection<double> ZoomPresets { get; } = new ObservableCollection<double> { 0.5, 0.75, 1.0, 1.5, 2.0, 3.0 };

        public ICommand ChangeThemeCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public ICommand AddJobCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand NextSeqCommand { get; }
        public ICommand PrevSeqCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand MoveSequenceEntryUpCommand { get; }
        public ICommand MoveSequenceEntryDownCommand { get; }
        public ICommand AddTwoDSourceJobCommand { get; }
        public ICommand RemoveTwoDSourceJobCommand { get; }
        public ICommand RemoveTwoDSourceJobFromEntryCommand { get; }
        public ICommand AddTwoDSourceJobToEntryCommand { get; }

        public MainViewModel()
        {
            SequenceRoleOptions = new ObservableCollection<SequenceRoleOption>(BuildSequenceRoleOptions());
            TwoDSymbolOptions = new ObservableCollection<TwoDSymbolOption>(BuildTwoDSymbolOptions());

            ChangeThemeCommand = new RelayCommand<string>(ThemeChange);
            ChangeLanguageCommand = new RelayCommand<string>(LanguageChange);

            AddJobCommand = new RelayCommand<object>(_ => AddJobToSequence());
            DeleteJobCommand = new RelayCommand<object>(_ => RemoveJobFromSequence(), _ => SelectedSequenceEntry != null);
            NextSeqCommand = new RelayCommand<object>(_ => MoveSequence(1));
            PrevSeqCommand = new RelayCommand<object>(_ => MoveSequence(-1));

            UndoCommand = new RelayCommand<object>(_ => History.Undo(), _ => History.CanUndo);
            RedoCommand = new RelayCommand<object>(_ => History.Redo(), _ => History.CanRedo);

            MoveSequenceEntryUpCommand = new RelayCommand<object>(_ => MoveSequenceEntry(-1), _ => CanMoveSequenceEntry(-1));
            MoveSequenceEntryDownCommand = new RelayCommand<object>(_ => MoveSequenceEntry(1), _ => CanMoveSequenceEntry(1));

            AddTwoDSourceJobCommand = new RelayCommand<object>(_ => AddTwoDSourceToPending());
            RemoveTwoDSourceJobCommand = new RelayCommand<object>(RemoveTwoDSourceFromPending);
            RemoveTwoDSourceJobFromEntryCommand = new RelayCommand<object>(RemoveTwoDSourceFromEntry);
            AddTwoDSourceJobToEntryCommand = new RelayCommand<object>(_ => AddTwoDSourceJobToSelectedEntry());

            InitializeFixedMemory();
        }

        private static SequenceRoleOption[] BuildSequenceRoleOptions()
        {
            static string T(string key, string fallback) =>
                Application.Current?.TryFindResource(key) as string ?? fallback;

            return new[]
            {
                new SequenceRoleOption(SequenceJobRole.PositionReference, T("Str_Cmd_P", "P - パラメータ参照")),
                new SequenceRoleOption(SequenceJobRole.CharacterReference, T("Str_Cmd_C", "C - 文字列参照")),
                new SequenceRoleOption(SequenceJobRole.MarkingExecute, T("Str_Cmd_J", "J - 刻印")),
                new SequenceRoleOption(SequenceJobRole.TextJoin, T("Str_Cmd_F", "F - 文字列連結")),
                new SequenceRoleOption(SequenceJobRole.QrCode, T("Str_Cmd_Q", "Q - QRコード")),
                new SequenceRoleOption(SequenceJobRole.DataMatrix, T("Str_Cmd_D", "D - DataMatrix")),
                new SequenceRoleOption(SequenceJobRole.MicroQr, T("Str_Cmd_U", "U - microQR")),
                new SequenceRoleOption(SequenceJobRole.CircleMove, T("Str_Cmd_A", "A - 円移動")),
                new SequenceRoleOption(SequenceJobRole.OptimizeMove, T("Str_Cmd_B", "B - 時短")),
                new SequenceRoleOption(SequenceJobRole.SelectPen, T("Str_Cmd_E", "E - 使用ペン指定")),
                new SequenceRoleOption(SequenceJobRole.OutputMarkedString, T("Str_Cmd_G", "G - 文字列出力")),
                new SequenceRoleOption(SequenceJobRole.Home, T("Str_Cmd_H", "H - 原点復帰")),
                new SequenceRoleOption(SequenceJobRole.SystemInfo, T("Str_Cmd_I", "I - システム情報")),
                new SequenceRoleOption(SequenceJobRole.IncrementDisable, T("Str_Cmd_K", "K - インクリメント無効")),
                new SequenceRoleOption(SequenceJobRole.MoveToStart, T("Str_Cmd_M", "M - ムーブ")),
                new SequenceRoleOption(SequenceJobRole.FoldDigits, T("Str_Cmd_N", "N - 桁数")),
                new SequenceRoleOption(SequenceJobRole.SelectFont, T("Str_Cmd_O", "O - フォント指定")),
                new SequenceRoleOption(SequenceJobRole.Reader, T("Str_Cmd_R", "R - リーダー")),
                new SequenceRoleOption(SequenceJobRole.LineMove, T("Str_Cmd_S", "S - 直線移動")),
                new SequenceRoleOption(SequenceJobRole.OverwriteCount, T("Str_Cmd_T", "T - 上書き回数")),
                new SequenceRoleOption(SequenceJobRole.Pause, T("Str_Cmd_W", "W - 一時停止")),
            };
        }

        private static TwoDSymbolOption[] BuildTwoDSymbolOptions()
        {
            static string T(string key, string fallback) =>
                Application.Current?.TryFindResource(key) as string ?? fallback;

            return new[]
            {
                new TwoDSymbolOption(TwoDSymbolKind.QrCode, T("Str_TwoD_QR", "QR")),
                new TwoDSymbolOption(TwoDSymbolKind.DataMatrix, T("Str_TwoD_DM", "DataMatrix")),
                new TwoDSymbolOption(TwoDSymbolKind.MicroQr, T("Str_TwoD_MicroQR", "microQR")),
            };
        }

        private void InitializeFixedMemory()
        {
            for (int j = 1; j <= 256; j++)
            {
                var job = new JobViewModel(Device)
                {
                    JobId = j,
                    Name = $"JOB-{j:000}",
                    StartX = 10,
                    StartY = 10,
                    Height = 5,
                    PitchX = 4
                };
                AllJobs.Add(job);
            }

            for (int s = 1; s <= 256; s++)
            {
                Sequences.Add(new SequenceViewModel { SequenceId = s, Title = $"SEQ-{s:000}" });
            }

            CurrentSequence = Sequences[0];
            JobToAdd = AllJobs[0];

            AllJobs[0].Marking.Text = "SAMPLE";
            AllJobs[1].Marking.Text = "TEST";
            CurrentSequence.Entries.Add(new SequenceJobEntry { Job = AllJobs[0], Role = SequenceJobRole.MarkingExecute });
            CurrentSequence.Entries.Add(new SequenceJobEntry { Job = AllJobs[1], Role = SequenceJobRole.MarkingExecute });
        }

        private void AddJobToSequence()
        {
            if (CurrentSequence == null)
                return;

            if (RoleToAddRequiresPrimaryJob && JobToAdd == null)
            {
                MessageBox.Show(
                    Application.Current?.TryFindResource("Str_MsgPrimaryJobRequired") as string
                    ?? "このコマンドには対象ジョブの指定が必要です。",
                    "MarkingDesigner",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            if ((RoleToAdd == SequenceJobRole.TextJoin ||
                RoleToAdd == SequenceJobRole.QrCode ||
                RoleToAdd == SequenceJobRole.DataMatrix ||
                RoleToAdd == SequenceJobRole.MicroQr) &&
                TwoDSourceJobsToAdd.Count == 0)
            {
                MessageBox.Show(
                    Application.Current?.TryFindResource("Str_MsgTwoDNeedsSources") as string
                    ?? "2次元コードでは、変換元となるジョブを1件以上追加してください。",
                    "MarkingDesigner",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var entry = new SequenceJobEntry
            {
                Job = JobToAdd,
                Role = RoleToAdd,
                TwoDSymbolKind = TryMapTwoDKindFromRole(RoleToAdd, out var mappedKind) ? mappedKind : TwoDSymbolKindToAdd,
                SymbolSizeV = SymbolSizeVToAdd
            };
            foreach (var j in TwoDSourceJobsToAdd)
                entry.TwoDSourceJobs.Add(j);

            History.Do(new AddJobToSequenceCommand(CurrentSequence, entry));
            SelectedSequenceEntry = entry;
            TwoDSourceJobsToAdd.Clear();
        }

        private void AddTwoDSourceToPending()
        {
            if (JobToAdd == null || TwoDSourceJobsToAdd.Contains(JobToAdd))
                return;
            TwoDSourceJobsToAdd.Add(JobToAdd);
        }

        private void RemoveTwoDSourceFromPending(object? parameter)
        {
            if (parameter is JobViewModel j)
                TwoDSourceJobsToAdd.Remove(j);
        }

        private void RemoveTwoDSourceFromEntry(object? parameter)
        {
            if (parameter is not JobViewModel j || SelectedSequenceEntry == null)
                return;
            SelectedSequenceEntry.TwoDSourceJobs.Remove(j);
        }

        private void AddTwoDSourceJobToSelectedEntry()
        {
            if (SelectedSequenceEntry == null || JobToAdd == null)
                return;
            if (!SelectedSequenceEntry.SupportsSourceJobs)
                return;
            if (SelectedSequenceEntry.TwoDSourceJobs.Contains(JobToAdd))
                return;
            SelectedSequenceEntry.TwoDSourceJobs.Add(JobToAdd);
        }

        private void RemoveJobFromSequence()
        {
            if (SelectedSequenceEntry == null || CurrentSequence == null)
                return;
            History.Do(new RemoveJobFromSequenceCommand(CurrentSequence, SelectedSequenceEntry));
            SelectedSequenceEntry = null;
        }

        private void MoveSequenceEntry(int delta)
        {
            if (SelectedSequenceEntry == null || CurrentSequence == null)
                return;
            int i = CurrentSequence.Entries.IndexOf(SelectedSequenceEntry);
            if (i < 0)
                return;
            int j = i + delta;
            if (j < 0 || j >= CurrentSequence.Entries.Count)
                return;
            History.Do(new MoveSequenceEntryCommand(CurrentSequence, i, j));
        }

        private bool CanMoveSequenceEntry(int delta)
        {
            if (SelectedSequenceEntry == null || CurrentSequence == null)
                return false;
            int i = CurrentSequence.Entries.IndexOf(SelectedSequenceEntry);
            if (i < 0)
                return false;
            int j = i + delta;
            return j >= 0 && j < CurrentSequence.Entries.Count;
        }

        private void MoveSequence(int dir)
        {
            if (Sequences.Count == 0)
                return;
            int idx = Sequences.IndexOf(CurrentSequence) + dir;
            if (idx < 0)
                idx = 0;
            if (idx >= Sequences.Count)
                idx = Sequences.Count - 1;
            CurrentSequence = Sequences[idx];
        }

        private void ThemeChange(string theme)
        {
            try
            {
                var dict = new ResourceDictionary { Source = new Uri($"Themes/{theme}Theme.xaml", UriKind.Relative) };
                var oldTheme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.ToString().Contains("Theme"));
                if (oldTheme != null)
                    Application.Current.Resources.MergedDictionaries.Remove(oldTheme);
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テーマ読み込みに失敗しました: {ex.Message}", "ThemeChange Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LanguageChange(string lang)
        {
            string uri = $"Resources/Languages/{lang}.xaml";
            try
            {
                var dict = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };
                var oldLang = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Contains("Str_Theme"));
                if (oldLang != null)
                    Application.Current.Resources.MergedDictionaries.Remove(oldLang);
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static bool TryMapTwoDKindFromRole(SequenceJobRole role, out TwoDSymbolKind kind)
        {
            switch (role)
            {
                case SequenceJobRole.QrCode:
                    kind = TwoDSymbolKind.QrCode;
                    return true;
                case SequenceJobRole.DataMatrix:
                    kind = TwoDSymbolKind.DataMatrix;
                    return true;
                case SequenceJobRole.MicroQr:
                    kind = TwoDSymbolKind.MicroQr;
                    return true;
                default:
                    kind = TwoDSymbolKind.QrCode;
                    return false;
            }
        }
    }
}
