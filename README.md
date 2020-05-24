
# VMix - WIP
Virtual Mixer Control Software
## Overview
VMix is intended as a flexible remote control for midi enabled digital mixers such as the Yamaha 01v96i, the Behringer X32, etc. It provides a sleek interface built on top of WPF and .NET to control these mixers. VMix features a flexible UI system as well as an extensible midi translator allowing it to support mixers with varying features.  
Another key feature is that of flexible scene storage and playback to allow for ultimate flexibility in creating scenes. It allows for multiple timelines of scenes (each targeting different parameters) to be executed in parallel, which makes modifying scenes easier and removes the hassle of "limited recall safe scene managers".
VMix also adds 8 virtual DCAs (fader groups) which can be freely assigned and stored in scenes. This adds additional functionality to older mixers while making it easier to pre-program shows without physical access to the mixing console.
![Screenshot](https://github.com/space928/VMix/raw/master/VMix%20Main.png)
## Features
 - Mixer support:
   - Yamaha 02r
 - Scene manager - WIP
 - Fader panel
 - Selected channel processing editor - WIP
 - Midi support
 - Dark theme
## Installation
For non-commercial use please feel free to download the binaries from the releases tab. The application is portable and requires no additional installation.
## Building
Using Microsoft Visual Studio 2019, the application can be built from the provided solution file.
### Dependencies
These can be downloaded using nuget using the included package manifest.
  - Newtonsoft.JSON
  - Commons.Music.Midi
## Contributing
Feel free to submit issues containing suggestions and bugs.
Currently code contributions are not accepted in this project.
## License
This software is free to look at, modify, and use non-commercially. To use this software in a commercial manner, please contact the developer to purchase a license (honestly it won't be very expensive).
For the full conditions see: LICENSE.md
