# UWPVolumeSample
Volume issue repro for UWP

This small sample will:
- Read the volume after creating a media player
- Query the volume on another thread untill it is non-zero and print elapsed time
- Depending on line 98 from MainViewModel.cs, try to write to the volume on another thread untill it can be successfully read back on the other thread
- Mute and unmute the player when hitting the button. This will "register" the volume to it's actual value
