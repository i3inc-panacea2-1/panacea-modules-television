using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.AudioManager;
using Panacea.Modularity.Billing;
using Panacea.Modularity.Media;
using Panacea.Modularity.MediaPlayerContainer;
using Panacea.Modularity.UiManager;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Panacea.Modularity.ScreenCast;
using Panacea.Modularity.TerminalPairing;

namespace Panacea.Modules.Television
{
    [View(typeof(TelevisionPage))]
    class TelevisionViewModel : ViewModelBase, IMediaTraverser
    {
        private bool _failedToLoadChannels;
        private MediaItem _defaultChannel;
        IMediaResponse _currentResponse;
        private bool _embedded;

        //private IKeyboardMouseEvents m_GlobalHook;

        public TelevisionViewModel(PanaceaServices core)
        {
            _core = core;
            Host = new ContentControl();
            FullscreenCommand = new RelayCommand(args =>
            {
                if (_core.TryGetMediaPlayerContainer(out IMediaPlayerContainer player))
                {
                    player.GoFullscreen();
                }
            });
            ChannelUpCommand = new RelayCommand(args =>
            {
                if (_core.TryGetMediaPlayerContainer(out IMediaPlayerContainer player))
                {
                    _response?.Next();
                }
            });
            ChannelDownCommand = new RelayCommand(args =>
            {
                if (_core.TryGetMediaPlayerContainer(out IMediaPlayerContainer player))
                {
                    _response?.Previous();
                }
            });
            MuteCommand = new RelayCommand(args =>
            {
                if (_core.TryGetAudioManager(out IAudioManager audio))
                {
                    _volume = audio.SpeakersVolume;
                    audio.SpeakersVolume = 0;
                }
            },
            args =>
            {
                if (_core.TryGetAudioManager(out IAudioManager audio))
                {
                    return audio.SpeakersVolume != 0;
                }
                return false;
            });

            UnmuteCommand = new RelayCommand(args =>
            {
                if (_core.TryGetAudioManager(out IAudioManager audio))
                {
                    if (_volume <= 0) _volume = 10;
                    audio.SpeakersVolume = _volume;
                }
            },
            args =>
            {
                if (_core.TryGetAudioManager(out IAudioManager audio))
                {
                    return audio.SpeakersVolume == 0;
                }
                return false;
            });

            VolDownCommand = new RelayCommand(args =>
            {
                if (_core.TryGetAudioManager(out IAudioManager audio))
                {
                    audio.SpeakersVolume = RoundBy5Down(audio.SpeakersVolume) - 5;
                }
            });

            VolUpCommand = new RelayCommand(args =>
            {
                if (_core.TryGetAudioManager(out IAudioManager audio))
                {
                    audio.SpeakersVolume = RoundBy5Up(audio.SpeakersVolume) + 5;
                }
            });
            StopCommand = new RelayCommand(args =>
            {
                _response.Stop();
            });
            ScreencastCommand = new RelayCommand(args =>
            {
                {
                    if (_core.TryGetScreenCast(out IScreenCastPlayer screencast))
                    {

                    }
                }
            },
            args =>
            {
                if (_core.TryGetPairing(out IBoundTerminalManager bound))
                {
                    return bound.IsBound();
                    
                }
                return false;
            });
        }
        int RoundBy5Down(int v)
        {
            return (int)(v / 10 * 10.0 + Math.Ceiling(v % 10 / 5.0) * 5);
        }

        int RoundBy5Up(int v)
        {
            return (int)(v / 10 * 10.0 + Math.Floor(v % 10 / 5.0) * 5);
        }
        int _volume;

        public ContentControl Host { get; set; }

        ObservableCollection<MediaItem> _channels;
        public ObservableCollection<MediaItem> Channels
        {
            get => _channels;
            set
            {
                _channels = value;
                OnPropertyChanged();
            }
        }

        bool _hasCaptions;
        public bool HasCaptions
        {
            get => _hasCaptions;
            set
            {
                _hasCaptions = value;
                OnPropertyChanged();
            }
        }

        bool _captionsEnabled;
        public bool CaptionsEnabled
        {
            get => _captionsEnabled;
            set
            {
                _captionsEnabled = value;
                _response?.SetSubtitles(_captionsEnabled);
            }
        }

        MediaItem _selectedChannel;
        public MediaItem SelectedChannel
        {
            get => _selectedChannel;
            set
            {
                if (_selectedChannel == value) return;
                _selectedChannel = value;
                OnPropertyChanged();
                SetChannel(value);
            }
        }


        public override async void Activate()
        {
            if (Channels == null)
            {
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    await ui.DoWhileBusy(async () =>
                    {
                        await LoadChannels();
                    });
                }
            }

