The FreeSpace ReadMe file


Introduction
============
FreeSpace is a program that will list the amount of free space available on your disk drives that
are in Ready status and match the drive types specified. The amount of free space will be written
to a log file that you specify.


Using FreeSpace
===============
When FreeSpace is first run you will notice that the log file name has defaulted to FreeSpace.log
on the desktop. Change the directory and filename to something that works for you. Below that on
the left are drive types. Select each type that you wish to report on. To the right are options
for how the log file is formatted. Select the number of decimal places, the format of the timestamp,
whether you want to use 1000^3 or 1024^3 to be used as the definition of GB, and if you want the
timestamp surrounded by square brackets.

The Log File menu contains an item to test the logging to check formatting, etc. There is also an
entry to view the specified log file.


Automating FreeSpace
====================
The File menu has a selection to start the Window Task Scheduler. When setting up a task, supply the
full path to FreeSpace.exe and then specify /write as an argument. Doing so will cause FreeSpace to
write the free space information to the log file and exit without showing the FreeSpace window.


Notices and License
===================
FreeSpace was written in C# by Tim Kennedy.

FreeSpace uses the following icons & packages:

Json.net v12.0.3 from Newtonsoft https://www.newtonsoft.com/json

NLog v4.7.7 https://nlog-project.org/


MIT License
Copyright (c) 2021 Tim Kennedy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject
to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
