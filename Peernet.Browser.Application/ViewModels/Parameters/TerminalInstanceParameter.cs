﻿using System.ComponentModel;
using System.Threading;

namespace Peernet.Browser.Application.ViewModels.Parameters
{
    public class TerminalInstanceParameter : INotifyPropertyChanged
    {
        private string commandLineInput = string.Empty;
        private string commandLineOutput = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public TerminalInstanceParameter()
        {
            CancellationTokenSource = new CancellationTokenSource();
        }

        public string CommandLineOutput
        {
            get => commandLineOutput;
            set
            {
                commandLineOutput = value;
                PropertyChanged?.Invoke(this, new(nameof(CommandLineOutput)));
            }
        }

        public string CommandLineInput
        {
            get => commandLineInput;
            set
            {
                commandLineInput = value;
                PropertyChanged?.Invoke(this, new(nameof(CommandLineInput)));
            }
        }

        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}