            if (_defaultChannel != null)
            {
                SelectedChannel = Channels.First(c => c.Id == _defaultChannel.Id);
            }
        }
        public override void Deactivate()
        {

        }

        public void Next()
        {
            if (SelectedChannel == null)
            {
                if (Channels.Any())
                {
                    SelectedChannel = Channels.First();
                    return;
                }
            }
            if (!Channels.Any()) return;
            var index = Channels.IndexOf(SelectedChannel);
            if (index >= Channels.Count - 1) return;
            SetChannel(Channels[++index]);
        }

        public void Previous()
        {
            if (SelectedChannel == null)
            {
                if (Channels.Any())
                {
                    SelectedChannel = Channels.Last();
                    return;
                }
            }
            if (!Channels.Any()) return;
            var index = Channels.IndexOf(SelectedChannel);
            if (index <= 0) return;
            SetChannel(Channels[--index]);
        }

        private async Task LoadChannels()
        {
            try
            {
                var response = await _core.HttpClient.GetObjectAsync<GetChannelsResponse>("television/get_channels/");
                if (response.Success)
                {
                    var result = response.Result;
                    Channels = new ObservableCollection<MediaItem>(result.Television.Channels.Select(c => c.GetChannel()));
                    _defaultChannel = result.Television.Channels.FirstOrDefault(c => c.IsDefault)?.GetChannel();
                }
                _failedToLoadChannels = false;
            }
            catch
            {
                _failedToLoadChannels = true;
            }
        }

        private async Task SetChannel(MediaItem c)
        {
            if (c == null) return;
            if (c == _currentChannel) return;
            try
            {

                if (c.Id == _defaultChannel?.Id)
                {
                    try
                    {
                        //_webSocket.PopularNotify("Television", "Channel", c.Id);
                    }
                    catch
                    {
                        //ignore
                    }
                    PlayChannel(c, null);
                    return;
                }
                else
                {

                    if (_core.TryGetBilling(out IBillingManager billing))
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SelectedChannel = _currentChannel;
                        }), DispatcherPriority.Send);
                        var serv = await billing.GetOrRequestServiceForItemAsync("Television", "Television", c);
                        if (serv != null)
                        {
                            //_webSocket.PopularNotify("Television", "Channel", c.Id);
                            PlayChannel(c, serv);
                        }
                    }
                }
            }
            catch
            {
                //ignore
            }
            return;
        }

        MediaItem _currentChannel;
        private readonly PanaceaServices _core;

        private void PlayChannel(MediaItem c, Service serv)
        {
            HasCaptions = false;
            if (_core.TryGetUiManager(out IUiManager ui) && _core.TryGetMediaPlayerContainer(out IMediaPlayerContainer player))
            {
                if (ui.CurrentPage != this)
                {
                    ui.Navigate(this);
                }
                if (!_embedded)
                {
                    _embedded = true;
                }
                if (Channels?.Any(ch => ch?.Id == c?.Id) == true)
                {
                    _currentChannel = Channels?.First(ch => ch?.Id == c?.Id);
                    SelectedChannel = _currentChannel;
                }
                else
                {
                    return;
                }
                if (_response != null)
                {
                    _response.Stopped -= _response_Stopped;
                    _response.Ended -= _response_Ended;
                    _response.Error -= _response_Error;
                }
                _response = _currentResponse = player.Play(
                    new MediaRequest(c)
                    {
                        MediaPlayerPosition = MediaPlayerPosition.Embedded,
                        MediaPlayerHost = Host,
                        AllowPip = true,
                        ShowControls = false,
                        MediaTraverser = this
                    });
                _response.HasSubtitlesChanged += _response_HasSubtitlesChanged;
                _response.Stopped += _response_Stopped;
                _response.Ended += _response_Ended;
                _response.Error += _response_Error;
            }

        }

        private void _response_HasSubtitlesChanged(object sender, bool e)
        {
            HasCaptions = e;
        }

        private void _response_Error(object sender, Exception e)
        {
            _currentChannel = SelectedChannel = null;
        }

        private void _response_Ended(object sender, EventArgs e)
        {
            _currentChannel = SelectedChannel = null;

        }

        private void _response_Stopped(object sender, EventArgs e)
        {
            _currentChannel = SelectedChannel = null;
        }

        IMediaResponse _response;

        public RelayCommand FullscreenCommand { get; }

        public RelayCommand ChannelUpCommand { get; }

        public RelayCommand ChannelDownCommand { get; }

        public RelayCommand VolUpCommand { get; }

        public RelayCommand VolDownCommand { get; }

        public RelayCommand MuteCommand { get; }

        public RelayCommand UnmuteCommand { get; }

        public RelayCommand StopCommand { get; }

        public RelayCommand ScreencastCommand { get; }
    }
}
