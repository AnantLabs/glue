## Download ##

### Download binary: ###

Download the latest version (currently [Glue-1.2.1.zip](http://glue.googlecode.com/files/Glue-1.2.1.zip)) from the [downloads](http://code.google.com/p/glue/downloads/list) page.

### Download source: ###

Download the latest source code distribution (currently [Glue-src-1.2.1.zip](http://glue.googlecode.com/files/Glue-src-1.2.1.zip)) from the [downloads](http://code.google.com/p/glue/downloads/list) page.

### Checkout from subversion: ###

Use this command to checkout the latest revision from the subversion repository:

`svn checkout http://glue.googlecode.com/svn/trunk/ glue`

## Build ##

The source distribution includes the Visual Studio solution (.sln) file in /src. If you cannot build with Visual Studio, you can build with nant with the command

` nant build `

The dll's will be placed in the /bin directory.

## Use ##

To use glue, reference the .dll's you need in your solution. For source distributions, you may also include the glue projects to your solution (by adding the .csproj files), and reference those projects.

To get started with glue, look at some example code in /samples, look at the Glue.Data [tutorial](GlueDataHowto.md), and don't forget to bookmark the [API](http://www.glueproject.com/api/)!