using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using LocalizationTranslator.BL.Services;
using LocalizationTranslator.Core.Services;
using LocalizationTranslator.Wpf.Interfaces;
using LocalizationTranslator.Wpf.Services;
using Newtonsoft.Json.Linq;

namespace LocalizationTranslator.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private const string SUBS_KEY = "396204b01c604a87b9a02d019044d4cf";


        private readonly IJsonFileService jsonFileService;
        private readonly IDialogService dialogService;
        private readonly ILocalizationJsonTranslator translator;

        private readonly IDictionary<string, string> _lngs;

        private JObject source;
        private JObject result;


        public bool IsNotProcessing
        {
            get { return (bool)GetValue(IsNotProcessingProperty); }
            set { SetValue(IsNotProcessingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsNotProcessing.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsNotProcessingProperty =
            DependencyProperty.Register("IsNotProcessing", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));




        public MainWindow()
        {

            InitializeComponent();


            dialogService = new DefaultDialogService();
            jsonFileService = new JsonFileService();
            var msAzureTraslator = new MsAzureTranslator(SUBS_KEY);
            var escapeTextProcessor = new EscapeTextWrapper();
            translator = new LocalizationJsonTranslator(msAzureTraslator, escapeTextProcessor, escapeTextProcessor);

            _lngs = msAzureTraslator.AvailableLanguages;

            cbSourceLng.ItemsSource = _lngs;
            cbSourceLng.DisplayMemberPath = "Value";
            cbSourceLng.SelectedValuePath = "Key";


            cbTargetLng.ItemsSource = _lngs;
            cbTargetLng.DisplayMemberPath = "Value";
            cbTargetLng.SelectedValuePath = "Key";


        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {

            if (dialogService.OpenFileDialog())
            {
                var path = dialogService.FilePath;
                source = jsonFileService.OpenFile(path);
                btnTranslate.IsEnabled = true;
            }
        }

        private void BtnTranslate_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                IsNotProcessing = false;

                tbResult.Text = "Translation on progress";

                Task.Run(() =>
                {
                    result = translator.Translate(source, "en", "ru");

                }).GetAwaiter()
                .OnCompleted(() =>
                {
                    btnDownload.IsEnabled = true;
                    IsNotProcessing = true;
                    tbResult.Text = result.ToString(Newtonsoft.Json.Formatting.Indented);
                });


            }
            catch (Exception ex)
            {
                dialogService.ShowMessage(ex.Message);

                IsNotProcessing = true;
                btnDownload.IsEnabled = false;
                tbResult.Text = "Failure";
            }
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (dialogService.SaveFileDialog())
            {
                var path = dialogService.FilePath;
                jsonFileService.SaveFile(result, path);
            }
        }
    }
}
