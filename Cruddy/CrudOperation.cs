namespace Cruddy;

/// <summary>
/// Represents the CRUD operations that can be performed on a data entity. 
/// This enum is used to specify the type of operation being performed, 
/// such as creating, reading, or updating an entity.
/// </summary>
public enum CrudOperation
{
    /// <summary>
    /// Represents no CRUD operation. This is the default value.
    /// </summary>
    None = 0,

    /// <summary>
    /// Represents the creation of a new entity. This operation is used when adding a new record to the database or data source.
    /// </summary>
    Create,

    /// <summary>
    /// Represents the reading or retrieval of an existing entity. This operation is used when fetching data from the database or data source.
    /// </summary>
    Read,

    /// <summary>
    /// Represents the updating of an existing entity. This operation is used when modifying an existing record in the database or data source.
    /// </summary>
    Update,

    /// <summary>
    /// Represents the deletion of an existing entity. This operation is used when removing a record from the database or data source.
    /// </summary>
    Delete
}
