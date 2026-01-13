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

        // 修正: 初期化前にアクセスされる可能性を考慮し、null! で警告抑制するか、Null許容にする
        private SequenceViewModel _currentSequence = null!;
        public SequenceViewModel CurrentSequence
        {
            get => _currentSequence;
            set { if (SetProperty(ref _currentSequence, value)) SelectedJob = null; }
        }

        // 修正: 選択なし状態がありうるため、Null許容型 (?) に変更
        private JobViewModel? _selectedJob;
        public JobViewModel? SelectedJob
        {
            get => _selectedJob;
            set
            {
                // 選択解除ロジックがあるため null チェックが必要
                if (_selectedJob != null) _selectedJob.IsSelected = false;

                SetProperty(ref _selectedJob, value);

                if (_selectedJob != null) _selectedJob.IsSelected = true;

                (DeleteJobCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            }
        }

        // 修正: 初期化ロジックでセットされるため null! で警告抑制
        private JobViewModel _jobToAdd = null!;
        public JobViewModel JobToAdd
        {
            get => _jobToAdd;
            set => SetProperty(ref _jobToAdd, value);
        }

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

        public MainViewModel()
        {
            ChangeThemeCommand = new RelayCommand<string>(ThemeChange);
            ChangeLanguageCommand = new RelayCommand<string>(LanguageChange);

            AddJobCommand = new RelayCommand<object>(_ => AddJobToSequence());
            DeleteJobCommand = new RelayCommand<object>(_ => RemoveJobFromSequence(), _ => SelectedJob != null);
            NextSeqCommand = new RelayCommand<object>(_ => MoveSequence(1));
            PrevSeqCommand = new RelayCommand<object>(_ => MoveSequence(-1));

            InitializeFixedMemory();
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
            CurrentSequence.Jobs.Add(AllJobs[0]);
            CurrentSequence.Jobs.Add(AllJobs[1]);
        }

        private void AddJobToSequence()
        {
            if (CurrentSequence == null || JobToAdd == null) return;
            if (CurrentSequence.Jobs.Contains(JobToAdd)) { SelectedJob = JobToAdd; return; }
            CurrentSequence.Jobs.Add(JobToAdd);
            SelectedJob = JobToAdd;
        }

        private void RemoveJobFromSequence()
        {
            if (SelectedJob == null || CurrentSequence == null) return;
            CurrentSequence.Jobs.Remove(SelectedJob);
            SelectedJob = null;
        }

        private void MoveSequence(int dir)
        {
            if (Sequences.Count == 0) return;
            int idx = Sequences.IndexOf(CurrentSequence) + dir;
            if (idx < 0) idx = 0;
            if (idx >= Sequences.Count) idx = Sequences.Count - 1;
            CurrentSequence = Sequences[idx];
        }

        private void ThemeChange(string theme)
        {
            try
            {
                var dict = new ResourceDictionary { Source = new Uri($"Themes/{theme}Theme.xaml", UriKind.Relative) };
                var oldTheme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.ToString().Contains("Theme"));
                if (oldTheme != null) Application.Current.Resources.MergedDictionaries.Remove(oldTheme);
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch { }
        }

        private void LanguageChange(string lang)
        {
            string uri = $"Resources/Languages/{lang}.xaml";
            try
            {
                var dict = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };
                var oldLang = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Contains("Str_Theme"));
                if (oldLang != null) Application.Current.Resources.MergedDictionaries.Remove(oldLang);
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}