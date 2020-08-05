
# Garbage Collector

.NET's garbage collector manages the allocation and release of memory for your application. Each time you create a new object, the common language runtime allocates memory for the object from the managed heap. As long as address space is available in the managed heap, the runtime continues to allocate space for new objects. However, memory is not infinite. Eventually the garbage collector must perform a collection in order to free some memory. The garbage collector's optimizing engine determines the best time to perform a collection, based upon the allocations being made. When the garbage collector performs a collection, it checks for objects in the managed heap that are no longer being used by the application and performs the necessary operations to reclaim their memory.

[Reference](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)

## Benefits

The garbage collector provides the following benefits:

- Frees developers from having to manually release memory.

- Allocates objects on the managed heap efficiently.

- Reclaims objects that are no longer being used, clears their memory, and keeps the memory available for future allocations. Managed objects automatically get clean content to start with, so their constructors don't have to initialize every data field.

- Provides memory safety by making sure that an object cannot use the content of another object.

[Reference](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals)

# Memory release

The garbage collector's optimizing engine determines the best time to perform a collection based on the allocations being made. When the garbage collector performs a collection, it releases the memory for objects that are no longer being used by the application. 

It determines which objects are no longer being used by examining the application's roots. An application's roots include static fields, local variables and parameters on a thread's stack, and CPU registers. Each root either refers to an object on the managed heap or is set to null.

The garbage collector has access to the list of active roots that the just-in-time (JIT) compiler and the runtime maintain. Using this list, the garbage collector creates a graph that contains all the objects that are reachable from the roots.

[Reference](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals)