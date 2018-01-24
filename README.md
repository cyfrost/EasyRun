
# EasyRun

[![build_status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com/cyfrost/EasyRun/releases/latest)
[![GitHub release](https://img.shields.io/badge/current%20release-v0.1-blue.svg)](https://github.com/cyfrost/EasyRun/releases/latest)
[![license](https://img.shields.io/badge/license-MIT-orange.svg)](https://github.com/cyfrost/easyrun/blob/master/LICENSE)
[![HitCount](http://hits.dwyl.com/cyfrost/easyrun.svg)](http://hits.dwyl.com/cyfrost/easyrun)

EasyRun has supercow powers! one-up your productivity using EasyRun shortcuts from your Run dialog box. 

## Download

Download the latest release from [here](https://github.com/cyfrost/EasyRun/releases/latest).

Compatible from Windows XP through 10.


## Screencaps

![img](https://i.imgur.com/f3yqTBi.png)          ![img](https://i.imgur.com/hmKrkpd.png)

![img](https://i.imgur.com/a9eQa43.png)


## Features


### Alias Manager

You can keep track (Add, remove, edit, start, and view details) of all the aliases being used by your computer under one-roof using the built-in Alias Manager feature.

To open the Alias Manager, click on "Manage Aliases" option at the bottom left corner on the EasyRun home screen.



### Trusted UAC (TUAC)

Have you ever wanted to ALWAYS run some programs/scripts as administrator (because the UAC prompt is annoying but you can't turn it off entirely [because it's a security feature](https://security.stackexchange.com/questions/601/how-is-uac-a-security-improvement))? EasyRun allows you to create a "Elevated launcher" for the programs you trust don't require UAC confirmation on every launch. It's like User Account Control (UAC) is turned off entirely (except it's not ;)). and the way this works is through simple use of a scheduled task (granted administrative privilege) which is triggered manually to run your program when you need it (via an alias you set-up earlier). I'm always trying to improve this feature to be more stable, efficient, and convenient to use.

To create a Trusted UAC shortcut, select "Add new Trusted UAC shortcut" from the EasyRun > Options menu.


 ## Build Instructions

Use Visual Studio 2012 or higher.


## License

Copyright (c) 2018 cyfrost.

Licensed under the [MIT License](https://github.com/cyfrost/EasyRun/blob/master/LICENSE).
