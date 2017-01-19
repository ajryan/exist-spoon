# Exist-Spoon #

This app submits completed tasks from [AbstractSpoon ToDoList](http://abstractspoon.weebly.com/) .TDL task list files to [exist.io](http://exist.io).

It reads the .TDL and corresponding .done.TDL file for tasks completed on the current day and updates the tasks_completed attribute.

This is a .NET Core application. Install the .NET core runtime and then execute the application with the commandline

    dotnet run <path to exist-spoon.dll> <path to .TDL tasklist>