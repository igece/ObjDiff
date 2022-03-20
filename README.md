# ObjDiff

[![nuget](https://img.shields.io/nuget/v/ObjDiff.svg)](https://www.nuget.org/packages/ObjDiff)
[![nuget](https://img.shields.io/nuget/dt/ObjDiff.svg)](https://www.nuget.org/packages/ObjDiff)
[![Build](https://github.com/igece/ObjDiff/actions/workflows/CI_build.yml/badge.svg)](https://github.com/igece/ObjDiff/actions/workflows/CI_build.yml)
[![Tests](https://github.com/igece/ObjDiff/actions/workflows/CI_Tests.yml/badge.svg)](https://github.com/igece/ObjDiff/actions/workflows/CI_Tests.yml)

A C# .NET Standard library that allows to obtain the differences between two objects using reflection.

Optionally, these differences can be used to patch the first object so it becomes equal to the second one.
This patching feature is specially useful when updating Entity Framework entities as it allows to update only those properties that have really changed.

## Installation

Using NuGet package manager console:

```
Install-Package ObjDiff
```

Using .NET CLI:

```
dotnet add package ObjDiff
```

## Features

* Compares arrays and collections
* Compares children objects
* Configuration options to ignore specific elements
* Patch an object with an obtained set of differences

## Current Limitations

This library is still in an early stage of development and so it lacks some features that similar libraries can offer:

* Only allows to compare objects of the same type
* Only public properties are compared

The above limitations may be addressed in future versions.

## Usage

### Obtaining Differences

``` csharp
using ObjDiff;

var object1 = new CustomObject();
var object2 = new CustomObject();

IEnumerable<Difference> differences = ObjDiff.Diff(object1, object2);
```

The last line can also be expressed through the use of the `Diff` extension method:

``` csharp
IEnumerable<Difference> differences = object1.Diff(object2);
```

The `Diff` method returns a list with all the differences found between `object1` and `object2` instances. Each one of these differences are represented through an instance of the `Difference` class:

``` csharp
  public class Difference
  {
      /// Full path of the element were values differ between compared objects.
      public string Path { get; private set; }

      /// Value of the object being compared.
      public object LeftValue { get; private set; }

      /// Value of the object against LeftValue is being compared.
      public object RightValue { get; private set; }
  }
```

### Comparison Options

It is possible to customize how the comparison process will perform:

``` csharp
var differences = object1.Diff(object2, new CompareOptions { MaxDepth = 10 });
```

The available options are:

``` csharp
public class CompareOptions
{
    /// When comparing arrays/collections, items must be in the same order. Default value is true.
    public bool CollectionsSameOrder { get; set; }

    /// Ignore properties marked with any of the specified attribute names. None set by default.
    public string[] IgnoredAttributes { get; set; }

    /// Ignore properties named with any of the specified values. None set by default.
    public string[] IgnoredProperties { get; set; }

    /// Maximum number of children levels to dive into. Default value is 10.
    public uint MaxDepth { get; set; }    
}
```

### Patching

Once we have the differences between two objects, we can apply them (patch) to the original object being compared. After patching it, both objects should be equivalent.

``` csharp
object1.Patch(differences);
Assert.Equal(object1, object2);
```

Both `Diff` and `Patch` operations can be executed through a simple call:

``` csharp
ObjDiff.MakeEqual(object1, object2);
```

Or using `MakeEqualTo` extension method:

``` csharp
object1.MakeEqualTo(object2);
```
