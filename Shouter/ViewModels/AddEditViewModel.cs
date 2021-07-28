using Shouter.Properties;
using Shouter.ViewModels.Commands;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shouter.ViewModels
{
    public class AddEditViewModel : ViewModelBase
    {
        private readonly Action<Tuple<string, string>> OnRequestClose;
        private const string UserAgent = "Mozilla/5.0 (compatible; MSIE 11.0; Windows NT 10.0; .NET CLR 1.0.3705;)";
        private const string PlsFileHeader = "File";
        private const string PlsTitleHeader = "Title";

        #region Properties
        private string _name;
        public string Name
        {
            get => _name;
            set => CheckAndSetProperty(ref _name, value, nameof(Name));
        }

        private string _address;
        public string Address
        {
            get => _address;
            set => CheckAndSetProperty(ref _address, value, nameof(Address));
        }

        private string _title;
        public string Title
        {
            get => _title;
            set => CheckAndSetProperty(ref _title, value, nameof(Title));
        }

        private string _okText;
        public string OkText
        {
            get => _okText;
            set => CheckAndSetProperty(ref _okText, value, nameof(OkText));
        }

        private string _plsPath;
        public string PlsPath
        {
            get => _plsPath;
            set => CheckAndSetProperty(ref _plsPath, value, nameof(PlsPath));
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set => CheckAndSetProperty(ref _statusText, value, nameof(StatusText));
        }
        #endregion

        #region Commands

        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand PickPlsCommand { get; }
        public RelayCommand LoadPlsCommand { get; }

        #endregion

        public AddEditViewModel(Action<Tuple<string, string>> onRequestClose)
        {
            OnRequestClose = onRequestClose;
            OkCommand = new RelayCommand(() =>
            {
                OnRequestClose(MakeTuple(Name, Address));
            }, () => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Address));
            CancelCommand = new RelayCommand(() => OnRequestClose(null));
            PickPlsCommand = new RelayCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = $"{Resources.plsFile} | *.pls",
                    Title = Resources.choosePlsDialog,
                    Multiselect = false
                };
                var result = openFileDialog.ShowDialog();
                if(result == DialogResult.OK)
                {
                    string file = openFileDialog.FileName;
                    if (!File.Exists(file) || Path.GetExtension(file) != ".pls") return;
                    PlsPath = file;
                }
            });
            LoadPlsCommand = new RelayCommand(() =>
            {
                Task.Run(LoadPlsFile);
            }, () => !string.IsNullOrWhiteSpace(PlsPath));
        }

        private async Task LoadPlsFile()
        {
            StatusText = string.Empty;
            AppendStatusText(Resources.readingPls);
            string content = await GetFileContent(PlsPath);
            AppendStatusText(Resources.fileRead);
            int i = 1;
            int indexOf;
            AppendStatusText(Resources.analizingFile);
            while ((indexOf = content.IndexOf($"{PlsFileHeader}{i}")) != -1)
            {
                AppendStatusText(string.Format(Resources.analizingLink, i));
                int lenght = $"File{i}=".Length;
                StringBuilder sb = new StringBuilder();
                int urlStart = indexOf + lenght;
                while(urlStart != content.Length && content[urlStart] != '\n')
                {
                    sb.Append(content[urlStart]);
                    urlStart++;
                }
                string url = sb.ToString();
                if (url[url.Length - 1] == '\r') url = url.Substring(0, url.Length - 1);
                AppendStatusText(Resources.validatingLink);
                bool linkValid = await ValidateLink(url);
                if (linkValid)
                {
                    AppendStatusText(Resources.linkIsValid);
                    Address = url;
                    AppendStatusText(Resources.gettingStationName);
                    int indexOfTitle = content.IndexOf($"{PlsTitleHeader}{i}");
                    if (indexOfTitle != -1)
                    {
                        StringBuilder titleBuilder = new StringBuilder();
                        int stratTitleIndex = indexOfTitle + $"{PlsTitleHeader}{i}=".Length;
                        while (content[stratTitleIndex] != '\n')
                        {
                            titleBuilder.Append(content[stratTitleIndex]);
                            stratTitleIndex++;
                        }
                        string title = titleBuilder.ToString();
                        if (title[title.Length - 1] == '\r') title = title.Substring(0, title.Length - 1);
                        Name = title;
                        AppendStatusText(Resources.nameFound);
                    }
                    else AppendStatusText(Resources.couldNotFindName);
                    return;
                }
                else AppendStatusText(Resources.linkInvalid);
                i++;
                
            }
            AppendStatusText(Resources.plsNoResults);
        }

        private void AppendStatusText(string value)
        {
            StatusText += value + Environment.NewLine;
        }

        private async Task<string> GetFileContent(string file)
        {
            // File in filesystem    
            try
            {
                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
                // Link to file
                else if (Uri.TryCreate(file, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
                {
                    WebClient client = new WebClient();
                    // set the user agent
                    client.Headers.Add("user-agent", UserAgent);
                    return await client.DownloadStringTaskAsync(uriResult);
                }
                // ¯\_(ツ)_/¯
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }


        private async Task<bool> ValidateLink(string link)
        {
            try
            {
                var request = WebRequest.CreateHttp(link);
                request.UserAgent = UserAgent;
                request.Timeout = 5000; 
                var response = await request.GetResponseAsync();
                if (response == null) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private Tuple<string, string> MakeTuple(string a, string b)
        {
            return new Tuple<string, string>(a, b);
        }
    }
}
