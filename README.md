# Exist-Spoon #

This app submits completed tasks from [AbstractSpoon ToDoList](http://abstractspoon.weebly.com/) .TDL task list files to [exist.io](http://exist.io).

It reads the .TDL and corresponding .done.TDL file for tasks completed on the current day and updates the tasks_completed attribute.

## Authentication ##

On the first run, the tool will open your browser to authenticate with exist.io. The authentication token is saved to your temp folder for use in future runs.

## Configuration ##

To configure this app as a user-defined tool in ToDoList:

1. Download the latest release from the [releases](https://github.com/ajryan/exist-spoon/releases) page. Unzip it.
2. Tools > Preferences
3. User Defined Tools
4. New Tool
5. Enter tool name (e.g. exist.io)
6. Path: `C:\your\folder\exist-spoon.exe`
7. Arguments: `"$(pathname)"`
