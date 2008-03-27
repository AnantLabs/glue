using System;
using System.Collections.Generic;
using System.Text;

namespace Glue.Data
{
    public interface IActiveRecord
    {
        /// <summary>
        /// Return the MappingProvider for this class
        /// </summary>
        /// <remarks>
        /// The MappingProvider can to be declared in the Table-attribute.
        ///
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
        IMappingProvider Provider { get; }


        /// <summary>
        /// Insert the current record
        /// </summary>
        /// <remarks>
        /// Shortcut for Insert((UnitOfWork) null)
        /// </remarks>
        void Insert();

        /// <summary>
        /// Insert the current record
        /// </summary>
        /// <param name="unitOfWork">Reference to UnitOfWork</param>
        /// <remarks>
        /// Override this method if you need to extend the Insert-behaviour. UnitOfWork can be null.
        /// </remarks>
        void Insert(UnitOfWork unitOfWork);

        /// <summary>
        /// Update the current record
        /// </summary>
        /// <remarks>
        /// Shortcut for Update((UnitOfWork) null)
        /// </remarks>
        void Update();

        /// <summary>
        /// Update the current record
        /// </summary>
        /// <param name="unitOfWork">Reference to UnitOfWork</param>
        /// <remarks>
        /// Override this method if you need to extend the Update-behaviour. UnitOfWork can be null.
        /// </remarks>
        void Update(UnitOfWork unitOfWork);

        /// <summary>
        /// Delete the current record
        /// </summary>
        /// <remarks>
        /// Shortcut for Delete((UnitOfWork) null)
        /// </remarks>
        void Delete();

        /// <summary>
        /// Delete the current record
        /// </summary>
        /// <param name="unitOfWork">Reference to UnitOfWork</param>
        /// <remarks>
        /// Override this method if you need to extend the Delete-behaviour. UnitOfWork can be null.
        /// </remarks>
        void Delete(UnitOfWork unitOfWork);
    }
}
