using Dmc.Siemens.Common;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Portal;
using Dmc.Siemens.Portal.Plc;
using Dmc.Wpf.Collections;
using Dmc.Wpf.Concurrent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Dmc.Siemens.AlarmGenerator
{
    public class ProjectManager : PortalPlc
    {

        #region Constructors

        public ProjectManager() : base()
        {
            UIDispatcher.BeginInvoke(() =>
            {
                BindingOperations.EnableCollectionSynchronization(this.DataBlocks, this._dataBlocksLock);
                BindingOperations.EnableCollectionSynchronization(this.UdtNames, this._udtNamesLock);
                BindingOperations.EnableCollectionSynchronization(this.ImportedUdts, this._importedUdtsLock);
            });
            this.DataBlocks.CollectionChanged += new NotifyCollectionChangedEventHandler((o, e) => this.FirePropertyChanged("IsReadyForExport"));
            ((INotifyCollectionChanged)this.UdtNames).CollectionChanged += new NotifyCollectionChangedEventHandler((o, e) => this.FirePropertyChanged("IsReadyForExport"));
            this.TagTables.Add(this.DefaultTagTable);
        }

        #endregion

        #region Public Properties

        public ObservableCollection<DataBlock> DataBlocks { get; } = new ObservableCollection<DataBlock>();
        public ObservableDictionary<string, bool> UdtNames { get; } = new ObservableDictionary<string, bool>();
        public PlcTagTable DefaultTagTable { get; } = new PlcTagTable("Default tag table");
        public List<UserDataType> ImportedUdts { get; } = new List<UserDataType>();
        public static ObservableCollection<DataType> SupportedConstantDataTypes { get; } = new ObservableCollection<DataType>()
        {
            DataType.BOOL,
            DataType.BYTE,
            DataType.CHAR,
            DataType.DINT,
            DataType.DWORD,
            DataType.INT,
            DataType.REAL,
            DataType.STRING,
            DataType.TIME,
            DataType.WORD
        };

        public bool IsReadyForExport => (this.DataBlocks.Count > 0) ? !this.UdtNames.ObservableValues.Contains(false) : false;

        private TiaPortalVersion _tiaPortalVersion = TiaPortalVersion.V15;
        public TiaPortalVersion TiaPortalVersion
        {
            get => this._tiaPortalVersion;
            set
            {
                this.SetProperty(ref this._tiaPortalVersion, value);
                this.SettingsUpdated(this, null);
            }
        }

        public IEnumerable<IAutomationObject> AutomationObjects { get; }

        #endregion

        #region Private Fields

        private readonly object _udtNamesLock = new object();
        private readonly object _dataBlocksLock = new object();
        private readonly object _importedUdtsLock = new object();

        #endregion

        #region Public Methods

        public void ImportFromFile(string filePath)
        {
            this.OrganizeSourceObjects(SourceParser.FromFile(filePath));
        }

        public void ImportFromText(string text)
        {
            this.OrganizeSourceObjects(SourceParser.FromText(text));
        }

        public void DeleteDataBlock(DataBlock dataBlock)
        {
            this.DataBlocks.Remove(dataBlock);

            // Delete all of the UDT references in the block
            this.DeleteDataEntryUdts(dataBlock.Children);

        }

        public void DeleteUdt(UserDataType dataType)
        {
            this.DeleteUdt(dataType.Name);

        }

        public void DeleteUdt(string name)
        {
            if (this.UdtNames.ContainsKey(name))
            {
                if (this.DataBlocks.Any(d => d.Children.Any(e => e.DataTypeName == name)))
                {
                    this.UdtNames[name] = false;
                }
                else
                {
                    this.UdtNames.Remove(name);
                }
            }

            this.ImportedUdts.RemoveAll(u => u.Name == name);

        }

        #endregion

        #region Private Methods

        private void OrganizeSourceObjects(IEnumerable<IParsableSource> sources)
        {
            foreach (var source in sources)
            {
                if (source is DataEntity)
                {
                    var entity = source as DataEntity;
                    if (!this.ImportedUdts.Any(u => u.Name == entity.Name) && !this.DataBlocks.Any(d => d.Name == entity.Name))
                    {
                        this.OrganizeDataEntry(entity.Children);

                        if (source is DataBlock)
                        {
                            this.DataBlocks.Add(source as DataBlock);
                            this.Blocks[BlockType.DataBlock].Add(source as IBlock);
                        }
                        else if (source is UserDataType && !string.IsNullOrWhiteSpace(source.Name))
                        {
                            this.ImportedUdts.Add(source as UserDataType);
                            this.UserDataTypes.Add(source as UserDataType);

                            if (this.UdtNames.ContainsKey(source.Name))
                            {
                                this.UdtNames[source.Name] = true;
                            }
                            else
                            {
                                this.UdtNames.Add(source.Name, true);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Duplicate source detected. Type: {entity.DataType}, Name: {entity.Name}");
                    }
                }
            }

            this.FirePropertyChanged("IsReadyForExport");
        }

        private void OrganizeDataEntry(IEnumerable<DataEntry> dataEntries)
        {
            // TODO: Make this parallelizable (race condition between .ContainsKey() and .Add()
            //Parallel.ForEach(dataEntries, data =>
            foreach (var data in dataEntries)
            {
                if (data.DataType == DataType.UDT && !string.IsNullOrWhiteSpace(data.DataTypeName))
                {
                    if (!this.UdtNames.ContainsKey(data.DataTypeName))
                    {
                        this.UdtNames.Add(data.DataTypeName, false);
                    }
                }
                else if (data.DataType == DataType.STRUCT)
                {
                    this.OrganizeDataEntry(data.Children);
                }
            };//);
        }

        private void DeleteDataEntryUdts(IEnumerable<DataEntry> dataEntries)
        {
            Parallel.ForEach<DataEntry>(dataEntries.Where(d => d.DataType == DataType.UDT || d.DataType == DataType.STRUCT), entry =>
            {
                if (entry.DataType == DataType.UDT)
                {
                    // Check if the UDT is still waiting to be imported
                    if (this.UdtNames.ContainsKey(entry.DataTypeName) && !this.UdtNames[entry.DataTypeName])
                    {
                        // Check other data blocks to make sure this data type isn't needed anymore
                        if (!this.DataBlocks.Any(d => d.Children.Any(e => e.DataTypeName == entry.DataTypeName)))
                        {
                            this.UdtNames.Remove(entry.DataTypeName);
                        }
                    }
                    else if (!this.UdtNames.ContainsKey(entry.DataTypeName))
                    {
                        throw new IndexOutOfRangeException($"This should not happen.  All UDTs should exist in the UdtName list.  Name: {entry.DataTypeName}");
                    }
                }
                else // Only other case is if it is a struct, in which case we call this method recursively
                {
                    this.DeleteDataEntryUdts(entry.Children);
                }
            });
        }

        #endregion

        #region Events

        public event EventHandler SettingsUpdated;

        #endregion

    }
}
