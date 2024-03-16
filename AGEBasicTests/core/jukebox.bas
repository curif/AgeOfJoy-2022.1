10 REM jukebox
20 REM add all the files in /music to the music queue (random order)
30 let files = GetFiles(MusicPath(), ":", 1)
40 CALL MusicClear()
50 CALL MusicAddList(files, ":")
60 CALL MusicLoop(1)
70 CALL MusicPlay()

80 CALL AudioMusicSetVolume(0)
90 CALL AudioAmbienceSetVolume(-5)

100 END

