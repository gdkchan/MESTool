# MESTool
Bayonetta Text Editing tool

This tool will allow you to edit the texts inside the *.mes files on the Bayonetta game. It stores the text data, and an "extra" font aswell.
This extra font is mapped starting from the value 0x1000. It contains variated characters depending on the mes file. Usually it contains the space " " character at 0x1000, that is not visible at all on the texture.
The japanese mes files uses those textures to store all the Kanji characters.
The XML file contains the mapping of those characters.

Notes:
- Starting from version 0.4.0, IDD and WTB options have been removed. New IDD tool can be found [here](https://github.com/gdkchan/IDDTool).
- Version 0.5.0 adds support for the PC version. Both PC and WiiU will export then texture as raw WTB files, instead of converting to DDS.
- This tool will be removed soon cause the code become pretty messy already. I don't have plans to do a rewrite.