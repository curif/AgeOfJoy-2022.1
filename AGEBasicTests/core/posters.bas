10 REM Program to replace posters in the room in random order
11 REM just change the path where you saved the poster pictures.
20 LET path = "C:\Users\curif\desarr\AgeOfJoy-2022.1\AGEBasicTests\core\posters"
30 LET roomPostersCount = PosterRoomCount()
40 LET files = getFiles(path, ":", 1)  ' Random order
50 LET posters = CountMembers(files, ":")
60 LET minPosters = MIN(roomPostersCount, posters)

80 FOR i = 0 TO minPosters - 1
90     LET imageName = GetMember(files, i, ":")
100    LET imagePath = path + "\" + imageName
110    call PosterRoomReplace(i, imagePath)
120 NEXT i

140 END
