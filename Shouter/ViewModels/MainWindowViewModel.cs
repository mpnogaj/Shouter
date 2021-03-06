using FontAwesome5;
using Shouter.Models;
using Shouter.Properties;
using Shouter.ViewModels.Commands;
using Shouter.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Shouter.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const char Separator = ';';
        private readonly string _stationsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        private readonly string _stationsFile;

        private bool _unsavedChanges;
        private string _currentStationName = string.Empty;

        public event EventHandler PlayRequest;
        public event EventHandler StopRequest;
        public event EventHandler ToggleMuteRequest;
        public event EventHandler ChangeVolumeRequest;

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

        private double _volume = 0.5;
        public double Volume
        {
            get => _volume * 100.0;
            set
            {
                value /= 100.0;
                if(_volume != value)
                {
                    ChangeVolumeRequest?.Invoke(value, EventArgs.Empty);
                }
                CheckAndSetProperty(ref _volume, value, nameof(Volume));
            }
        }

        #endregion

        #region Commands

        public RelayCommand PlayCommand { get; }
        public RelayCommand ForcePlayCommand { get; }
        public RelayCommand StopCommand { get; }
        public RelayCommand ToggleMuteCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ToggleTray { get; }

        #endregion

        public MainWindowViewModel()
        {
            _stationsFile = $@"{_stationsDirectory}\Shouter\stations.csv";

            Stations = new ObservableCollection<Station>();
            LoadStations();

            ToggleTray = new RelayCommand(() => MinimizeToTray = !MinimizeToTray);

            PlayCommand = new RelayCommand(() =>
            {
                Play(SelectedStation);
            }, () => !IsPlaying && SelectedStation != null);

            ToggleMuteCommand = new RelayCommand(() => 
            {
                ToggleMuteRequest?.Invoke(null, EventArgs.Empty);
            });
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
                PrepareDialog(Resources.addStation, Resources.add, string.Empty, string.Empty, (tuple) =>
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
                PrepareDialog(Resources.editStation, Resources.edit, SelectedStation.Name, SelectedStation.Address, (tuple) =>
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
                var result = YesNoDialog.ShowDialog(
                    Resources.deleteStationQuestion,
                    Resources.question,
                    EFontAwesomeIcon.Solid_QuestionCircle);
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
                OkDialog.ShowDialog(
                    Resources.changesSavedSuccessfully,
                    Resources.success,
                    EFontAwesomeIcon.Solid_InfoCircle);
                _unsavedChanges = false;
            });
        }

        public void UpdateStatus(MediaStatus status)
        {
            switch (status)
            {
                case MediaStatus.Playing:
                    IsPlaying = true;
                    Status = string.Format(Resources.playing_station, _currentStationName);
                    break;
                case MediaStatus.Stopped:
                    IsPlaying = false;
                    Status = Resources.ready;
                    break;
                case MediaStatus.Connecting:
                    Status = Resources.connecting;
                    break;
                case MediaStatus.Error:
                    Status = Resources.connectionLostWrongAddress;
                    IsPlaying = false;
                    break;
            }
        }

        private void Play(Station s)
        {
            try
            {
                _currentStationName = s.Name;
                PlayRequest?.Invoke(s, EventArgs.Empty);
            }
            catch (UriFormatException)
            {
                OkDialog.ShowDialog(Resources.invalidAddress, Resources.error, EFontAwesomeIcon.Solid_MinusCircle);
            }
        }

        private void Stop()
        {
            _currentStationName = string.Empty;
            StopRequest?.Invoke(null, EventArgs.Empty);
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

        private void PrepareDialog(
            string title,
            string okText,
            string initName,
            string initAddress,
            Action<Tuple<string, string>> onOkPressed)
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
                OkText = okText,
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
            var result = YesNoDialog.ShowDialog(
                Resources.unsavedChangesWarning,
                Resources.question,
                EFontAwesomeIcon.Solid_ExclamationTriangle);
            if (!result)
            {
                e.Cancel = true;
            }
        }
    }
}
