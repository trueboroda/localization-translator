using System;
using System.Collections.Generic;
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
                result = translator.Translate(source, "en", "ru");

                btnDownload.IsEnabled = true;
            }
            catch (Exception ex)
            {
                dialogService.ShowMessage(ex.Message);
                btnDownload.IsEnabled = false;
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
