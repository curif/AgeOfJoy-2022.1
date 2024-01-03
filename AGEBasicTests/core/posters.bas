10 REM Program to replace posters in the room in random order
11 REM just change the path where you saved the poster pictures.
13 REM You will need to execute it again every time you enter a room, 
14 REM   because posters doesn't saves configuration.

20 LET path = CombinePath(ConfigPath(), "posters") 'new posters
30 LET roomPostersCount = PosterRoomCount()
40 LET files = getFiles(path, ":", 1)  ' Random order
50 LET posters = CountMembers(files, ":")
60 LET minPosters = MIN(roomPostersCount, posters)

70 CLS
80 FOR i = 0 TO minPosters - 1
90     LET imageName = GetMember(files, i, ":")
100    LET imagePath = CombinePath(path, imageName)
110    print 0, 2 + MOD(i, 10), "#" + str(i) + " by poster: " + imageName + "                 "
120    if  PosterRoomReplace(i, imagePath) = 0 then goto 150
130 NEXT i

140 END

150 cls
160 print 0,0, "error replacing poster", 1
170 print 0,1, "poster: " + imageName
180 print 0,2, "path: " + imagePath
190 show

200 END
