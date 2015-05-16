# KerbalGit

KerbalGit is a Kerbal Space Program plugin that adds Git game state versioning.

__The plugin is very basic and doesn't have a UI. Don't install KerbalGit unless you are comfortable using Git.__ 

When first run, the plugin will set up a Git repository for the `saves/` directory. The `.git/` directory itself is placed in `GameData/KerbalGit/`. 
The plugin is invoked and commits changes on three events:

* autosave
* quicksave (F5)
* save in VAB/SPH

![KerbalGit commits](https://raw.githubusercontent.com/kria/KerbalGit/master/kerbalgit-commits.png)

## OS support

I have only tried KerbalGit on Windows. The plugin uses LibGit2Sharp 0.17 which relies on the the native library libgit2. 
A Windows binary (git2-06d772d.dll) is included, but separate [builds][0] are needed for Linux and OSX.

[0]: https://libgit2.github.com/docs/guides/build-and-link/

## Installation

1. Get the latest [release][1].
2. Copy `GameData/KerbalGit/` to your KSP directory.
3. Copy `git2-06d772d.dll` (on Windows) to the root of your KSP directory.

[1]: https://github.com/kria/KerbalGit/releases

## Configuration

You can exclude any files and directories that you don't want to have under version control in `saves/.gitignore`.
In `GameData/KerbalGit/settings.cfg`, you can set the minimum interval (in seconds) between commits and the committer's name and email.

## License

Copyright (C) 2015 Kristian Adrup

KerbalGit is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. See included file [COPYING](COPYING) for details.
