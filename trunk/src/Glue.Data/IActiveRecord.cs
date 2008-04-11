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
        void Insert();

        /// <summary>
        /// Update the current record
        /// </summary>
        void Update();

        /// <summary>
        /// Delete the current record
        /// </summary>
        void Delete();
    }
}
