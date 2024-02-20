10 let volGame = AudioGameGetVolume()
20 let volAmbience = AudioAmbienceGetVolume()

25 ' this is not noticeable, you cant ear it.
30 for vol = 1 to 5
40   call AudioAmbienceSetVolume(volAmbience + vol)
50 next vol

60 REM back no previous 
70 call AudioAmbienceSetVolume(volAmbience)

80 REM mute ambience audio
90 call AudioAmbienceSetVolume(-80)
100 if AudioAmbienceGetVolume() != -80 then let error = "ambience not muted"
