using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.Media;
using Panacea.Modularity.ScreenCast;
using Panacea.Modularity.TerminalPairing;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Television.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Television.ViewModels
{
    [View(typeof(RemoteControl))]
    class RemoteControlViewModel : ViewModelBase
    {
        private readonly PanaceaServices _core;
        public event EventHandler Disconnected;
        public event EventHandler Stopped;
        public RemoteControlViewModel(PanaceaServices core)
        {

            _core = core;
            if (_core.TryGetPairing(out IBoundTerminalManager pair))
            {
                pair.GetBoundTerminal().Disconnected += RemoteControlViewModel_Disconnected;
            }
            if (_core.TryGetScreenCast(out IScreenCastPlayer screencast2))
            {
                screencast2.Stopped += Screencast_Stopped;
                screencast2.VolumeChanged += Screencast2_VolumeChanged;
            }
            VolUpCommand = new RelayCommand(args =>
            {
                if (_core.TryGetScreenCast(out IScreenCastPlayer screencast))
                {
                    var volume = RoundBy5Up(Volume) + 5;
                    screencast.SetVolume(volume);
                }
            },
            args =>
            {
                return Volume != -1 && Volume < 100;
            });

            VolDownCommand = new RelayCommand(args =>
            {
                if (_core.TryGetScreenCast(out IScreenCastPlayer screencast))
                {
                    var volume = RoundBy5Down(Volume) - 5;
                    screencast.SetVolume(volume);
                }
            },
            args =>
            {
                return Volume > 0;
            });
            StopCommand = new RelayCommand(args =>
            {
                if (_core.TryGetScreenCast(out IScreenCastPlayer screencast))
                {
                    screencast.Stop();
                }
            });

            MuteCommand = new RelayCommand(args =>
            {
                _lastVolume = Volume;
                if (_core.TryGetScreenCast(out IScreenCastPlayer screencast))
                {
                    screencast.SetVolume(0);
                }
            },
            args => Volume != 0);

            UnmuteCommand = new RelayCommand(args =>
            {
                if (_core.TryGetScreenCast(out IScreenCastPlayer screencast))
                {
                    if (_lastVolume == 0) _lastVolume = 10;
                    screencast.SetVolume(_lastVolume);
                }
            },
           args => Volume == 0);
        }
        int _lastVolume;
        private void Screencast2_VolumeChanged(object sender, EventArgs e)
        {
            if (_core.TryGetScreenCast(out IScreenCastPlayer screencast2))
            {
                Volume = screencast2.Volume;
                VolDownCommand.RaiseCanExecuteChanged();
                VolUpCommand.RaiseCanExecuteChanged();
                MuteCommand.RaiseCanExecuteChanged();
                UnmuteCommand.RaiseCanExecuteChanged();
            }
        }

        private void Screencast_Stopped(object sender, EventArgs e)
        {
            Stopped?.Invoke(this, null);
        }

        private void RemoteControlViewModel_Disconnected(object sender, EventArgs e)
        {
            Disconnected?.Invoke(this, null);
        }

        public void Play(MediaItem item)
        {
            if (_core.TryGetScreenCast(out IScreenCastPlayer screencast))
            {
                screencast.Play(item);
            }
        }

        public override void Activate()
        {
            base.Activate();
        }

        int RoundBy5Down(int v)
        {
            return (int)(v / 10 * 10.0 + Math.Ceiling(v % 10 / 5.0) * 5);
        }

        int RoundBy5Up(int v)
        {
            return (int)(v / 10 * 10.0 + Math.Floor(v % 10 / 5.0) * 5);
        }
        int _volume = -1;
        int Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
            }
        }
        public RelayCommand VolUpCommand { get; }

        public RelayCommand VolDownCommand { get; }

        public RelayCommand MuteCommand { get; }

        public RelayCommand UnmuteCommand { get; }

        public RelayCommand StopCommand { get; }
    }
}
