10 REM jukebox
20 REM add all the files in /music to the music queue (random order)
30 let files = GetFiles(MusicPath(), ":", 1)
40 CALL MusicAddList(files, ":")
50 CALL MusicLoop(1)
60 CALL MusicPlay()
70 END

