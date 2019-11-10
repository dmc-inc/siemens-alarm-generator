using Dmc.Siemens.Common.Export;
using Dmc.Siemens.Common.Export.Base;
using Dmc.Siemens.Common.Plc;
using Dmc.Wpf;
using Dmc.Wpf.Collections;
using Dmc.Wpf.Commands;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace Dmc.Siemens.AlarmGenerator
{
    public class MainWindowViewModel : NotifyPropertyChanged
    {

        public ProjectManager ProjectManager { get; } = new ProjectManager();

        private bool _isDataBlockSelected;
        private bool _isPathSelected = true;
        private readonly OpenFileDialog _sourceOpenFileDialog = new OpenFileDialog()
        {
            CheckPathExists = true,
            CheckFileExists = true,
            Filter = "Source Files (*.db, *.udt, *.txt, *.awl)|*.db;*.udt;*.txt;*.awl",
            Multiselect = true,
            Title = "Select source files"
        };
        private readonly SaveFileDialog _exportSaveFileDialog = new SaveFileDialog()
        {
            AddExtension = true,
            CheckPathExists = true,
            CheckFileExists = false,
            Filter = "WinCC Import File (*.xlsx)|*.xlsx",
            ValidateNames = true
        };

        private bool _isPopupVisible;
        public bool IsPopupVisible
        {
            get => this._isPopupVisible;
            set => this.SetProperty(ref this._isPopupVisible, value);
        }

        private string _importString;
        public string ImportString
        {
            get => this._importString;
            set => this.SetProperty(ref this._importString, value);
        }

        private bool _isErrorActive;
        public bool IsErrorActive
        {
            get => this._isErrorActive;
            set => this.SetProperty(ref this._isErrorActive, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => this._errorMessage;
            set => this.SetProperty(ref this._errorMessage, value);
        }

        private ObservableHashSet<string> _importPaths = new ObservableHashSet<string>();
        public ObservableHashSet<string> ImportPaths
        {
            get => this._importPaths;
            set => this.SetProperty(ref this._importPaths, value);
        }

        public DataBlock SelectedDataBlock { get; set; }
        public KeyValuePair<string, bool> SelectedUdt { get; set; }

        public ICommand TogglePopupVisibilityCommand { get; }
        public ICommand AcknowledgeErrorCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand BrowseCommand { get; }
        public ICommand DataBlockFocusCommand { get; }
        public ICommand UdtFocusCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand TogglePathSelectedCommand { get; }

        public MainWindowViewModel()
        {
            this.TogglePopupVisibilityCommand = new RelayCommand(o => this.TogglePopupVisibility());
            this.AcknowledgeErrorCommand = new RelayCommand(o => this.IsErrorActive = false);
            this.ImportCommand = new RelayCommand(o => this.Import());
            this.ExportCommand = new RelayCommand(o => this.Export());
            this.BrowseCommand = new RelayCommand(o => this.Browse());
            this.DataBlockFocusCommand = new RelayCommand(o => this._isDataBlockSelected = true);
            this.UdtFocusCommand = new RelayCommand(o => this._isDataBlockSelected = false);
            this.DeleteCommand = new RelayCommand(o => this.DeleteEntry());
            this.TogglePathSelectedCommand = new RelayCommand(o => this.TogglePathSelected());
        }

        private void TogglePopupVisibility()
        {
            if (!this.IsPopupVisible)
            {
                this.ImportPaths.Clear();
            }
            this.IsPopupVisible = !this.IsPopupVisible;
        }

        private void TogglePathSelected()
        {
            this._isPathSelected = !this._isPathSelected;
        }

        private void Browse()
        {
            if (this._sourceOpenFileDialog.ShowDialog() == true)
            {
                foreach (var file in this._sourceOpenFileDialog.FileNames)
                {
                    this.ImportPaths.Add(file);
                }
            }
        }

        private void Import()
        {
            try
            {
                this.IsPopupVisible = false;

                if (this._isPathSelected)
                {
                    // TODO: Maybe one day make this compatible with Parallel.ForEach()
                    foreach (var file in this.ImportPaths)
                    {
                        if (File.Exists(file))
                        {
                            this.ProjectManager.ImportFromFile(file);
                        }
                    }
                }
                else
                {
                    this.ProjectManager.ImportFromText(this.ImportString);
                }
            }
            catch (Exception e)
            {
                this.HandleException(e);
            }
        }

        private void Export()
        {
            if (this._exportSaveFileDialog.ShowDialog() == true)
            {
                try
                {
                    WinccConfiguration.Create(this.ProjectManager.DataBlocks, this._exportSaveFileDialog.FileName,
                        this.ProjectManager, WinccExportType.ComfortAdvanced, this.ProjectManager.TiaPortalVersion);
                }
                catch (Exception e)
                {
                    this.HandleException(e);
                }
            }
            
        }

        private void DeleteEntry()
        {
            try
            {
                if (this._isDataBlockSelected)
                {
                    this.ProjectManager.DeleteDataBlock(this.SelectedDataBlock);
                }
                else
                {
                    this.ProjectManager.DeleteUdt(this.SelectedUdt.Key);
                }
            }
            catch (Exception e)
            {
                this.HandleException(e);
            }
        }

        private void HandleException(Exception e)
        {
            this.ErrorMessage = e.Message;
            this.IsErrorActive = true;
        }

    }
}
