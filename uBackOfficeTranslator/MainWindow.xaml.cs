using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
using uLanguageTranslator;

namespace uBackOfficeTranslator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.

            if ((bool)dialog.ShowDialog(this))
            {
                umbracoFolder.Text = dialog.SelectedPath;

                LanguageRepository.Init(System.IO.Path.Combine(umbracoFolder.Text, @"umbraco\config\lang"));

                

                string[] languagecodes = LanguageRepository.ListLanguages();

                foreach (var code in languagecodes)
                {
                    Language lang = LanguageRepository.GetLanguage(code);


                    cmbSourceLanguage.Items.Add(new ComboBoxItem() { Content = string.Format("{0} [{1}]", lang.InternationalName, lang.Id), Tag = code });
                    cmbDestinationLanguage.Items.Add(new ComboBoxItem() { Content = string.Format("{0} [{1}]", lang.InternationalName, lang.Id), Tag = code });
                }


                cmbSourceLanguage.IsEnabled = true;
                cmbDestinationLanguage.IsEnabled = true;


            }
        }

        private void cmbSourceLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Language language = LanguageRepository.GetLanguage(SourceLanguage);

            UpdateSyncState();
        }

        private void cmbDestinationLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSyncState();

            btnSaveAs.IsEnabled = false;

        }

        private void UpdateSyncState()
        {

            if (SourceLanguage == "" || DestinationLanguage == "")
            {
                btnTranslate.IsEnabled = false;
                return;
            }

            if (SourceLanguage == DestinationLanguage)
            {
                btnTranslate.IsEnabled = false;
                return;
            }

            btnTranslate.IsEnabled = true;

        }

        private void UpdateLanguageStackPanel()
        {

            
            Language destination = LanguageRepository.GetLanguage(DestinationLanguage);


            languageStack.Children.Clear();

            languageStack.Children.Add(new Label() { Content = "Translation match complete" });
            languageStack.Children.Add(new Label() { Content = "The following fields should be translated:", FontWeight = FontWeight.FromOpenTypeWeight(800) });

            foreach (var area in destination.Areas)
            {
                string[] keys = destination.GetTranslationKeys(area);
                foreach (var key in keys)
                {
                    string phrase = destination.GetRawPhrase(area, key);

                    if (phrase.StartsWith("TRANSLATE"))
                        languageStack.Children.Add(new Label() { Content = string.Format("[{0}/{1}] {2}", area, key, phrase) });

                }
            }

            languageStack.Children.Add(new Label() { Content = "The following fields should be considered for removal:", FontWeight = FontWeight.FromOpenTypeWeight(800) });

            foreach (var area in destination.Areas)
            {
                string[] keys = destination.GetTranslationKeys(area);
                foreach (var key in keys)
                {
                    string phrase = destination.GetRawPhrase(area, key);

                    if (phrase.StartsWith("REMOVE"))
                        languageStack.Children.Add(new Label() { Content = string.Format("[{0}/{1}] {2}", area, key, phrase) });

                }
            }

        }

        public string SourceLanguage
        {
            get
            {
                return cmbSourceLanguage.SelectedValue != null ? (string)((ComboBoxItem)cmbSourceLanguage.SelectedValue).Tag : "";
            }
        }


        public string DestinationLanguage
        {
            get
            {
                return cmbDestinationLanguage.SelectedValue != null ? (string)((ComboBoxItem)cmbDestinationLanguage.SelectedValue).Tag : "";
            }
        }

        private void btnTranslate_Click(object sender, RoutedEventArgs e)
        {
            Language destination = LanguageRepository.GetLanguage(DestinationLanguage);
            Language source = LanguageRepository.GetLanguage(SourceLanguage);

            destination.SynchronizeTranslations(source);

            UpdateLanguageStackPanel();
            btnSaveAs.IsEnabled = true;
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            VistaSaveFileDialog dialog = new VistaSaveFileDialog();

            dialog.Title = "Save your translation as..";
            dialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
            dialog.OverwritePrompt = true;
            dialog.DefaultExt = "xml";
            dialog.AddExtension = true;
            dialog.FileName = DestinationLanguage + ".xml";

            if ((bool)dialog.ShowDialog())
            {
                string file = dialog.FileName;

                
                Language destination = LanguageRepository.GetLanguage(DestinationLanguage);

                destination.Save(file);
            
            }



        }
    }
}
