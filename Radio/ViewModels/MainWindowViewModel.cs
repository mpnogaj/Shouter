using FontAwesome5;
using LibVLCSharp.Shared;
using Radio.Models;
using Radio.Properties;
using Radio.ViewModels.Commands;
using Radio.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Radio.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const char Separator = ';';
        private readonly MediaPlayer _mediaPlayer;
        private readonly LibVLC _libVlc;
        private readonly string _stationsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string _stationsFile;
        
        private bool _unsavedChanges;
        private string _currentStationName = string.Empty;

        #region Properties
        private ObservableCollection<Station> _stations;
        public ObservableCollection<Station> Stations
        {
            get => _stations;
            set => CheckAndSetProperty(ref _stations, value, nameof(Stations));
        }

        private Station _selectedStation;
        public Station SelectedStation
        {
            get => _selectedStation;
            set => CheckAndSetProperty(ref _selectedStation, value, nameof(SelectedStation));
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set => CheckAndSetProperty(ref _isPlaying, value, nameof(IsPlaying));
        }

        private string _status = Resources.ready;
        public string Status
        {
            get => _status;
            set => CheckAndSetProperty(ref _status, value, nameof(Status));
        }

        private bool _minimizeToTray = Settings.Default.MinimizeToTray;
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                CheckAndSetProperty(ref _minimizeToTray, value, nameof(MinimizeToTray));
                Settings.Default.MinimizeToTray = value;
            }
        }
        #endregion

        #region Commands

        public RelayCommand PlayCommand { get; }
        public RelayCommand ForcePlayCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ToggleTray { get; }

        #endregion

        public MainWindowViewModel()
        {
            Core.Initialize();
            _stationsFile = $@"{_stationsDirectory}\Radio\stations.csv";
            _libVlc = new LibVLC(enableDebugLogs: true);
            _mediaPlayer = new MediaPlayer(_libVlc);
            _libVlc.Log += (sender, e) =>
            {
                Console.WriteLine(e.FormattedLog);
                if (_mediaPlayer != null && _mediaPlayer.Media != null)
                {
                    switch (_mediaPlayer.Media.State)
                    {
                        case VLCState.Opening:
                            Status = Resources.connecting;
                            break;
                        case VLCState.Buffering:
                            Status = Resources.buffering;
                            break;
                        case VLCState.Playing:
                            Status = string.Format(Resources.playing_station, _currentStationName);
                            break;
                        case VLCState.Paused:
                        case VLCState.Stopped:
                        case VLCState.NothingSpecial:
                            Status = Resources.ready;
                            break;
                        case VLCState.Ended:
                        case VLCState.Error:
                            Status = Resources.connectionLostWrongAddress;
                            break;
                    }
                }
            };
            _mediaPlayer.Playing += (sender, args) =>
            {
                IsPlaying = true;
            };
            _mediaPlayer.Stopped += (sender, args) =>
            {
                IsPlaying = false;
            };

            Stations = new ObservableCollection<Station>();
            LoadStations();

            ToggleTray = new RelayCommand(() => MinimizeToTray = !MinimizeToTray);

            PlayCommand = new RelayCommand(() =>
            {
                Play(SelectedStation);
            }, () => !IsPlaying && SelectedStation != null);

            ForcePlayCommand = new RelayCommand(() =>
            {
                Stop();
                Play(SelectedStation);
            }, () => SelectedStation != null);

            StopCommand = new RelayCommand(() =>
            {
                Stop();
            }, () => IsPlaying);

            AddCommand = new RelayCommand(() =>
            {
                PrepareDialog(Resources.addStation, string.Empty, string.Empty, (tuple) =>
                {
                    Stations.Add(new Station
                    {
                        Name = tuple.Item1,
                        Address = tuple.Item2
                    });
                });
            });

            EditCommand = new RelayCommand(() =>
            {
                PrepareDialog(Resources.editStation, SelectedStation.Name, SelectedStation.Address, (tuple) =>
                {
                    Stations[Stations.IndexOf(SelectedStation)] = new Station
                    {
                        Name = tuple.Item1,
                        Address = tuple.Item2
                    };
                });
            }, () => SelectedStation != null);

            DeleteCommand = new RelayCommand(() =>
            {
                var result = YesNoDialog.ShowDialog(Resources.deleteStationQuestion, Resources.question, EFontAwesomeIcon.Solid_QuestionCircle);
                if (result)
                {
                    Stations.Remove(SelectedStation);
                }
            }, () => SelectedStation != null);

            SaveCommand = new RelayCommand(() =>
            {
                StringBuilder sb = new StringBuilder();
                foreach (var station in Stations)
                {
                    sb.Append($"{station.Name}{Separator}{station.Address}{Environment.NewLine}");
                }
                using (StreamWriter sw = new StreamWriter(_stationsFile))
                {
                    sw.Write(sb.ToString());
                }
                OkDialog.ShowDialog(Resources.changesSavedSuccessfully, Resources.success, EFontAwesomeIcon.Solid_InfoCircle);
                _unsavedChanges = false;
            });
        }

        ~MainWindowViewModel()
        {
            _libVlc.Dispose();
            _mediaPlayer.Dispose();
        }

        private void Play(Station s)
        {
            try
            {
                _currentStationName = s.Name;
                _mediaPlayer.Media = new Media(_libVlc, new Uri(s.Address));
                _mediaPlayer.Play();
            }
            catch (UriFormatException)
            {
                OkDialog.ShowDialog(Resources.invalidAddress, Resources.error, EFontAwesomeIcon.Solid_MinusCircle);
            }
        }

        private void Stop()
        {
            _currentStationName = string.Empty;
            _mediaPlayer.Stop();
        }

        private void LoadStations()
        {
            string content = string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(_stationsFile))
                {
                    while ((content = sr.ReadLine()) != null)
                    {
                        try
                        {
                            var arr = content.Split(';');
                            Stations.Add(new Station
                            {
                                Name = arr[0],
                                Address = arr[1]
                            });
                        }
                        catch (Exception ex)
                        {
                            OkDialog.ShowDialog(ex.Message, Resources.fatalError, EFontAwesomeIcon.Solid_MinusCircle);
                        }
                    }
                }
            }
            catch { /*Ignore*/ }
        }

        private void PrepareDialog(string title, string initName, string initAddress, Action<Tuple<string, string>> onOkPressed)
        {
            var dialog = new AddEditDialog();
            var vm = new AddEditViewModel((tuple) =>
            {
                dialog.Close();
                if (tuple != null)
                {
                    onOkPressed(tuple);
                    _unsavedChanges = true;
                    OnPropertyChanged(nameof(Stations));
                }
            })
            {
                Title = title,
                Name = initName,
                Address = initAddress
            };
            dialog.DataContext = vm;

            dialog.ShowDialog();
        }

        public void OnClose(CancelEventArgs e)
        {
            if (!_unsavedChanges)
            {
                return;
            }
            var result = YesNoDialog.ShowDialog(Resources.unsavedChangesWarning, Resources.question, EFontAwesomeIcon.Solid_ExclamationTriangle);
            if (!result)
            {
                e.Cancel = true;
            }
        }
    }
}
