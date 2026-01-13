using System;
using System.Collections.Generic;
using System.Linq;
using MarkingDesigner.Models;

namespace MarkingDesigner.ViewModels
{
    public class DeviceOption
    {
        public Devices Value { get; set; }
        public string Label { get; set; } = "";
    }

    public class DeviceViewModel : BindableBase
    {
        public List<DeviceOption> DeviceOptions { get; }

        public DeviceViewModel()
        {
            DeviceOptions = Enum.GetValues(typeof(Devices))
                                .Cast<Devices>()
                                .Select(d => new DeviceOption { Value = d, Label = GetDeviceString(d) })
                                .ToList();

            // 修正: 初期値を VM-M1 に設定
            // プロパティ経由でセットすることで、UpdateDeviceSpecs() が走り、
            // RangeX/RangeY も VM-M1 の値 (80x30) に自動更新されます。
            CurrentDevice = Devices.VMM1;
        }

        // 初期フィールド値は None のままにしておき、コンストラクタで変更を検知させる
        private Devices _currentDevice = Devices.None;
        public Devices CurrentDevice
        {
            get => _currentDevice;
            set
            {
                if (SetProperty(ref _currentDevice, value))
                {
                    UpdateDeviceSpecs();
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        private double _rangeX = 100.0;
        public double RangeX { get => _rangeX; set => SetProperty(ref _rangeX, value); }

        private double _rangeY = 100.0;
        public double RangeY { get => _rangeY; set => SetProperty(ref _rangeY, value); }

        public string DisplayName => GetDeviceString(CurrentDevice);

        private string GetDeviceString(Devices dev)
        {
            return dev switch
            {
                Devices.None => "None",
                Devices.VM2030 => "VM2030",
                Devices.VM5030 => "VM5030",
                Devices.VM3730A => "VM3730A",
                Devices.VMM1 => "VM-M1",
                _ => "Unknown"
            };
        }

        private void UpdateDeviceSpecs()
        {
            switch (CurrentDevice)
            {
                case Devices.VM2030: RangeX = 80; RangeY = 30; break;
                case Devices.VM5030: RangeX = 130; RangeY = 30; break;
                case Devices.VM3730A: RangeX = 80; RangeY = 80; break;
                case Devices.VMM1: RangeX = 80; RangeY = 30; break;
                default: RangeX = 100; RangeY = 100; break;
            }
        }
    }
}