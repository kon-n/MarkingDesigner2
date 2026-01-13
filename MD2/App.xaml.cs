using System.Windows;
using MarkingDesigner.Models;

namespace MarkingDesigner
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // フォントデータの読み込み開始
            MarkingFont.Initialize();
        }
    }
}