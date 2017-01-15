# CodeCaster.SerializeThis

## Introduction

SerializeThis is a Visual Studio Extension. It lets you generate an example JSON for a class by right-clicking a type name. This can be helpful to generate example JSON to use in unit tests or through a REST client such as [Postman](https://www.getpostman.com/).

This is not meant as a replacement for documentation and client generators such as [Swagger](http://swagger.io/).

It currently looks like this:

![SerializeThis Screenshot](./static/images/typeserialization.png)

The extension does not yet output anything. Perhaps it could copy the serialized object to the clipboard, or open a new tab in the text editor and write the serialized object into it. 

## Building and Running

This project is a Visual Studio Extension, so you'll need to install the [Visual Studio SDK](https://msdn.microsoft.com/en-us/library/mt683786.aspx) in order to compile it. 

If you want to run the project, set the CodeCaster.SerializeThis as startup project, and in the project's Properties, on the Debug tab, set "Start external program" to Visual Studio ("C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe" for Visual Studio 2015) and set the Command line arguments to `/rootSuffix Exp`, so the Experimental Instance will be started. 

