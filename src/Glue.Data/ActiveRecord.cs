using System;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Glue.Data.Mapping;

namespace Glue.Data
{
    /// <summary>
    /// Implementation of the ActiveRecord design pattern to wrap a database table or view into a class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActiveRecord : IActiveRecord
    {
        #region Provider
        
        /// <summary>
        /// Return the MappingProvider for this class
        /// </summary>
        /// <remarks>
        /// The MappingProvider can to be declared in the Table-attribute.
        
        /// If the MappingProvider is not declared, the default MappingProvider is initialized
        /// from the element "dataprovider" (which, obviously, has to be there...).
        /// </remarks>
        /// <example>
        /// [Table(MappingProvider="dataprovider-account")]
        /// public class Account : ActiveRecord
        /// {
        /// [...]
        /// }
        /// </example>
        public IMappingProvider Provider
        {
            get 
            {
                return MappingProvider.Get(this.GetType()); 
            }
        }

        #endregion 

        #region IActiveRecord Members

        /// <summary>
        /// Insert the current record
        /// </summary>
        /// <remarks>
        /// Shortcut for Insert((UnitOfWork) null)
        /// </remarks>
        public void Insert()
        {
            Insert(null);
        }

        /// <summary>
        /// Insert the current record
        /// </summary>
        /// <param name="unitOfWork">Reference to UnitOfWork</param>
        /// <remarks>
        /// Override this method if you need to extend the Insert-behaviour. UnitOfWork can be null.
        /// </remarks>
        public virtual void Insert(UnitOfWork unitOfWork)
        {
            Provider.Insert(unitOfWork, this);
        }

        /// <summary>
        /// Update the current record
        /// </summary>
        /// <remarks>
        /// Shortcut for Update((UnitOfWork) null)
        /// </remarks>
        public void Update()
        {
            Update(null);
        }

        /// <summary>
        /// Update the current record
        /// </summary>
        /// <param name="unitOfWork">Reference to UnitOfWork</param>
        /// <remarks>
        /// Override this method if you need to extend the Update-behaviour. UnitOfWork can be null.
        /// </remarks>
        public virtual void Update(UnitOfWork unitOfWork)
        {
            Provider.Update(unitOfWork, this);
        }

        /// <summary>
        /// Delete the current record
        /// </summary>
        /// <remarks>
        /// Shortcut for Delete((UnitOfWork) null)
        /// </remarks>
        public void Delete()
        {
            Delete((UnitOfWork) null);
        }

        /// <summary>
        /// Delete the current record
        /// </summary>
        /// <param name="unitOfWork">Reference to UnitOfWork</param>
        /// <remarks>
        /// Override this method if you need to extend the Delete-behaviour. UnitOfWork can be null.
        /// </remarks>
        public virtual void Delete(UnitOfWork unitOfWork)
        {
            Provider.Delete(unitOfWork, this);
        }

        #endregion
    }
}
