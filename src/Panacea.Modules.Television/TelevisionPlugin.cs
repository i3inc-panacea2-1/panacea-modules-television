using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Television.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Television
{
    public class TelevisionPlugin : ICallablePlugin
    {
        private readonly PanaceaServices _core;

        public TelevisionPlugin(PanaceaServices core)
        {
            _core = core;
        }
        public Task BeginInit()
        {
            return Task.CompletedTask;
        }
        TelevisionViewModel _page;
        public void Call()
        {
            try
            {
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    _page = _page ?? new TelevisionViewModel(_core);
                    ui.Navigate(_page);
                    
                }
            }
            catch
            {
            }
        }

        public void Dispose()
        {

        }

        public Task EndInit()
        {
            return Task.CompletedTask;
        }

        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }
}
