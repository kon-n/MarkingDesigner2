using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MarkingDesigner.Models;

namespace MarkingDesigner.ViewModels
{
    /// <summary>
    /// 1 つのシーケンスに含まれる「ジョブ 1 件分」の登録情報（役割・2次元の種類・V・変換元など）。
    /// 複数の2次元刻印は、本エントリを複数行登録することで表現する。
    /// </summary>
    public class SequenceJobEntry : BindableBase
    {
        public SequenceJobEntry()
        {
            TwoDSourceJobs.CollectionChanged += OnTwoDSourceJobsChanged;
        }

        private void OnTwoDSourceJobsChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => RaisePropertyChanged(nameof(UiTwoDSrcLabel));

        private JobViewModel? _job;
        public JobViewModel? Job
        {
            get => _job;
            set
            {
                if (SetProperty(ref _job, value))
                {
                    RaisePropertyChanged(nameof(UiJobIdLabel));
                    RaisePropertyChanged(nameof(UiJobNameLabel));
                    RaisePropertyChanged(nameof(UiJobTextLabel));
                }
            }
        }

        private SequenceJobRole _role = SequenceJobRole.MarkingExecute;
        public SequenceJobRole Role
        {
            get => _role;
            set
            {
                if (SetProperty(ref _role, value))
                {
                    if (TryMapTwoDKind(_role, out var mapped))
                        TwoDSymbolKind = mapped;
                    NotifyTwoDGridColumns();
                }
            }
        }

        private TwoDSymbolKind _twoDSymbolKind = TwoDSymbolKind.QrCode;
        /// <summary>2次元の場合の種別（QR / DataMatrix / microQR）。</summary>
        public TwoDSymbolKind TwoDSymbolKind
        {
            get => _twoDSymbolKind;
            set
            {
                if (SetProperty(ref _twoDSymbolKind, value))
                    NotifyTwoDGridColumns();
            }
        }

        private int _symbolSizeV = 4;
        /// <summary>V コマンドで指定するシンボルサイズ（1〜99）。例: P1/V4/Q2 の 4。</summary>
        public int SymbolSizeV
        {
            get => _symbolSizeV;
            set
            {
                int v = System.Math.Clamp(value, 1, 99);
                if (SetProperty(ref _symbolSizeV, v))
                    NotifyTwoDGridColumns();
            }
        }

        /// <summary>2次元コード化する際に連結・符号化の入力とするジョブ（複数可）。</summary>
        public ObservableCollection<JobViewModel> TwoDSourceJobs { get; } = new ObservableCollection<JobViewModel>();

        /// <summary>DataGrid 表示用: 種別の短縮ラベル。</summary>
        public string UiTwoDKindLabel
        {
            get
            {
                if (!IsTwoDCommand)
                    return "—";
                return TwoDSymbolKind switch
                {
                    TwoDSymbolKind.QrCode => "QR",
                    TwoDSymbolKind.DataMatrix => "DM",
                    TwoDSymbolKind.MicroQr => "μQR",
                    _ => "?"
                };
            }
        }

        /// <summary>DataGrid 表示用: V の値。</summary>
        public string UiSymbolVLabel => !IsTwoDCommand ? "—" : SymbolSizeV.ToString();

        /// <summary>DataGrid 表示用: 変換元件数。</summary>
        public string UiTwoDSrcLabel => !SupportsSourceJobs ? "—" : TwoDSourceJobs.Count.ToString();

        public bool IsTwoDCommand =>
            Role == SequenceJobRole.QrCode ||
            Role == SequenceJobRole.DataMatrix ||
            Role == SequenceJobRole.MicroQr;

        public bool SupportsSourceJobs => IsTwoDCommand || Role == SequenceJobRole.TextJoin;

        /// <summary>`P` が直前で与える開始位置基準を使うコマンドかどうか。</summary>
        public bool UsesPlacementFromP =>
            Role == SequenceJobRole.CharacterReference ||
            Role == SequenceJobRole.QrCode ||
            Role == SequenceJobRole.DataMatrix ||
            Role == SequenceJobRole.MicroQr;

        /// <summary>`C` コマンドの反復刻印仕様（前回刻印内容を使用）かどうか。</summary>
        public bool RepeatsPreviousMarkedContent => Role == SequenceJobRole.CharacterReference;

        /// <summary>`C` ではインクリメント・カレンダー更新を行わない。</summary>
        public bool ShouldUpdateIncrementAndCalendar => !RepeatsPreviousMarkedContent;

        public bool RequiresPrimaryJob =>
            Role == SequenceJobRole.MarkingExecute ||
            Role == SequenceJobRole.CharacterReference ||
            Role == SequenceJobRole.PositionReference;

        public string UiJobIdLabel => !RequiresPrimaryJob ? "" : (Job == null ? "" : Job.JobId.ToString());
        public string UiJobNameLabel => !RequiresPrimaryJob ? "" : (Job?.Name ?? "");
        public string UiJobTextLabel => !RequiresPrimaryJob ? "" : (Job?.Marking.Text ?? "");

        private void NotifyTwoDGridColumns()
        {
            RaisePropertyChanged(nameof(IsTwoDCommand));
            RaisePropertyChanged(nameof(SupportsSourceJobs));
            RaisePropertyChanged(nameof(UsesPlacementFromP));
            RaisePropertyChanged(nameof(RepeatsPreviousMarkedContent));
            RaisePropertyChanged(nameof(ShouldUpdateIncrementAndCalendar));
            RaisePropertyChanged(nameof(RequiresPrimaryJob));
            RaisePropertyChanged(nameof(UiJobIdLabel));
            RaisePropertyChanged(nameof(UiJobNameLabel));
            RaisePropertyChanged(nameof(UiJobTextLabel));
            RaisePropertyChanged(nameof(UiTwoDKindLabel));
            RaisePropertyChanged(nameof(UiSymbolVLabel));
            RaisePropertyChanged(nameof(UiTwoDSrcLabel));
        }

        private static bool TryMapTwoDKind(SequenceJobRole role, out TwoDSymbolKind kind)
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
