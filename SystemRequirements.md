# System requirements #

The Glue .dll's need a CLR (.Net or Mono) version 2.0 or higher.

## Building ##

To build Glue you need either Visual Studio or Nant (together with a suitable framework). We currently use Visual Studio 2005 to build most releases. Building may also be possible using Monodevelop. The solution file can be found in `src/Glue.sln`.

A second way to build uses `nant`. To build the complete project, from the src/ directory, run:

```
nant build
```

We use this method less often, so it will probably not work if you check out the latest unstable sources from Subversion.