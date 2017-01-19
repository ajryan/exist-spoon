# Exist-Spoon #

This app submits completed tasks from [AbstractSpoon ToDoList](http://abstractspoon.weebly.com/) .TDL task list files to [exist.io](http://exist.io).

It reads the .TDL and corresponding .done.TDL file for tasks completed on the current day and updates the tasks_completed attribute.

This is a .NET Core application. Install the .NET core runtime and then execute the application with the commandline

    dotnet run <path to exist-spoon.dll> <path to .TDL tasklist>

## Authentication ##

On the first run, the tool will open your browser to authenticate with exist.io. The authentication token is saved to your temp folder for use in future runs.

## Configuration ##

To configure this app as a user-defined tool in ToDoList:

1. Tools > Preferences
2. User Defined Tools
3. New Tool
4. Enter tool name (e.g. exist.io)
4. Path: `C:\Program Files\dotnet\dotnet.exe`
5. Arguments: `<path to exist-spoon.dll> "$(pathname)"`