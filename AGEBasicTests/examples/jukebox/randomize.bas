001  'In-Game Music Playback Control by crusher124

002  'This program will create a random playlist of all your music files 

100  LET volume = 0  'set beginning volume to default
105  CALL MusicClear()  'clear music queue

110  LET playlist = GetFiles(MusicPath(), ":", 1)  'create random (all files) playlist
125  CALL MusicAddList(playlist, ":")  'add the playlist to queue
130  CALL MusicLoop(1)  'auto-loop queue, set to (0) for no looping
135  CALL MusicReset()  'start queue playback
420  END  'exit Music_Playback_Control.bas