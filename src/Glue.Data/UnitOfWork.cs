using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Glue.Lib;
using Glue.Lib.Threading;

namespace Glue.Data
{
    public class UnitOfWork
    {
        #region Static

        /// <summary>
        /// Create new UnitOfWork-instance with a specified IsolationLevel
        /// </summary>
        public static UnitOfWork Create(IMappingProvider mappingProvider, IDbConnection connection, IsolationLevel isolationLevel)
        {
            Log.Debug("UnitOfWork: Creating new UnitOfWork with IsolationLevel {0}", isolationLevel.ToString());
            
            UnitOfWork uow = new UnitOfWork();
            uow._isolationLevel = isolationLevel;
            uow._connection = connection;
            uow._mappingProvider = mappingProvider;
    
            return uow;
        }

        #endregion

        #region Fields 

        private List<IActiveRecord> _dirtyRecords = new List<IActiveRecord>();
        private List<IActiveRecord> _newRecords = new List<IActiveRecord>();
        private List<IActiveRecord> _cleanRecords= new List<IActiveRecord>();
        private List<IActiveRecord> _deletedRecords = new List<IActiveRecord>();
        
        private IsolationLevel _isolationLevel;

        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private IMappingProvider _mappingProvider;

        private bool _leaveConnectionOpen = false;

        #endregion

        #region Public properties

        /// <summary>
        /// Transaction IsolationLevel
        /// </summary>
        public IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
        }

        /// <summary>
        /// Database connection
        /// </summary>
        public IDbConnection Connection
        {
            get { return _connection; }
        }

        /// <summary>
        /// Return the active transaction
        /// </summary>
        public IDbTransaction Transaction
        {
            get { return _transaction; }
        }

        #endregion

        #region Methods

        protected UnitOfWork()
        {
        }

        /// <summary>
        /// Register record for Insert
        /// </summary>
        /// <param name="activeRecord"></param>
        public void RegisterNew(IActiveRecord activeRecord)
        {
            if(_dirtyRecords.Contains(activeRecord))
            {
                throw new ApplicationException("UnitOfWork already contains this record as dirty!");
            }
            else if (_deletedRecords.Contains(activeRecord))
            {
                throw new ApplicationException("UnitOfWork already contains this record as deleted!");
            }
            
            if (!_newRecords.Contains(activeRecord))
            {
                Log.Debug("UnitOfWork: Registering {0} for Insert", activeRecord.GetType().FullName);
                _newRecords.Add(activeRecord);
            }
        }

        /// <summary>
        /// Register record for Update
        /// </summary>
        /// <param name="activeRecord"></param>
        public void RegisterDirty(IActiveRecord activeRecord)
        {
            if (_deletedRecords.Contains(activeRecord))
            {
                throw new ApplicationException("UnitOfWork already contains this record as deleted!");
            }
                
            // See if the record is not new and not already scheduled for update
            if ((!_newRecords.Contains(activeRecord)) || (!_dirtyRecords.Contains(activeRecord)))
            {
                Log.Debug("UnitOfWork: Registering {0} for Update", activeRecord.GetType().FullName);
                _dirtyRecords.Add(activeRecord);
            }
        }

        /// <summary>
        /// Register record as clean
        /// </summary>
        /// <param name="activeRecord"></param>
        public void RegisterClean(IActiveRecord activeRecord)
        {
            // don't do anything...
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeRecord"></param>
        public void RegisterDeleted(IActiveRecord activeRecord)
        {
            if (_newRecords.Contains(activeRecord))
            {
                // just remove from new records.
                _newRecords.Remove(activeRecord);
                return;
            } 

            // remove from the dirty collection, just in case
            _dirtyRecords.Remove(activeRecord);
            
            // if this record is not already registered for deletion, register it!
            if (!_deletedRecords.Contains(activeRecord))
            {
                Log.Debug("UnitOfWork: Registering {0} for Deletion", activeRecord.GetType().FullName);
                _deletedRecords.Add(activeRecord);
            }
        }

        /// <summary>
        /// Commit the UnitOfWork to the database using an underlying IDbTransaction
        /// </summary>
        public void Commit()
        {
            Log.Debug("UnitOfWork: Committing Transaction with IsolationLevel " + IsolationLevel.ToString());
            Log.Debug("UnitOfWork: Inserting {0} objects, Updating {1} objects, Deleting {2} objects.", 
                _newRecords.Count, _dirtyRecords.Count, _deletedRecords.Count);

            // open connection
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
                _leaveConnectionOpen = false;
            }

            // begin transaction
            _transaction = Connection.BeginTransaction(_isolationLevel);

            try
            {
                InsertNew();
                UpdateDirty();
                DeleteRemoved();

                _transaction.Commit();

                Log.Debug("UnitOfWork: Commit succeeded!");
            }
            catch (Exception e)
            {
                Log.Warn("UnitOfWork: Exception during transaction, beginning rollback!");

                // rollback!
                _transaction.Rollback();

                Log.Warn("UnitOfWork: Rollback succeeded, re-throwing exception.");
                Log.Warn(e);
                // and throw 
                throw e;
            }
            finally
            {
                if (!_leaveConnectionOpen)
                    Connection.Close();
            }
        }

        /// <summary>
        /// Close UnitOfWork
        /// </summary>
        public void Close()
        {
            // cleanup
            _deletedRecords.Clear();
            _dirtyRecords.Clear();
            _newRecords.Clear();
            _cleanRecords.Clear();
        }

        /// <summary>
        /// Insert all records scheduled for insertion
        /// </summary>
        protected void InsertNew()
        {
            foreach (object record in _newRecords)
            {
                Log.Debug("Inserting {0}", record.GetType().FullName);
                this._mappingProvider.Insert(record);
                //record.Insert(this);
            }
        }

        /// <summary>
        /// Update all records scheduled for update
        /// </summary>
        protected void UpdateDirty()
        {
            foreach (IActiveRecord record in _dirtyRecords)
            {
                Log.Debug("Updating {0}", record.GetType().FullName);
                record.Update(this);
            }
        }

        /// <summary>
        /// Delete all records registered for deletion
        /// </summary>
        protected void DeleteRemoved()
        {
            foreach (IActiveRecord record in _deletedRecords)
            {
                Log.Debug("Deleting {0}", record.GetType().FullName);
                record.Delete(this);
            }
        }

        #endregion

    }
}
